using Godot;
using System;

public partial class GameMaster : Node2D
{
	[Export] public NetworkCore ObjectSpawner;
	[Export] public NetworkCore UiSpawner;
	
	async public void SpawnCharacter(int ConnectionId){
		if(!GenericCore.Instance.IsServer || ConnectionId == 1){
			return;
		}
		Vector3 ObjectPosition = new Vector3((float)GD.RandRange(0.0,400.0), (float)GD.RandRange(0.0,400.0), 0);
		Vector3 ObjectRotation = new Vector3((float)GD.RandRange(0.0,360.0), (float)GD.RandRange(0.0,360.0), 0);
		CharacterBody2D NewCharacter = (CharacterBody2D)ObjectSpawner.SpawnObject(0, ObjectPosition, ObjectRotation, new Vector3(1,1,1), ConnectionId);
		await ToSignal(GetTree().CreateTimer(7.0),"timeout");
		NewCharacter.QueueFree();
	}
}
