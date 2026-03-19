using Godot;

public partial class GenericCore : Node
{
	public static GenericCore Instance { get; private set; }

	//Signals emitted when connection functionality happens
	//They have nothing to do with the functions that share the names that are connected to the multiplayer signals
	//Except for the fact that those functions emit them. THEY'RE NOT CONNECTED!!!
	[Signal] public delegate void PlayerConnectedEventHandler(int peerId);
	[Signal] public delegate void PlayerDisconnectedEventHandler(int peerId);
	[Signal] public delegate void ServerDisconnectedEventHandler();
	
	[Export] public string DefaultServerIP = "127.0.0.1"; // IPv4 localhost
	[Export] public int Port = 7000;
	[Export] public int MaxConnections = 20;
	
	//Variables that make my life easier
	public bool IsServer; //Whether the multiplayer peer is the server(just to make life easier)
	public int ConnectionId; //The multiplayer peer's unique id (just to make life a litte easer
	public bool IsConnected = false; //Whether the multiplayer peer is connected

	// This will contain the unique multiplayer keys for each registered connection
	// It's a dictionary to make life a little easier
	public Godot.Collections.Dictionary<long, long> _players = new Godot.Collections.Dictionary<long, long>();

	public int _playersLoaded = 0;

	public override void _Ready()
	{
		Instance = this;
		//Server and client
		Multiplayer.PeerConnected += OnPlayerConnected;
		Multiplayer.PeerDisconnected += OnPlayerDisconnected;
		//Clients only
		Multiplayer.ConnectedToServer += OnConnectOk;
		Multiplayer.ConnectionFailed += OnConnectionFail;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

	//Join a multiplayer agme
	public Error JoinGame(string address = "")
	{
		//Set the Ip Address
		if (string.IsNullOrEmpty(address))
		{
			address = DefaultServerIP;
		}
		
		//Create the multiplayer peer client
		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateClient(address, Port);

		if (error != Error.Ok)
		{
			return error;
		}
		
		//Set the current multiplayer peer to the client
		Multiplayer.MultiplayerPeer = peer;
		
		//Mark this as not the server
		IsServer = false;
		ConnectionId = Multiplayer.GetUniqueId();
		
		return Error.Ok;
	}

	//Start a multiplayer game
	public Error CreateGame()
	{
		//Create the multiplayer peer server
		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Port, MaxConnections);

		if (error != Error.Ok)
		{
			return error;
		}
		
		//Set the current mutiplayer peer to the server
		Multiplayer.MultiplayerPeer = peer;
		//Register the server in the list of connections
		_players[1] = 1;
		EmitSignal(SignalName.PlayerConnected, 1);
		
		//Mark that this is the server and it is connected
		IsServer = true;
		IsConnected = true;
		ConnectionId = 1;
		
		return Error.Ok;
	}

	// Stop multiplayer
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

	//Connection between two peers is established(can be client-client, or client-server)
	public void OnPlayerConnected(long id)
	{
		//Server registers client or client registers server
		//Godot forces clients to connect to each other, so they just won't register each other
		//Meaning that they'll have connections, they just won't know it.
		if(IsServer || id == 1){
			RpcId(id, MethodName.RegisterPlayer);
		}
		//Why not just force disconnect? Because that opens the door to weird errors that I'd rather not deal with
	}

	//Register a connection
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RegisterPlayer()
	{
		int newPlayerId = Multiplayer.GetRemoteSenderId();
		_players[newPlayerId] = newPlayerId;
		EmitSignal(SignalName.PlayerConnected, newPlayerId);
	}
	
	//Peer disconnected from this peer
	public void OnPlayerDisconnected(long id)
	{
		//Unregister it
		_players.Remove(id);
		EmitSignal(SignalName.PlayerDisconnected, id);
	}

	//Client connects to the server
	public void OnConnectOk()
	{
		IsConnected = true;
		//Register the client in the client's list of connections
		_players[ConnectionId] = ConnectionId;
		//Register the server
		EmitSignal(SignalName.PlayerConnected, ConnectionId);
	}

	//Client couldn't connect to the server
	public void OnConnectionFail()
	{
		Multiplayer.MultiplayerPeer = null;
	}

	//Server disconnected the player
	public void OnServerDisconnected()
	{
		GD.Print("Server ended the game");
		IsConnected = false;
		Multiplayer.MultiplayerPeer = null;
		_players.Clear();
		EmitSignal(SignalName.ServerDisconnected);
	}
}
