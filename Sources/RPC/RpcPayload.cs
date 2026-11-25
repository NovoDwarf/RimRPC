namespace RimRPC.RPC;

internal record RpcPayload
{
    public string State { get; set; }
    public string Details { get; set; }
    public long StartTimestamp { get; set; }
    public string LargeImageKey { get; set; }
    public string LargeImageText { get; set; }
    public string SmallImageKey { get; set; }
    public string SmallImageText { get; set; }

    public RpcPayload Normalize() => this with
    {
        Details = RpcText.Limit(Details),
        State = RpcText.Limit(State),
        LargeImageKey = RpcText.Limit(LargeImageKey?.Trim()),
        SmallImageKey = RpcText.Limit(SmallImageKey?.Trim()),
        LargeImageText = RpcText.Limit(LargeImageText),
        SmallImageText = RpcText.Limit(SmallImageText)
    };
}