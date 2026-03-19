using Godot;

public partial class GenericCore : Node
{
	public static GenericCore Instance { get; private set; }

	//Signals emitted when connection functionality happens
	//They have nothing to do with the functions that share the names that are connected to the multiplayer signals
	//Except for the fact that those functions can emit them. THEY'RE NOT CONNECTED!!!
	[Signal] public delegate void ClientConnectedEventHandler(int peerId);
	[Signal] public delegate void ClientDisconnectedEventHandler(int peerId);
	[Signal] public delegate void ServerDisconnectedEventHandler();
	
	//Connection information
	[Export] public string DefaultServerIP = "127.0.0.1"; // IPv4 localhost
	[Export] public int Port = 7000;
	[Export] public int MaxConnections = 20;
	public int ConnectionsLoaded = 0;
	
	//Variables that make my life easier
	public bool IsServer; //Whether the multiplayer peer is the server(just to make life easier)
	public int ConnectionId; //The multiplayer peer's unique id (just to make life a litte easer
	public bool IsConnected = false; //Whether the multiplayer peer is connected

	// This will contain the unique multiplayer keys for each registered connection
	// It's a dictionary to make life a little easier
	public Godot.Collections.Dictionary<long, long> Connections = new Godot.Collections.Dictionary<long, long>();

	public override void _Ready()
	{
		Instance = this;
		//Server and client
		Multiplayer.PeerConnected += OnClientConnected;
		Multiplayer.PeerDisconnected += OnClientDisconnected;
		//Clients only
		Multiplayer.ConnectedToServer += OnConnectOk;
		Multiplayer.ConnectionFailed += OnConnectionFail;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

	//Join a multiplayer agme
	public Error JoinGame(string Address = "")
	{
		//Set the Ip Address
		if (string.IsNullOrEmpty(Address))
		{
			Address = DefaultServerIP;
		}
		
		//Create the multiplayer peer client
		var Peer = new ENetMultiplayerPeer();
		Error Response = Peer.CreateClient(Address, Port);

		if (Response != Error.Ok)
		{
			return Response;
		}
		
		//Set the current multiplayer peer to the client
		Multiplayer.MultiplayerPeer = Peer;
		
		return Error.Ok;
	}

	//Start a multiplayer game
	public Error CreateGame()
	{
		//Create the multiplayer peer server
		var Peer = new ENetMultiplayerPeer();
		Error Response = Peer.CreateServer(Port, MaxConnections);

		if (Response != Error.Ok)
		{
			return Response;
		}
		
		//Set the current mutiplayer peer to the server
		Multiplayer.MultiplayerPeer = Peer;
		//Register the server in the list of connections
		Connections[1] = 1;
		EmitSignal(SignalName.ClientConnected, 1);
		
		//Mark that this is the server and it is connected
		IsServer = true;
		IsConnected = true;
		ConnectionId = 1;
		
		return Error.Ok;
	}

	// Stop multiplayer
	public void RemoveMultiplayerPeer()
	{
		IsConnected = false;
		Multiplayer.MultiplayerPeer.Close();
		Connections.Clear();
	}

	//Connection between two peers is established(can be client-client, or client-server)
	public void OnClientConnected(long id)
	{
		//Server registers client or client registers server
		//Godot forces clients to connect to each other, so they just won't register each other
		//Meaning that they'll have connections, they just won't know it.
		if(IsServer || id == 1){
			RpcId(id, MethodName.RegisterClient);
		}else{
			//Multiplayer.MultiplayerPeer.DisconnectPeer((int)id, false);
		}
		//Why not just force disconnect? Because that opens the door to weird errors that I'd rather not deal with
	}

	//Register a connection in the list of connections
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RegisterClient()
	{
		int NewClientId = Multiplayer.GetRemoteSenderId();
		Connections[NewClientId] = NewClientId;
		EmitSignal(SignalName.ClientConnected, NewClientId);
	}
	
	//A peer disconnected from this peer
	public void OnClientDisconnected(long id)
	{
		//Unregister it
		Connections.Remove(id);
		EmitSignal(SignalName.ClientDisconnected, id);
	}

	//Client connects to the server
	public void OnConnectOk()
	{
		//Mark this client as sucessfully connected
		IsConnected = true;
		IsServer = false;
		ConnectionId = Multiplayer.GetUniqueId();
		//Register the client in the client's list of connections
		Connections[ConnectionId] = ConnectionId;
		//Register the server
		EmitSignal(SignalName.ClientConnected, ConnectionId);
	}

	//Client couldn't connect to the server
	public void OnConnectionFail()
	{
		RemoveMultiplayerPeer();
	}

	//Server disconnected the client
	public void OnServerDisconnected()
	{
		GD.Print("Server disconnected");
		RemoveMultiplayerPeer();
		EmitSignal(SignalName.ServerDisconnected);
	}
}
