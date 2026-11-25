namespace RimRPC.Core;

internal sealed record RpcData(
    string Colony,
    int Colonists,
    int Year,
    int Days,
    int Hour,
    string Quadrum,
    string Biome,
    bool HasLocalTime);