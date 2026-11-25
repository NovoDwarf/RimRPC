using System;
using System.Diagnostics;
using RimRPC.Core;
using RimRPC.RPC;
using RimRPC.Runtime;
using Verse;
using Waylith;

namespace RimRPC;

public class RimRpcState
{
    private const string ApplicationId = RpcIdentity.Default;

    internal static bool IsRunning => _updater != null;
    internal static readonly long Started = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private static readonly Stopwatch Clock = Stopwatch.StartNew();
    private static EngineApplication _engine;
    private static PresenceUpdater _updater;
    private static double _retryAfter;

    public static void BootMeUp()
    {
        if (_updater != null)
            return;

        try
        {
            _engine = EngineApplication.Instance ?? throw new InvalidOperationException("Waylith is not initialized.");
            _updater = new PresenceUpdater();
            _engine.OnUpdate += Update;
            _engine.OnShutdown += Shutdown;
            if (!ApplyApplicationId())
                DiscordRpcService.Configure(RimWorldRPCMod.Settings.RpcEnabled, ApplicationId);
            StateHandler.PushState();
        }
        catch (Exception exception)
        {
            Shutdown();
            Log.Warning("RichPresence :: Could not start Discord integration: " + exception);
        }
    }

    internal static bool TryResolveApplicationId(string customId, out string id)
    {
        return RpcIdentity.TryResolve(customId, out id);
    }

    internal static bool ApplyApplicationId(bool force = false)
    {
        if (!RimWorldRPCMod.Settings.RpcEnabled)
        {
            DiscordRpcService.Configure(false, ApplicationId);
            return true;
        }
        if (!TryResolveApplicationId(RimWorldRPCMod.Settings.RpcApplicationId, out var id))
            return false;
        DiscordRpcService.Configure(true, id, force);
        return true;
    }

    private static void Update()
    {
        var now = Clock.Elapsed.TotalSeconds;

        DiscordRpcService.RunCallbacks();
        if (now < _retryAfter)
            return;

        try
        {
            _updater?.Update(now);
        }
        catch (Exception exception)
        {
            _retryAfter = now + 5;
            Log.Warning("RichPresence :: Could not update status; retrying in 5 seconds: " + exception);
        }
    }

    internal static void Shutdown()
    {
        if (_engine != null)
        {
            _engine.OnUpdate -= Update;
            _engine.OnShutdown -= Shutdown;
            _engine = null;
        }

        _updater = null;
        _retryAfter = 0;

        DiscordRpcService.Shutdown();
    }
}
