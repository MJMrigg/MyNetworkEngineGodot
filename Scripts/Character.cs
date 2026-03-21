using Godot;
using System;

public partial class Character : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public NetId ClientSynchronizer;
	[Export] public NetId ServerSynchronizer;
	
	//Input variables
	public Vector2 InputedVelocity = Vector2.Zero;
	
	public override void _EnterTree(){
		//Give client authority to the client synchronizer
		ClientSynchronizer.GiveAuthority();
		base._EnterTree();
	}
	
	public override void _Ready(){
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if(ServerSynchronizer.IsServer && ClientSynchronizer.SendingData){ //Server
			//Calculate velocity using the velocity from the client and move the object
			Velocity = InputedVelocity.Normalized()*Speed;
			MoveAndSlide();
		}else if(!ClientSynchronizer.IsServer && ServerSynchronizer.SendingData){ //Clients
			if(ClientSynchronizer.IsLocal){ //Local Client
				//Get inputed velocity from the clients
				InputedVelocity = Input.GetVector("Left","Right","Up","Down");
			}else{ //Non-local Clients
				
			}
		}
	}
}
