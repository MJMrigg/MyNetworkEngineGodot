using Godot;
using System;

public partial class Portal : StaticBody2D
{
	[Export] public int Port;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	//Send the player to the next server
	public void GateToLevel(Node2D body){
		if(!(body is Character)){
			return;
		}
		Character Player = (Character)body;
		//If they're the local player, have them send their data to the 
		if(!Player.ClientSynchronizer.IsLocal){
			return;
		}
		GenericCore.Instance.RemoveMultiplayerPeer();
		Database.Instance.Port = Port;
		GenericCore.Instance.Port = Port;
		GenericCore.Instance.JoinGame();
	}
}
