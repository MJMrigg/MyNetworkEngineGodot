using Godot;
using System;

public partial class NetId : MultiplayerSynchronizer
{
	//Booleans that store information regarding the Mutliplayer Peer Authority
	[Export] public bool ClientAuthority = false; //If the client has authority over the synchronizer
	public bool IsLocal = false; //If the peer is the local client
	public bool IsServer = false; //If the peer is the server
	
	//The connection that NetId handles
	public int OwnerId;
	
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
		
		/*
		ON THE NODE THE SYNCHRONIZER IS ATTATCHED TO, MAKE SURE TO ADD THE FOLLOWING:
		public override void _EnterTree(){
			//Give client authority to the client synchronizer
			ClientSynchronizer.GiveAuthority();
			base._EnterTree();
		}
		
		You should be able to just type the variable name in the synchronizer and call it a day
		*/
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
	
	//Despawn the object when the client disconnects
	public void ClientDespawn(int PeerId){
		if(PeerId != GetMultiplayerAuthority()){
			return;
		}
		GetNode(RootPath).QueueFree();
	}
}
