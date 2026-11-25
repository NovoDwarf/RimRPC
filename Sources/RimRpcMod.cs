using RimRPC.Runtime;
using UnityEngine;
using Verse;

namespace RimRPC;

public sealed class RimWorldRPCMod : Verse.Mod
{
    public static RimRpcSettings Settings;
    private string _draftApplicationId;
    private bool _applicationIdInvalid;
    private Vector2 _scrollPosition;
    private float _settingsHeight = 900f;

    public RimWorldRPCMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<RwrpcSettings>();
        _draftApplicationId = Settings.RpcApplicationId ?? string.Empty;
        LongEventHandler.ExecuteWhenFinished(RimRpcState.BootMeUp);
    }
    public override string SettingsCategory() => "RPC_CategoryLabel".Translate();

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();

        var viewRect = new Rect(0, 0, inRect.width - 20f, _settingsHeight);
        Widgets.BeginScrollView(inRect, ref _scrollPosition, viewRect);
        listingStandard.Begin(viewRect);
        listingStandard.Label(("RPC_State_" + RPC.DiscordRpcService.State).Translate());
        if (!string.IsNullOrEmpty(RPC.DiscordRpcService.LastError))
            listingStandard.Label("RPC_ConnectionError".Translate() + " " + RPC.DiscordRpcService.LastError);
        var wasEnabled = Settings.RpcEnabled;
        listingStandard.CheckboxLabeled("RPC_EnabledLabel".Translate(), ref Settings.RpcEnabled);
        if (wasEnabled != Settings.RpcEnabled)
            ApplyConnection();
        SetConnectionFields(listingStandard);

        SetDefaultSettings(listingStandard);
        SetCustomFields(listingStandard);
        var preview = StateHandler.CreatePresence();
        listingStandard.Label("RPC_Preview".Translate());
        listingStandard.Label(preview.Details ?? "RPC_EmptyLine".Translate().ToString());
        listingStandard.Label(preview.State ?? "RPC_EmptyLine".Translate().ToString());
        listingStandard.Label("RPC_PreviewHint".Translate());
        if (listingStandard.ButtonText("RPC_Reset".Translate()))
        {
            Settings.ResetToDefaults();
            _draftApplicationId = string.Empty;
            _applicationIdInvalid = false;
            ApplyConnection();
            StateHandler.PushState();
        }

        _settingsHeight = listingStandard.CurHeight + 20f;
        listingStandard.End();
        Widgets.EndScrollView();
    }

    private static void SetDefaultSettings(Listing_Standard listingStandard)
    {
        Settings.RpcPeriod = listingStandard.SliderLabeled(
            "RPC_Period".Translate() + " " + Settings.RpcPeriod.ToString("0") + " " + "RPC_Seconds".Translate(),
            Settings.RpcPeriod, RimRpcSettings.MinimumPeriod, RimRpcSettings.MaximumPeriod, 1);

        Settings.Normalize();

        listingStandard.CheckboxLabeled("RPC_ColonyLabel".Translate() + " ", ref Settings.RpcColony);
        listingStandard.CheckboxLabeled("RPC_ColonistCountLabel".Translate() + " ", ref Settings.RpcColonistCount);
        listingStandard.CheckboxLabeled("RPC_BiomeLabel".Translate() + " ", ref Settings.RpcBiome);
        listingStandard.CheckboxLabeled("RPC_YearLabel".Translate() + " ", ref Settings.RpcYear);
        listingStandard.CheckboxLabeled("RPC_YearShortLabel".Translate() + " ", ref Settings.RpcYearShort);
        listingStandard.CheckboxLabeled("RPC_QuadrumLabel".Translate() + " ", ref Settings.RpcQuadrum);
        listingStandard.CheckboxLabeled("RPC_DayLabel".Translate() + "  ", ref Settings.RpcDay);
        listingStandard.CheckboxLabeled("RPC_HourLabel".Translate() + "  ", ref Settings.RpcHour);
        listingStandard.CheckboxLabeled("RPC_TimeLabel".Translate() + "  ", ref Settings.RpcTime);
    }

    private void SetConnectionFields(Listing_Standard listingStandard)
    {
        var previous = _draftApplicationId;
        _draftApplicationId = listingStandard.TextEntryLabeled("RPC_ApplicationIdLabel".Translate(), _draftApplicationId);
        if (previous != _draftApplicationId)
            _applicationIdInvalid = false;
        listingStandard.Label("RPC_ApplicationIdHint".Translate());
        if (_applicationIdInvalid)
            listingStandard.Label("RPC_ApplicationIdInvalid".Translate());
        if (listingStandard.ButtonText("RPC_ApplyConnection".Translate()))
        {
            _applicationIdInvalid = !RimRpcState.TryResolveApplicationId(_draftApplicationId, out _);
            if (!_applicationIdInvalid)
            {
                Settings.RpcApplicationId = _draftApplicationId.Trim();
                ApplyConnection();
            }
        }
        Settings.RpcLargeImageKey = listingStandard.TextEntryLabeled("RPC_LargeImageKey".Translate(), Settings.RpcLargeImageKey ?? string.Empty);
        Settings.RpcSmallImageKey = listingStandard.TextEntryLabeled("RPC_SmallImageKey".Translate(), Settings.RpcSmallImageKey ?? string.Empty);
    }

    private static void ApplyConnection()
    {
        RimRpcState.ApplyApplicationId(force: true);
        Settings.Write();
        StateHandler.PushState();
    }

    private void SetCustomFields(Listing_Standard listingStandard)
    {
        listingStandard.CheckboxLabeled("CustomTopCheckbox".Translate() + "  ", ref Settings.RpcCustomTop);

        if (Settings.RpcCustomTop)
            Settings.RpcCustomTopText = listingStandard.TextEntryLabeled("CustomTopLabel".Translate() + " ", Settings.RpcCustomTopText);

        listingStandard.CheckboxLabeled("CustomBottomCheckbox".Translate() + "  ", ref Settings.RpcCustomBottom);

        if (Settings.RpcCustomBottom)
            Settings.RpcCustomBottomText = listingStandard.TextEntryLabeled("CustomBottomLabel".Translate() + " ", Settings.RpcCustomBottomText);


        if (!listingStandard.ButtonText("RPC_UpdateLabel".Translate()))
            return;

        Settings.Write();
        StateHandler.PushState();
    }
}