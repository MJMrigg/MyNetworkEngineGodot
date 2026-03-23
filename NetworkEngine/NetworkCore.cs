using Godot;
using Godot.Collections;
using System;

public partial class NetworkCore : MultiplayerSpawner
{
	public int ObjectCount = 0; //Number of objects spawned
	[Export] public bool SpawnOnConnect; //Whether to spawn the first index on connection
	[Export] public bool HostIsPlayer; //Whether the host is a player or not
	
	//Spawn first object in spawn list upon client connect
	public void SpawnOnConnection(int OwnerId){
		if(!SpawnOnConnect){
			return;
		}
		//If the server isn't a player, don't spawn anything
		if(OwnerId == 1 && !HostIsPlayer){
			return;
		}
		//Only server should spawn objects
		if(GenericCore.Instance.IsServer()){
			SpawnObject(0, new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(1,1,1), OwnerId);
		}
	}
	
	//Spawn an object and set its properties
	public Node SpawnObject(int Index, Vector3 SpawnPosition, Vector3 SpawnRotation, Vector3 SpawnScale, int OwnerId=1){
		try{
			//Create the object
			var ObjectScene = GD.Load<PackedScene>(GetSpawnableScene(Index));
			var NewObject = ObjectScene.Instantiate();
			
			//Set properties
			if(NewObject is Node2D){ //2D nodes
				((Node2D)NewObject).GlobalPosition = new Vector2(SpawnPosition.X, SpawnPosition.Y);
				//Rotation is weird, since you can only rotate one axis for 3D objects, so Godot stores that as a float
				((Node2D)NewObject).Rotation = SpawnRotation.X;
				//For scaling, the object's scale must be multiplied by its spawn scale, other wise it'll just scale to the spawn scale and forget its intial scale
				Vector2 IntialScale = ((Node2D)NewObject).Scale;
				((Node2D)NewObject).Scale = new Vector2(IntialScale.X * SpawnScale.X, IntialScale.Y * SpawnScale.Y);
			}else if(NewObject is Node3D){ //3D nodes
				((Node3D)NewObject).GlobalPosition = SpawnPosition;
				((Node3D)NewObject).Rotation = SpawnRotation;
				//For scaling, the object's scale must be multiplied by its spawn scale, other wise it'll just scale to the spawn scale and forget its intial scale
				Vector3 IntialScale = ((Node3D)NewObject).Scale;
				((Node3D)NewObject).Scale = SpawnScale * IntialScale;
			}
			
			//Set its name to be random since Spawners can't handle having two objects of the same name
			//Include the OwnerId. When the object starts synchronizing, it'll need it
			NewObject.Name = NewObject.Name+ObjectCount+"+"+OwnerId.ToString();
			ObjectCount += 1;
			
			//Add it to the scene
			GetNode(SpawnPath).AddChild(NewObject);
			
			/*Collections.Array[Node] Synchronizers = NewObject.GetChildren("*","MultiplayerSychronizer");
			foreach(Node Child in Synchronizers){
				GD.Print(Child);
				GD.Print(Child is NetId);
			}*/

			//Connect the disconnected signal to its NetId so that it despawns when the player disconnects
			foreach(Node Child in NewObject.GetChildren()){
				if(!(Child is NetId)){
					continue;
				}
				NetId Temp = (NetId)Child;
				//Only the server should be deleting objects
				if(Temp.IsServer){
					GenericCore.Instance.ClientDisconnected += Temp.ClientDespawn;
				}
			}
			
			//Return it
			return NewObject;
		}catch{
			GD.Print("Error, couldn't spawn object");
		}
		//If it got here, there was an error
		return null;
	}
	
	//Destroy all objects in the spawn path
	public void DestroyObjects(){
		foreach(Node Child in GetNode(SpawnPath).GetChildren()){
			Child.QueueFree();
		}
	}
}
