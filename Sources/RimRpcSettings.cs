using System;
using Verse;

namespace RimRPC;

public class RimRpcSettings : ModSettings
{
    public const float DefaultPeriod = 20f;
    public const float MinimumPeriod = 5f;
    public const float MaximumPeriod = 120f;

    // Real seconds
    public float RpcPeriod = DefaultPeriod;

    public bool RpcEnabled = true;
    public string RpcLargeImageKey = "logo";
    public string RpcSmallImageKey = "inmap";

    public bool RpcColony = true;
    public bool RpcColonistCount;
    public bool RpcYear;
    public bool RpcYearShort = true;
    public bool RpcQuadrum = true;
    public bool RpcDay = true;
    public bool RpcHour = true;
    public bool RpcTime = true;
    public bool RpcBiome;
    public bool RpcCustomTop;
    public bool RpcCustomBottom;

    public string RpcApplicationId = string.Empty;

    public string RpcCustomTopText = "Example";
    public string RpcCustomBottomText = "Example";

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref RpcPeriod, "RPC_UpdateIntervalSeconds", -1f);
        if (Scribe.mode == LoadSaveMode.LoadingVars && RpcPeriod == -1f)
        {
            var legacyTicks = 1250f;
            Scribe_Values.Look(ref legacyTicks, "RPC_Period", 1250f);
            RpcPeriod = legacyTicks / 60f;
        }
        Normalize();

        Scribe_Values.Look(ref RpcEnabled, "RPC_Enabled", true);
        Scribe_Values.Look(ref RpcLargeImageKey, "RPC_LargeImageKey", "logo");
        Scribe_Values.Look(ref RpcSmallImageKey, "RPC_SmallImageKey", "inmap");
        Scribe_Values.Look(ref RpcColony, "RPC_Colony", true);
        Scribe_Values.Look(ref RpcColonistCount, "RPC_ColonistCount");
        Scribe_Values.Look(ref RpcBiome, "RPC_Biome");
        Scribe_Values.Look(ref RpcYear, "RPC_Year");
        Scribe_Values.Look(ref RpcYearShort, "RPC_YearShort", true);
        Scribe_Values.Look(ref RpcQuadrum, "RPC_Quadrum", true);
        Scribe_Values.Look(ref RpcDay, "RPC_Day", true);
        Scribe_Values.Look(ref RpcHour, "RPC_Hour", true);
        Scribe_Values.Look(ref RpcTime, "RPC_Time", true);
        Scribe_Values.Look(ref RpcCustomTop, "RPC_CustomTop");
        Scribe_Values.Look(ref RpcCustomBottom, "RPC_CustomBottom");
        Scribe_Values.Look(ref RpcApplicationId, "RPC_ApplicationId", string.Empty);
        Scribe_Values.Look(ref RpcCustomTopText, "RPC_CustomTopText", "Example");
        Scribe_Values.Look(ref RpcCustomBottomText, "RPC_CustomBottomText", "Example");
    }

    public void ResetToDefaults()
    {
        var defaults = new RimRpcSettings();
        foreach (var field in typeof(RimRpcSettings).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            field.SetValue(this, field.GetValue(defaults));
    }

    public void Normalize()
    {
        RpcPeriod = !float.IsFinite(RpcPeriod) || RpcPeriod <= 0
            ? DefaultPeriod
            : Math.Clamp(RpcPeriod, MinimumPeriod, MaximumPeriod);
    }
}

