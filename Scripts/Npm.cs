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
	
	public override void _EnterTree(){
		//Give client authority to the client synchronizer
		ClientSynchronizer.GiveAuthority();
		base._EnterTree();
	}
	
	public override void _Ready(){
		base._Ready();
		//If this is the local client, get the data from the database amd send it to the server
		if(ClientSynchronizer.IsLocal){
			RpcId(1, "SpawnPlayer", 
				Database.Instance.Character, 
				Database.Instance.CharacterColor, 
				Database.Instance.PlayerName,
				Database.Instance.Score,
				Database.Instance.Health,
				Database.Instance.Port
			);
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
		
		//Spawn their character based on their chosen character
		ThePlayer = (Character)GenericCore.Instance.MainNetworkCore.SpawnObject(
			0+Character, new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(1,1,1), Multiplayer.GetRemoteSenderId());
		
		//Set their stats
		ThePlayer.SetHealth(Health);
		ThePlayer.ChangeName(PlayerName);
		ThePlayer.ChangeColor(CharacterColor);
	}
}
