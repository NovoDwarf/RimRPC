using RimRPC.RPC;
using RimRPC.Utilities;
using RimWorld;
using Verse;

namespace RimRPC.Runtime;

public static class StateHandler
{
    internal static RpcPayload CreatePresence()
    {
        var settings = RimRpc.Settings;
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

        var map = Current.Game.CurrentMap ?? Find.Maps.FirstOrDefault(candidate => candidate.IsPlayerHome);
        var longitude = map == null ? 0f : Find.WorldGrid.LongLatOf(map.Tile).x;
        var ticks = Find.TickManager.TicksAbs;
       
        var data = new RpcData(
            Colony: Faction.OfPlayer?.Name ?? string.Empty,
            Colonists: PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count,
            Year: GenDate.Year(ticks, longitude), 
            Days: GenDate.DaysPassedSinceSettle,
            Hour: GenDate.HourOfDay(ticks, longitude), 
            Quadrum: GenDate.Quadrum(ticks, longitude).Label(),
            Biome: Current.Game.CurrentMap?.Biome?.LabelCap.ToString() ?? string.Empty,
            HasLocalTime: map != null);

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
            RpcService.UpdatePresence(CreatePresence());
    }
}
