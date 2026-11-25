using System.Globalization;

namespace RimRPC.Core;

internal static class RpcIdentity
{
    public const string Default = "428272711702282252";

    public static bool TryResolve(string customId, out string id)
    {
        id = string.IsNullOrWhiteSpace(customId)
            ? Default
            : customId.Trim();

        if (ulong.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
            return value != 0;

        return false;
    }
}
