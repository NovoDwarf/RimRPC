using System;
using System.Linq;
using System.Text;
using RimRPC.Core;
using Verse;
using RimRPC.Utilities;
using Xunit;

namespace RimRPC.Tests;

public sealed class SettingsAndFormattingTests
{
    private static readonly RpcData Colony = new("Colony", 3, 5502, 120, 12, "Quadrum", "Forest", true);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyApplicationUsesDefault(string input)
    {
        Assert.True(RpcIdentity.TryResolve(input, out var id));
        Assert.Equal(RpcIdentity.Default, id);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-123")]
    [InlineData("123 456")]
    [InlineData("18446744073709551616")]
    public void InvalidApplicationIsRejected(string input)
    {
        Assert.False(RpcIdentity.TryResolve(input, out _));
    }

    [Fact]
    public void ApplicationIdIsTrimmed()
    {
        Assert.True(RpcIdentity.TryResolve(" 123456789012345678 ", out var id));
        Assert.Equal("123456789012345678", id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void InvalidPeriodHasSafeDefault(float value)
    {
        var settings = new RimRpcSettings { RpcPeriod = value };
        settings.Normalize();
        Assert.Equal(20f, settings.RpcPeriod);
    }

    [Fact]
    public void LegacySettingsMigrateAndNewFieldsKeepDefaults()
    {
        Scribe_Values.Values.Clear(); Scribe.mode = LoadSaveMode.LoadingVars;
        Scribe_Values.Values["RPC_Period"] = 3000f;
        Scribe_Values.Values["RPC_CustomBottom"] = true;
        Scribe_Values.Values["RPC_CustomBottomText"] = "Bottom";
        var settings = new RimRpcSettings(); settings.ExposeData();
        Assert.Equal(50f, settings.RpcPeriod);
        Assert.True(settings.RpcEnabled);
        Assert.Equal("logo", settings.RpcLargeImageKey);
        Assert.Equal("", settings.RpcApplicationId);
        Assert.True(settings.RpcCustomBottom); Assert.False(settings.RpcCustomTop);
        Assert.Equal("Bottom", settings.RpcCustomBottomText);
        Scribe.mode = LoadSaveMode.Inactive;
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(999, 120)]
    public void PeriodIsClampedToSupportedRange(float value, float expected)
    {
        var settings = new RimRpcSettings { RpcPeriod = value };
        settings.Normalize();
        Assert.Equal(expected, settings.RpcPeriod);
    }

    [Fact]
    public void LegacyZeroIsRepairedAndMissingPeriodUsesLegacyDefault()
    {
        Scribe_Values.Values.Clear(); Scribe.mode = LoadSaveMode.LoadingVars;
        var missing = new RimRpcSettings(); missing.ExposeData();
        Assert.InRange(missing.RpcPeriod, 20.8f, 20.9f);
        Scribe_Values.Values["RPC_Period"] = 0f;
        var zero = new RimRpcSettings(); zero.ExposeData();
        Assert.Equal(20, zero.RpcPeriod);
        Scribe.mode = LoadSaveMode.Inactive;
    }

    [Fact]
    public void NewSettingsRoundTripWithoutLegacyPeriodOverridingThem()
    {
        Scribe_Values.Values.Clear(); Scribe.mode = LoadSaveMode.Saving;
        var settings = new RimRpcSettings { RpcPeriod = 45, RpcEnabled = false, RpcApplicationId = "123", RpcLargeImageKey = "custom" };
        settings.ExposeData();
        Scribe_Values.Values["RPC_Period"] = 6000f;
        Scribe.mode = LoadSaveMode.LoadingVars;
        var loaded = new RimRpcSettings(); loaded.ExposeData();
        Assert.Equal(45, loaded.RpcPeriod); Assert.False(loaded.RpcEnabled);
        Assert.Equal("123", loaded.RpcApplicationId); Assert.Equal("custom", loaded.RpcLargeImageKey);
        Scribe.mode = LoadSaveMode.Inactive;
    }

    [Fact]
    public void ResetKeepsSettingsObjectAndRestoresEveryField()
    {
        var settings = new RwrpcSettings { RpcEnabled = false, RpcApplicationId = "123", RpcLargeImageKey = "custom", RpcPeriod = 99, RpcCustomBottom = true };
        settings.ResetToDefaults();
        var defaults = new RimRpcSettings();
        foreach (var field in typeof(RimRpcSettings).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            Assert.Equal(field.GetValue(defaults), field.GetValue(settings));
        Assert.IsType<RwrpcSettings>(settings);
    }

    [Fact]
    public void HourIsIndependentAndUnknownLocationOmitsLocalFields()
    {
        RimWorldRPCMod.Settings = new RimRpcSettings { RpcDay = false, RpcQuadrum = false };
        Assert.Equal("12RPC_Hour", States.GetState(Colony));
        RimWorldRPCMod.Settings.RpcQuadrum = true;
        Assert.Equal("", States.GetState(Colony with { HasLocalTime = false }));
    }

    [Fact]
    public void TypedFieldsProduceCorrectSeparators()
    {
        RimWorldRPCMod.Settings = new RimRpcSettings { RpcDay = false, RpcHour = false, RpcQuadrum = false, RpcYear = true, RpcBiome = true, RpcColonistCount = true };
        Assert.Equal("5502RPC_ShortYears | Forest", States.GetState(Colony));
        Assert.Equal("Colony (3)", Details.GetDetails(Colony));
    }

    [Theory]
    [InlineData("Ж", 200)]
    [InlineData("🐱", 100)]
    [InlineData("a", 500)]
    public void PreviewAndTransportShareUtf8Truncation(string symbol, int repeat)
    {
        var value = string.Concat(Enumerable.Repeat(symbol, repeat));
        var payload = new RpcPayload { Details = value, State = value, LargeImageKey = " custom " }.Normalize();
        Assert.Equal(128, Encoding.UTF8.GetByteCount(payload.Details));
        Assert.Equal(payload.Details, payload.State);
        Assert.Equal("custom", payload.LargeImageKey);
        Assert.False(char.IsHighSurrogate(payload.Details[^1]));
    }

    [Fact]
    public void CustomTextOverridesFieldsAndWhitespaceIsCleared()
    {
        RimWorldRPCMod.Settings = new RimRpcSettings { RpcCustomTop = true, RpcCustomTopText = "Top", RpcCustomBottom = true, RpcCustomBottomText = " " };
        Assert.Equal("Top", Details.GetDetails(Colony));
        Assert.Null(new RpcPayload { State = States.GetState(Colony) }.Normalize().State);
    }
}
