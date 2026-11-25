using System;
using System.Linq;
using System.Text;
using RimRPC.RPC;
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
    public void MissingPeriodUsesDefault()
    {
        Scribe_Values.Values.Clear();
        Scribe.mode = LoadSaveMode.LoadingVars;
        var settings = new RimRpcSettings();

        settings.ExposeData();

        Assert.Equal(RimRpcSettings.DefaultPeriod, settings.RpcPeriod);
        Scribe.mode = LoadSaveMode.Inactive;
    }

    [Fact]
    public void SettingsRoundTrip()
    {
        Scribe_Values.Values.Clear(); Scribe.mode = LoadSaveMode.Saving;
        var settings = new RimRpcSettings { RpcPeriod = 45, RpcEnabled = false, RpcApplicationId = "123", RpcLargeImageKey = "custom" };
        settings.ExposeData();
        Scribe.mode = LoadSaveMode.LoadingVars;
        var loaded = new RimRpcSettings(); loaded.ExposeData();
        Assert.Equal(45, loaded.RpcPeriod); Assert.False(loaded.RpcEnabled);
        Assert.Equal("123", loaded.RpcApplicationId); Assert.Equal("custom", loaded.RpcLargeImageKey);
        Scribe.mode = LoadSaveMode.Inactive;
    }

    [Fact]
    public void ResetKeepsSettingsObjectAndRestoresEveryField()
    {
        var settings = new RimRpcSettings { RpcEnabled = false, RpcApplicationId = "123", RpcLargeImageKey = "custom", RpcPeriod = 99, RpcCustomBottom = true };
        settings.ResetToDefaults();
        var defaults = new RimRpcSettings();
        foreach (var field in typeof(RimRpcSettings).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            Assert.Equal(field.GetValue(defaults), field.GetValue(settings));
    }

    [Fact]
    public void HourIsIndependentAndUnknownLocationOmitsLocalFields()
    {
        RimRpc.Settings = new RimRpcSettings { RpcDay = false, RpcQuadrum = false };
        Assert.Equal("12RPC_Hour", States.GetState(Colony));
        RimRpc.Settings.RpcQuadrum = true;
        Assert.Equal("", States.GetState(Colony with { HasLocalTime = false }));
    }

    [Fact]
    public void TypedFieldsProduceCorrectSeparators()
    {
        RimRpc.Settings = new RimRpcSettings { RpcDay = false, RpcHour = false, RpcQuadrum = false, RpcYear = true, RpcBiome = true, RpcColonistCount = true };
        Assert.Equal("5502RPC_ShortYears | Forest", States.GetState(Colony));
        Assert.Equal("Colony (3)", Details.GetDetails(Colony));
    }

    [Theory]
    [InlineData("Ж", 200)]
    [InlineData("🐱", 100)]
    [InlineData("a", 500)]
    public void PresenceTextUsesUtf8Truncation(string symbol, int repeat)
    {
        var value = string.Concat(Enumerable.Repeat(symbol, repeat));
        var payload = new RpcPayload { Details = value, State = value, LargeImageKey = " custom " }.Normalize();
        Assert.Equal(128, Encoding.UTF8.GetByteCount(payload.Details));
        Assert.Equal(payload.Details, payload.State);
        Assert.Equal("custom", payload.LargeImageKey);
        Assert.False(char.IsHighSurrogate(payload.Details[^1]));
    }

    [Theory]
    [InlineData(false, false, null)]
    [InlineData(true, false, "Colony")]
    [InlineData(false, true, "3 RPC_Colonists")]
    [InlineData(true, true, "Colony (3)")]
    public void DetailsRespectColonyAndColonistSettings(bool showColony, bool showColonists, string expected)
    {
        RimRpc.Settings = new RimRpcSettings
        {
            RpcColony = showColony,
            RpcColonistCount = showColonists
        };

        var result = Details.GetDetails(Colony);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CustomTextOverridesFieldsAndWhitespaceIsCleared()
    {
        RimRpc.Settings = new RimRpcSettings { RpcCustomTop = true, RpcCustomTopText = "Top", RpcCustomBottom = true, RpcCustomBottomText = " " };
        Assert.Equal("Top", Details.GetDetails(Colony));
        Assert.Null(new RpcPayload { State = States.GetState(Colony) }.Normalize().State);
    }
}
