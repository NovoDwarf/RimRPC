using RimRPC.Core;
using RimRPC.Utilities;
using RimWorld;
using Verse;

namespace RimRPC.Runtime;

public class StateHandler
{
    internal static RpcPayload CreatePresence()
    {
        var settings = RimWorldRPCMod.Settings;
        var presence = new RpcPayload
        {
            LargeImageKey = settings.RpcLargeImageKey?.Trim(),
            LargeImageText = "RimWorld"
        };
        if (Current.ProgramState != ProgramState.Playing || Current.Game?.World == null)
        {
            presence.Details = "RPC_MainMenu".Translate();
            return presence.Normalize();
        }

        // World view uses a home map's location. Without one, local time is omitted.
        var map = Current.Game.CurrentMap ?? Find.Maps.FirstOrDefault(candidate => candidate.IsPlayerHome);
        var longitude = map == null ? 0f : Find.WorldGrid.LongLatOf(map.Tile).x;
        var ticks = Find.TickManager.TicksAbs;
        var data = new RpcData(Faction.OfPlayer?.Name ?? string.Empty,
            PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count,
            GenDate.Year(ticks, longitude), GenDate.DaysPassedSinceSettle,
            GenDate.HourOfDay(ticks, longitude), GenDate.Quadrum(ticks, longitude).Label(),
            Current.Game.CurrentMap?.Biome?.LabelCap.ToString() ?? string.Empty, map != null);

        presence.State = States.GetState(data);
        presence.Details = Details.GetDetails(data);
        presence.SmallImageKey = settings.RpcSmallImageKey?.Trim();
        presence.SmallImageText = "RPC_Playing".Translate();
        presence.StartTimestamp = settings.RpcTime ? RimRpcState.Started : 0;
        return presence.Normalize();
    }

    public static void PushState()
    {
        if (RimRpcState.IsRunning)
            RPC.DiscordRpcService.UpdatePresence(CreatePresence());
    }
}
