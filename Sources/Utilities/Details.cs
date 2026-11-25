using RimRPC.Core;
using Verse;

namespace RimRPC.Utilities;

internal static class Details
{
    public static string GetDetails(RpcData data)
    {
        var colonyname = data.Colony;
        var colonistnumber = data.Colonists.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (RimWorldRPCMod.Settings.RpcCustomTop)
            return RimWorldRPCMod.Settings.RpcCustomTopText;

        if (RimWorldRPCMod.Settings.RpcColony)
        {
            if (RimWorldRPCMod.Settings.RpcColonistCount)
            {
                return colonyname + " (" + colonistnumber + ")";
            }

            return colonyname;
        }

        if (RimWorldRPCMod.Settings.RpcColonistCount)
        {
            return colonistnumber + " " + "RPC_Colonists".Translate();
        }

        return null;
    }
}
