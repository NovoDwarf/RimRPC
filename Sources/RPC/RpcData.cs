namespace RimRPC.RPC;

internal sealed record RpcData(
    string Colony,
    int Colonists,
    int Year,
    int Days,
    int Hour,
    string Quadrum,
    string Biome,
    bool HasLocalTime);