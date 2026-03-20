using Godot;
using System;

public partial class Character : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public MultiplayerSynchronizer ClientSynchronizer;
	
	//Whether the synchronizers are synced
	public bool ClientSynced;
	public bool ServerSynced;
	
	//Input variables
	public Vector2 InputedVelocity = Vector2.Zero;
	
	public override void _EnterTree(){
		//Give client authority to the client synchronizer
		ClientSynchronizer.SetMultiplayerAuthority(int.Parse(((string)Name).Split("+")[1]));
		ClientSynchronizer.SetVisibilityFor(1,true);
	}
	
	public override void _Ready(){
		//Don't do anything until the synchronizer has been set up
		base._Ready();
	}
	
	//Mark if the server synchronizer is synced
	public void ServerIsSynced(){
		ServerSynced = true;
	}
	
	//Mark if the client synchronizer is synced
	public void ClientIsSynced(){
		ClientSynced = true;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		//Only process if all synchronizers are working
		/*if(!ClientSynced || !ServerSynced){
			return;
		}*/
		if(GenericCore.Instance.IsServer()){ //Server
			//Calculate velocity using the velocity from the client and move the object
			Velocity = InputedVelocity.Normalized()*Speed;
			MoveAndSlide();
		}else{ //Clients
			GD.Print(ClientSynchronizer.GetMultiplayerAuthority());
			if(GenericCore.Instance.IsLocal(ClientSynchronizer.GetMultiplayerAuthority())){ //Local Client
				//Get inputed velocity from the clients
				InputedVelocity = Input.GetVector("Left","Right","Up","Down");
			}else{ //Non-local Clients
				
			}
		}
	}
}
