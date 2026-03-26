using Godot;
using System;

public partial class Coin : CharacterBody2D
{
	[Export] public float RotationSpeed = 0.05f;
	
	public override void _Process(double delta){
		base._Process(delta);
		//Rotate the coin on the clients
		if(!GenericCore.Instance.IsServer()){
			Scale = new Vector2(Scale.X - RotationSpeed, Scale.Y);
			if(Mathf.Abs(Scale.X) >= 1.0f){
				RotationSpeed *= -1;
			}
		}
	}
	
	//Player collects the coin
	public void Collide(Node2D body){
		if(!GenericCore.Instance.IsServer()){
			return;
		}
		if(!(body is Character)){
			return;
		}
		Character Player = (Character)body;
		Player.GivePoints();
		QueueFree();
	}
}
