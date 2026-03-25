using Godot;
using System;

public partial class GameMaster : Node2D
{
	//Spawn the level based on the port number
	public void SpawnLevel(){
		int LevelStartIndex = 5; //Index where levels start
		GenericCore.Instance.MainNetworkCore.SpawnObject(
			(GenericCore.Instance.Port - 7000) + LevelStartIndex,
			new Vector3(0,0,0),
			new Vector3(0,0,0),
			new Vector3(1,1,1),
			1
		);
	}
}
