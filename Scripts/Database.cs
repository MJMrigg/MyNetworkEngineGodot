using Godot;
using System;

public partial class Database : Label
{
	//Player variables
	public int Port = 7000;
	public int Score = 0;
	public string PlayerName = "No Name";
	public float Health = 100;
	public int Character = 0; //The selected character
	public Color CharacterColor = new Color(1,1,1); //The character's color
	[Export] public TextureRect CharacterImage; //Change the textureRect when the player changes their character
	
	//Make this a singleton(believe me, this will make it hecka easier
	public static Database Instance {get; private set;}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	//Change the port corresponding with the level
	public void SetLevel(int NewPort){
		Port = 7000+NewPort;
		GenericCore.Instance.Port = Port;
	}
	
	//Change the score text
	public void ChangeScore(int Change){
		Score += Change;
		Text = "Score: "+Score;
	}
	
	//Change the name
	public void ChangeName(string NewName){
		PlayerName = NewName;
	}

	public void ChangeCharacter(int NewCharacter){
		Character = NewCharacter;
		string NewImage = "";
		switch(NewCharacter){
			case 0: //Square
				NewImage = "uid://cjdxcnsa4iq4y";
				break;
			case 1: //Circle
				NewImage = "uid://08na0i7kethe";
				break;
			case 2: //Triangle
				NewImage = "uid://c2j6gov3uc8tv";
				break;
			case 3: //Star
				NewImage = "uid://dd82cwhu65ctw";
				break;
			default:
				return;
		}
		CharacterImage.Texture = (Texture2D)(GD.Load(NewImage));
	}
	
	public void ChangeColor(Color NewColor){
		CharacterColor = NewColor;
		CharacterImage.Modulate = CharacterColor;
	}
	
	//Show/hide the menu when the client connects/disconnects to the server
	public void ShowScoreOnConnect(int PeerId){
		if(GenericCore.Instance.IsServer()){
			return;
		}
		Visible = true;
	}
	public void HideScoreOnDisconnect(){
		if(GenericCore.Instance.IsServer()){
			return;
		}
		Visible = false;
	}
}
