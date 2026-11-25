using Verse;

namespace RimRPC.Runtime;

internal sealed class PresenceUpdater
{
    private Game _lastGame;
    private Map _lastMap;
    private bool _wasPlaying;
    private double _nextUpdate;
    private float _lastPeriod;

    public void Update(double elapsedSeconds)
    {
        var game = Current.Game;
        var playing = Current.ProgramState == ProgramState.Playing && game?.World != null;

        if (!playing)
        {
            if (_wasPlaying)
                StateHandler.PushState();

            _wasPlaying = false;
            _lastGame = null;
            _lastMap = null;

            return;
        }

        var map = game.CurrentMap;
        var period = RimWorldRPCMod.Settings.RpcPeriod;

        if (!_wasPlaying || game != _lastGame || map != _lastMap || period != _lastPeriod || elapsedSeconds >= _nextUpdate)
        {
            StateHandler.PushState();
            _nextUpdate = elapsedSeconds + period;
        }

        _wasPlaying = true;
        _lastGame = game;
        _lastMap = map;
        _lastPeriod = period;
    }
}
