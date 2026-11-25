using System.Diagnostics;
using RimRPC.Core;

namespace RimRPC.RPC;

internal static class DiscordRpcService
{
    private static readonly Stopwatch Clock = Stopwatch.StartNew();
    private static readonly RpcConnection Connection = new();

    private static string _lastReportedError;

    public static RpcConnectionState State => Connection.State;
    public static bool IsConnected => State == RpcConnectionState.Connected;
    public static string LastError => Connection.LastError;
    public static void Configure(bool enabled, string id, bool force = false) => Connection.Configure(enabled, id, Clock.Elapsed.TotalSeconds, force);
    public static void RunCallbacks()
    {
        Connection.Tick(Clock.Elapsed.TotalSeconds);

        switch (State)
        {
            case RpcConnectionState.Error when LastError != _lastReportedError:
                _lastReportedError = LastError;
                Verse.Log.Warning("RichPresence :: " + LastError);
                break;
            case RpcConnectionState.Connected:
                _lastReportedError = null;
                break;
        }
    }

    public static void UpdatePresence(RpcPayload presence) => Connection.Publish(presence);
    public static void Shutdown() => Connection.Stop();
}