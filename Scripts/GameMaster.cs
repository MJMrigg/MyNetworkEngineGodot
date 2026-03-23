using Godot;
using System;

public partial class GameMaster : Node2D
{
	//Spawn the level based on the port number
	public void SpawnLevel(){
		//Spawn the object at the center of the viewport
		Vector2 CenterPosition = GetViewportRect().Size;
		CenterPosition *= 0.5f;
		int LevelStartIndex = 5; //Index where levels start
		GenericCore.Instance.MainNetworkCore.SpawnObject(
			(GenericCore.Instance.Port - 7000) + LevelStartIndex,
			new Vector3(CenterPosition.X,CenterPosition.Y,0),
			new Vector3(0,0,0),
			new Vector3(1,1,1),
			1
		);
	}
}
