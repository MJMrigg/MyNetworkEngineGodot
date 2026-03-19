using Godot;

public partial class GenericCore : Node
{
	public static GenericCore Instance { get; private set; }

	// These signals can be connected to by a UI lobby scene or the game scene.
	[Signal]
	public delegate void PlayerConnectedEventHandler(int peerId, Godot.Collections.Dictionary<string, string> playerInfo);
	[Signal]
	public delegate void PlayerDisconnectedEventHandler(int peerId);
	[Signal]
	public delegate void ServerDisconnectedEventHandler();
	
	[Export] public string DefaultServerIP = "127.0.0.1"; // IPv4 localhost
	[Export] public int Port = 7000;
	[Export] public int MaxConnections = 20;
	public bool IsServer;

	// This will contain player info for every player,
	// with the keys being each player's unique IDs.
	public Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> _players = new Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>>();

	// This is the local player info. This should be modified locally
	// before the connection is made. It will be passed to every other peer.
	// For example, the value of "name" can be set to something the player
	// entered in a UI scene.
	public Godot.Collections.Dictionary<string, string> _playerInfo = new Godot.Collections.Dictionary<string, string>()
	{
		{ "Name", "PlayerName" },
	};

	public int _playersLoaded = 0;

	public override void _Ready()
	{
		Instance = this;
		//Server and client
		Multiplayer.PeerConnected += OnPlayerConnected;
		Multiplayer.PeerDisconnected += OnPlayerDisconnected;
		//Just client
		Multiplayer.ConnectedToServer += OnConnectOk;
		Multiplayer.ConnectionFailed += OnConnectionFail;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

	public Error JoinGame(string address = "")
	{
		if (string.IsNullOrEmpty(address))
		{
			address = DefaultServerIP;
		}

		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateClient(address, Port);

		if (error != Error.Ok)
		{
			return error;
		}

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joined Game");
		IsServer = false;
		return Error.Ok;
	}

	public Error CreateGame()
	{
		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Port, MaxConnections);

		if (error != Error.Ok)
		{
			return error;
		}

		Multiplayer.MultiplayerPeer = peer;
		_players[1] = _playerInfo;
		EmitSignal(SignalName.PlayerConnected, 1, _playerInfo);
		GD.Print("Server Created");
		IsServer = true;
		return Error.Ok;
	}

	public void RemoveMultiplayerPeer()
	{
		Multiplayer.MultiplayerPeer = null;
		_players.Clear();
	}

	// When the server decides to start the game from a UI scene,
	// do Rpc(Lobby.MethodName.LoadGame, filePath);
	[Rpc(CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void LoadGame(string gameScenePath)
	{
		GetTree().ChangeSceneToFile(gameScenePath);
	}

	// Every peer will call this when they have loaded the game scene.
	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void PlayerLoaded()
	{
		if (Multiplayer.IsServer())
		{
			_playersLoaded += 1;
			if (_playersLoaded == _players.Count)
			{
				//GetNode<Game>("/root/Game").StartGame();
				_playersLoaded = 0;
			}
		}
	}

	// When a peer connects, send them my player info.
	// This allows transfer of all desired data for each player, not only the unique ID.
	public void OnPlayerConnected(long id)
	{
		GD.Print("I am "+Multiplayer.GetUniqueId()+" and "+id+" just connected to me");
		RpcId(id, MethodName.RegisterPlayer, _playerInfo);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RegisterPlayer(Godot.Collections.Dictionary<string, string> newPlayerInfo)
	{
		int newPlayerId = Multiplayer.GetRemoteSenderId();
		GD.Print("I am "+Multiplayer.GetUniqueId()+" and I am adding "+newPlayerId+" to player list");
		_players[newPlayerId] = newPlayerInfo;
		EmitSignal(SignalName.PlayerConnected, newPlayerId, newPlayerInfo);
		GD.Print(Multiplayer.GetUniqueId()+" "+_players);
	}

	public void OnPlayerDisconnected(long id)
	{
		_players.Remove(id);
		EmitSignal(SignalName.PlayerDisconnected, id);
	}

	public void OnConnectOk()
	{
		int peerId = Multiplayer.GetUniqueId();
		_players[peerId] = _playerInfo;
		EmitSignal(SignalName.PlayerConnected, peerId, _playerInfo);
	}

	public void OnConnectionFail()
	{
		Multiplayer.MultiplayerPeer = null;
	}

	public void OnServerDisconnected()
	{
		Multiplayer.MultiplayerPeer = null;
		_players.Clear();
		EmitSignal(SignalName.ServerDisconnected);
	}
}
