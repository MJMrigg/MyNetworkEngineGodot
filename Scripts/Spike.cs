using Godot;
using System;

public partial class Spike : CharacterBody2D
{
	[Export] public float RotationSpeed = 1.5f;
	[Export] public float Damage = 10.0f;
	
	public override void _Process(double delta){
		base._Process(delta);
		//Spin the spike on clients
		if(GenericCore.Instance.IsServer()){
			
		}else{
			Rotate(Mathf.DegToRad(RotationSpeed));
		}
	}
	
	//Have the player take damage when it collides
	public void DamagePlayer(Node2D body){
		if(!(body is Character)){
			return;
		}
		Character Player = (Character)body;
		if(GenericCore.Instance.IsServer()){
			Player.TakeDamage(Damage);
		}
	}
}
