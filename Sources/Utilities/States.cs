using System.Collections.Generic;
using RimRPC.RPC;
using Verse;

namespace RimRPC.Utilities;

internal static class States
{
    public static string GetState(RpcData data)
    {
        var settings = RimRpc.Settings;

        if (settings.RpcCustomBottom)
            return settings.RpcCustomBottomText;

        var parts = new List<string>();

        if (settings.RpcDay)
        {
            var day = $"{"RPC_Day".Translate()} {data.Days}";

            if (settings.RpcHour && data.HasLocalTime)
                day += $" ({data.Hour}{"RPC_Hour".Translate()})";

            parts.Add(day);
        }
        else if (settings.RpcHour && data.HasLocalTime)
            parts.Add($"{data.Hour}{"RPC_Hour".Translate()}");

        if (settings.RpcQuadrum && data.HasLocalTime)
            parts.Add(data.Quadrum);

        if (settings.RpcYear)
            parts.Add(settings.RpcYearShort
                ? $"{data.Year}{"RPC_ShortYears".Translate()}"
                : $"{"RPC_Years".Translate()} {data.Year}");

        if (settings.RpcBiome && !string.IsNullOrEmpty(data.Biome))
            parts.Add(data.Biome);

        return string.Join(" | ", parts);
    }
}
