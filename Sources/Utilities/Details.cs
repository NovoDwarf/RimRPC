using System.Globalization;
using RimRPC.RPC;
using Verse;

namespace RimRPC.Utilities;

internal static class Details
{
    public static string GetDetails(RpcData data)
    {
        var settings = RimRpc.Settings;
     
        if (settings.RpcCustomTop)
            return settings.RpcCustomTopText;

        if (!settings.RpcColonistCount)
            return settings.RpcColony ? data.Colony : null;

        var colonists = data.Colonists.ToString(CultureInfo.InvariantCulture);
        return settings.RpcColony
            ? $"{data.Colony} ({colonists})"
            : colonists + " " + "RPC_Colonists".Translate();
    }
}
