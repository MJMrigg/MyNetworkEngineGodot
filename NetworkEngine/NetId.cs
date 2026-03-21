using Godot;
using System;

public partial class NetId : MultiplayerSynchronizer
{
	//Booleans that store information regarding the Mutliplayer Peer Authority
	[Export] public bool ClientAuthority = false; //If the client has authority over the synchronizer
	public bool IsLocal = false; //If the peer is the local client
	public bool IsServer = false; //If the peer is the server
	
	//Whether the NetId is sending data to the server
	public bool SendingData = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//If the client is supposed to have authority over this synchronizer, only target the server
		if(ClientAuthority){
			PublicVisibility = false;
			SetVisibilityFor(1,true);
		}
		//Figure out if this NetId's peer is the server or the local peer
		IsServer = GenericCore.Instance.IsServer();
		IsLocal = (GetMultiplayerAuthority() == GenericCore.Instance.GetConnectionId());
		
		base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	//Give client authority to this synchronizer using the name of the node its synchronizing
	public void GiveAuthority(){
		//Set the multiplayer authority
		int ClientId = int.Parse(((string)GetNode(RootPath).Name).Split("+")[1]);
		SetMultiplayerAuthority(ClientId);
		
		//Redo network variables
		IsLocal = (ClientId == GenericCore.Instance.GetConnectionId());
		IsServer = GenericCore.Instance.IsServer();
	}
	
	//Tell the object the synchronizer is attached to that its synchronizing data now
	public void Synchronizing(){
		//Disconnect the synchronized signal so that it doesn't keep calling this function every frame
		var Function = new Callable(this, nameof(Synchronizing));
		if(IsConnected(SignalName.Synchronized, Function)){
			Disconnect(SignalName.Synchronized, Function);
		}
		//Say that the NetId is sending data
		SendingData = true;
	}
}
