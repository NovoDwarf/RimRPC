// Only the settings serializer and translation boundary are replaced, not the logic under test.
using System.Collections.Generic;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace RimRPC
{
    internal static class RimWorldRPCMod
    {
        public static RimRpcSettings Settings = new();
    }
}
namespace Verse
{
    public enum LoadSaveMode { Inactive, LoadingVars, Saving }
    public static class Scribe { public static LoadSaveMode mode; }
    public class ModSettings { public virtual void ExposeData() { } }
    public static class Translator { public static string Translate(this string text) => text; }
    public static class Scribe_Values
    {
        public static readonly Dictionary<string, object> Values = new();
        public static void Look<T>(ref T value, string key, T defaultValue = default)
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                value = Values.TryGetValue(key, out var stored) ? (T)stored : defaultValue;
            else if (Scribe.mode == LoadSaveMode.Saving)
                Values[key] = value;
        }
    }
}
