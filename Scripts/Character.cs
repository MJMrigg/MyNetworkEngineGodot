using Godot;
using System;

public partial class Character : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public MultiplayerSynchronizer ClientSynchronizer;
	
	//Input variables
	public Vector2 InputedVelocity = Vector2.Zero;
	
	//Whether the snychronizer is ready
	public bool IsSynced;
	
	public override void _EnterTree(){
		//Give client authority to the client synchronizer
		ClientSynchronizer.SetMultiplayerAuthority(int.Parse(((string)Name).Split("+")[1]));
		ClientSynchronizer.SetVisibilityFor(1,true);
	}
	
	public override void _Ready(){
		base._Ready();
	}
	
	//Mark if the server synchronizer is synced
	public void Synchronized(){
		IsSynced = true;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if(!IsSynced){
			return;
		}
		if(GenericCore.Instance.IsServer()){ //Server
			//Calculate velocity using the velocity from the client and move the object
			Velocity = InputedVelocity.Normalized()*Speed;
			MoveAndSlide();
		}else{ //Clients
			if(GenericCore.Instance.IsLocal(ClientSynchronizer.GetMultiplayerAuthority())){ //Local Client
				//Get inputed velocity from the clients
				InputedVelocity = Input.GetVector("Left","Right","Up","Down");
			}else{ //Non-local Clients
				
			}
		}
	}
}
