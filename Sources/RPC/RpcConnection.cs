using System;
using DiscordRPC;
using DiscordRPC.Message;

namespace RimRPC.RPC;

internal sealed class RpcConnection
{
    public RpcConnectionState State { get; private set; } = RpcConnectionState.Disabled;
    public string LastError { get; private set; }

    private DiscordRpcClient _client;
    private RpcPayload _presence;
    
    private string _applicationId;
    private bool _enabled;
    
    private double _now;
    private double _retryAt;
    private double _attemptStarted;
    
    private int _failures;
    
    public void Configure(bool enabled, string applicationId, double now, bool force = false)
    {
        _now = now;
        
        if (!force && _enabled == enabled && _applicationId == applicationId)
            return;
        
        Close();
       
        _enabled = enabled;
        _applicationId = applicationId;
        _failures = 0;
       
        LastError = null;
        State = RpcConnectionState.Disabled;
       
        if (enabled)
            Connect();
    }

    public void Tick(double now)
    {
        _now = now;

        if (!_enabled)
            return;

        if (_client == null)
        {
            if (now >= _retryAt)
                Connect();

            return;
        }
        try
        {
            _client.Invoke();
           
            if (State == RpcConnectionState.Connecting && now - _attemptStarted >= 30)
                Fail("Discord did not confirm the connection within 30 seconds.");
        }
        catch (Exception exception)
        {
            Fail(exception.Message);
        }
    }

    public void Publish(RpcPayload presence)
    {
        _presence = presence.Normalize();
     
        if (State != RpcConnectionState.Connected)
            return;

        try
        {
            _client.SetPresence(ToRichPresence(_presence));
        }
        catch (Exception exception)
        {
            Fail(exception.Message);
        }
    }

    public void Stop()
    {
        _enabled = false;

        Close();

        State = RpcConnectionState.Disabled;
        LastError = null;
    }

    internal static RichPresence ToRichPresence(RpcPayload presence) => new()
    {
        Details = presence.Details,
        State = presence.State,
        
        Assets = new Assets
        {
            LargeImageKey = presence.LargeImageKey,
            LargeImageText = presence.LargeImageText,
            SmallImageKey = presence.SmallImageKey,
            SmallImageText = presence.SmallImageText
        },
        
        Timestamps = presence.StartTimestamp == 0
            ? null
            : new Timestamps(DateTimeOffset.FromUnixTimeSeconds(presence.StartTimestamp).UtcDateTime)
    };

    private void Connect()
    {
        State = RpcConnectionState.Connecting;
      
        _attemptStarted = _now;
       
        try
        {
            var client = new DiscordRpcClient(_applicationId, autoEvents: false);
           
            _client = client;
           
            client.OnReady += OnReady;
            client.OnClose += OnClose;
            client.OnConnectionFailed += OnConnectionFailed;
            client.OnError += OnError;

            if (!client.Initialize() && _client == client)
                Fail("Could not initialize the Discord IPC client.");
        }
        catch (Exception exception)
        {
            Fail(exception.Message);
        }
    }

    private void OnReady(object sender, ReadyMessage message)
    {
        if (sender != _client || !_enabled)
            return;

        State = RpcConnectionState.Connected;
        LastError = null;
        
        _failures = 0;

        if (_presence != null)
            Publish(_presence);
    }

    private void OnClose(object sender, CloseMessage message)
    {
        if (sender == _client)
            Fail(message.Reason);
    }

    private void OnConnectionFailed(object sender, ConnectionFailedMessage message)
    {
        if (sender == _client)
            Fail("Discord desktop client is unavailable.");
    }

    private void OnError(object sender, ErrorMessage message)
    {
        if (sender == _client)
            Fail(message.Message);
    }

    private void Fail(string error)
    {
        Close();
       
        LastError = string.IsNullOrWhiteSpace(error) ? "Discord connection failed." : error;
        State = RpcConnectionState.Error;
       
        _retryAt = _now + Math.Min(60, 5 * Math.Pow(2, Math.Min(_failures++, 4)));
    }

    private void Close()
    {
        var client = _client;
        _client = null;
        try
        {
            client?.Dispose();
        }
        catch
        {
            // Cleanup failure must not stop reconnects or the game loop.
        }
    }
}
