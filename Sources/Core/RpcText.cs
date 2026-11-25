using System.Text;

namespace RimRPC.Core;

internal static class RpcText
{
    public static string Limit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var result = new StringBuilder();
        var bytes = 0;

        foreach (var rune in value.EnumerateRunes())
        {
            if (bytes + rune.Utf8SequenceLength > 128)
                break;

            result.Append(rune.ToString());

            bytes += rune.Utf8SequenceLength;
        }

        return result.ToString();
    }
}
