using Godot;
using System;

public partial class SpawnEgg : Node2D
{
	[Export] public int SpawnIndex; //The object in Network Core's autospawn list that will be spawned upon connection
	[Export] public bool Respawn; //Whether the spawnegg should respawn the object
	public bool Spawned = false; //Whether the object has spawned
	public Node TheObject; //The object it spawned
	[Export] public float RespawnTimer; //The timer for respawning an object
	public bool Spawning = false; //Whether the Spawn Egg is currently spawning the object(only for respawning)
	
	// Called when the node enters the scene tree for the first time.
	public override void _EnterTree()
	{
		base._EnterTree();
		//If the Generic Core is already connected, spawn the object
		if(GenericCore.Instance.ThinksItsConnected && GenericCore.Instance.IsServer()){
			Spawn();
		}else if(!GenericCore.Instance.ThinksItsConnected && GenericCore.Instance.IsServer()){
			//If it's not connected, start listening for the connected signal
			GenericCore.Instance.ServerCreated += Spawn;
		}
	}
	
	public override void _Process(double delta){
		base._Process(delta);
		//If the object was deleted and it needs to be respawed, do it
		if(Respawn && TheObject == null && !Spawning){
			Spawning = true;
			Spawn();
			Spawning = false;
		}
	}
	
	//Spawn the object
	async public void Spawn(){
		//Only spawn objects server side
		if(!GenericCore.Instance.IsServer()){
			return;
		}
		//If the object has already spawned the object and repsawn isn't enabled, don't do anything
		if(Spawned && !Respawn){
			return;
		}
		//If the object has spawned and this a respawn, wait before respawning
		if(Spawned && Respawn){
			await ToSignal(GetTree().CreateTimer(RespawnTimer),"timeout");
		}
		//Set up its position so that it spawns relative to its parent
		Vector2 SpawnPosition = new Vector2(Position.X, Position.Y);
		//If the parent is also a Node2D, add its position in case the parent was off centered
		Node Parent = GetParent();
		if(Parent is Node2D){
			Node2D Parent2D = (Node2D)Parent;
			SpawnPosition += Parent2D.Position;
		}
		//Spawn the object
		TheObject = GenericCore.Instance.MainNetworkCore.SpawnObject(
			SpawnIndex, 
			new Vector3(SpawnPosition.X, SpawnPosition.Y, 0), 
			new Vector3(Rotation, Rotation, Rotation), 
			new Vector3(Scale.X, Scale.Y, 1), 
			1
		);
		Spawned = true;
	}
}
