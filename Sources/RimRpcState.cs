using System;
using System.Diagnostics;
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
            _engine = EngineApplication.Instance;
            _updater = new PresenceUpdater();
            _engine.OnUpdate += Update;
            _engine.OnShutdown += Shutdown;
         
            if (!ApplyApplicationId())
                RpcService.Configure(RimRpc.Settings.RpcEnabled, ApplicationId);
           
            StateHandler.PushState();
        }
        catch (Exception exception)
        {
            Shutdown();
            Log.Warning("[RimRPC] Could not start Discord integration: " + exception);
        }
    }

    internal static bool ApplyApplicationId(bool force = false)
    {
        if (!RimRpc.Settings.RpcEnabled)
        {
            RpcService.Configure(false, ApplicationId);
            return true;
        }
      
        if (!RpcIdentity.TryResolve(RimRpc.Settings.RpcApplicationId, out var id))
            return false;
       
        RpcService.Configure(true, id, force);
        return true;
    }

    private static void Update()
    {
        var now = Clock.Elapsed.TotalSeconds;

        RpcService.RunCallbacks();
       
        if (now < _retryAfter)
            return;

        try
        {
            _updater?.Update(now);
        }
        catch (Exception exception)
        {
            _retryAfter = now + 5;
            Log.Warning("[RimRPC] Could not update status; retrying in 5 seconds: " + exception);
        }
    }

    private static void Shutdown()
    {
        if (_engine != null)
        {
            _engine.OnUpdate -= Update;
            _engine.OnShutdown -= Shutdown;
            _engine = null;
        }

        _updater = null;
        _retryAfter = 0;

        RpcService.Shutdown();
    }
}
