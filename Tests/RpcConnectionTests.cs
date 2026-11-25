using RimRPC.RPC;
using Xunit;

namespace RimRPC.Tests;

public sealed class RpcConnectionTests
{
    [Fact]
    public void DisabledConnectionDoesNotStart()
    {
        var connection = new RpcConnection();

        connection.Configure(false, "123", 0);
        connection.Publish(new RpcPayload { Details = "Colony" });
        connection.Tick(100);

        Assert.Equal(RpcConnectionState.Disabled, connection.State);
        Assert.Null(connection.LastError);
    }

    [Fact]
    public void PresenceMapsFieldsToDiscord()
    {
        var presence = new RpcPayload
        {
            Details = "Colony",
            State = "Day 10",
            StartTimestamp = 1700000000,
            LargeImageKey = "logo",
            LargeImageText = "RimWorld",
            SmallImageKey = "inmap",
            SmallImageText = "Playing"
        };

        var result = RpcConnection.ToRichPresence(presence);

        Assert.Equal(presence.Details, result.Details);
        Assert.Equal(presence.State, result.State);
        Assert.Equal(presence.LargeImageKey, result.Assets.LargeImageKey);
        Assert.Equal(presence.LargeImageText, result.Assets.LargeImageText);
        Assert.Equal(presence.SmallImageKey, result.Assets.SmallImageKey);
        Assert.Equal(presence.SmallImageText, result.Assets.SmallImageText);
        Assert.Equal(System.DateTimeOffset.FromUnixTimeSeconds(presence.StartTimestamp).UtcDateTime, result.Timestamps.Start);
    }

    [Fact]
    public void PresenceOmitsMissingTimestamp()
    {
        var result = RpcConnection.ToRichPresence(new RpcPayload());

        Assert.Null(result.Timestamps);
    }
}
