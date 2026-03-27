using Godot;
using System;

public partial class Npm : Control
{
	[Export] public NetId ClientSynchronizer;
	[Export] public NetId ServerSynchronizer;
	
	//Character variables
	public int Score = 0;
	public string PlayerName = "No Name";
	public float Health = 100;
	public int MaxHealth = 100;
	public int Character = 0; //The selected character
	public Color CharacterColor = new Color(1,1,1); //The character's color
	
	//The character itself
	public Character ThePlayer;
	
	//Whether the player has spawned or not
	public bool Spawned = false;
	
	public override void _EnterTree(){
		//Give client authority to the client synchronizer
		ClientSynchronizer.GiveAuthority();
		base._EnterTree();
	}
	
	public override void _Ready(){
		base._Ready();
		//If this is the local client, get the data from the database amd send it to the server
		if(ClientSynchronizer.IsLocal && !GenericCore.Instance.IsServer()){
			RpcId(1, "SpawnPlayer", 
				Database.Instance.Character, 
				Database.Instance.CharacterColor, 
				Database.Instance.PlayerName,
				Database.Instance.Score,
				Database.Instance.Health,
				Database.Instance.Port
			);
			//Set the database's port to be this port
			Database.Instance.Port = GenericCore.Instance.Port;
		}
	}
	
	//Spawn the player character
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SpawnPlayer(int ChosenCharacter, Color ChosenColor, string ChosenName, int CurrentScore, float CurrentHealth, int PreviousPort){
		if(!GenericCore.Instance.IsServer()){
			return;
		}
		
		//Store their information
		Character = ChosenCharacter;
		CharacterColor = ChosenColor;
		PlayerName = ChosenName;
		Score = CurrentScore;
		Health = CurrentHealth;
		
		//Get the player's spawn position based on their last port
		//Use the viewport size to set the default spawn point
		Vector2 Center = GetViewportRect().Size;
		Vector3 SpawnPosition = new Vector3(Center.X/2,4*Center.Y/5,0);
		var SpawnPoints = GetTree().GetNodesInGroup("SpawnPoints");
		foreach(var SpawnLocation in SpawnPoints){
			if(!(SpawnLocation is SpawnPoint)){
				return;
			}
			SpawnPoint Location = (SpawnPoint)SpawnLocation;
			if(Location.LastPort == PreviousPort){
				SpawnPosition = new Vector3(Location.Position.X, Location.Position.Y, 0);
				break;
			}
		}
		
		//Spawn their character based on their chosen character
		ThePlayer = (Character)GenericCore.Instance.MainNetworkCore.SpawnObject(
			0+Character, SpawnPosition, new Vector3(0,0,0), new Vector3(1,1,1), ClientSynchronizer.GetMultiplayerAuthority());
		
		//Set their stats
		ThePlayer.SetHealth(Health);
		ThePlayer.ChangeName(PlayerName);
		ThePlayer.ChangeColor(CharacterColor);
		
		//Mark player as spawned
		Spawned = true;
	}
	
	public override void _Process(double delta){
		base._Process(delta);
		if(GenericCore.Instance.IsServer()){
			//If the player died, respawn them
			if(Spawned && !GodotObject.IsInstanceValid(ThePlayer)){
				Spawned = false; //So that they don't respawn twice
				Health = 100;
				SpawnPlayer(Character, CharacterColor, PlayerName, Score, Health, 0);
			}
		}else{
			if(ClientSynchronizer.IsLocal){
				
			}else{
				
			}
		}
	}
}
