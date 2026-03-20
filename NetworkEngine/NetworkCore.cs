using Godot;
using System;

public partial class NetworkCore : MultiplayerSpawner
{
	//Spawn first object in spawn list upon client connect
	public void SpawnOnConnection(int OwnerId){
		if(OwnerId != 1 && GenericCore.Instance.IsServer()){ //Not server though
			SpawnObject(0, new Vector3(GD.RandRange(0,400),GD.RandRange(0,400),0), new Vector3(GD.RandRange(0,400),GD.RandRange(0,400),0), new Vector3(GD.RandRange(1,3),GD.RandRange(1,3),1), OwnerId);
		}
	}
	
	//Spawn an object and set its properties
	public Node SpawnObject(int Index, Vector3 SpawnPosition, Vector3 SpawnRotation, Vector3 SpawnScale, int OwnerId=1){
		try{
			//Create the object
			var ObjectScene = GD.Load<PackedScene>(GetSpawnableScene(Index));
			var NewObject = ObjectScene.Instantiate();
			
			//Set properties
			if(NewObject is Node2D || NewObject is Control){ //2D nodes
				((Node2D)NewObject).Position = new Vector2(SpawnPosition.X, SpawnPosition.Y);
				//Rotation is weird, since you can only rotate one axis for 3D objects, so Godot stores that as a float
				((Node2D)NewObject).Rotation = SpawnRotation.X;
				((Node2D)NewObject).Scale = new Vector2(SpawnScale.X, SpawnScale.Y);
			}else if(NewObject is Node3D){ //3D nodes
				((Node3D)NewObject).Position = SpawnPosition;
				((Node3D)NewObject).Rotation = SpawnRotation;
				((Node3D)NewObject).Scale = SpawnScale;
			}
			
			//Set its name to be random since Spawners can't handle having two objects of the same name
			//Include the OwnerId. When the object starts synchronizing, it'll need it
			NewObject.Name = Multiplayer.MultiplayerPeer.GenerateUniqueId().ToString()+"+"+OwnerId.ToString();
			
			//Add it to the scene
			GetNode(SpawnPath).AddChild(NewObject);
			
			//Return it
			return NewObject;
		}catch{
			GD.Print("Error, couldn't spawn object");
		}
		//If it got here, there was an error
		return null;
	}
}
