using Godot;
using System;

public partial class Character : CharacterBody2D
{
	//NetIds
	[Export] public NetId ClientSynchronizer;
	[Export] public NetId ServerSynchronizer;
	
	//Player variables
	public float Health = 100.0f;
	public int Score = 0;
	[Export] public float Speed = 300.0f;
	[Export] public Label NameTag;
	[Export] public ProgressBar HealthBar;
	[Export] public Sprite2D Sprite;
	
	//Input variables
	public Vector2 InputedVelocity = Vector2.Zero;
	
	//Reference to the NPM that spawned it
	public Npm DataStore;
	
	public override void _EnterTree(){
		//Give client authority to the client synchronizer
		ClientSynchronizer.GiveAuthority();
		base._EnterTree();
	}
	
	public override void _Ready(){
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if(ServerSynchronizer.IsServer){ //Server
			//Calculate velocity using the velocity from the client and move the object
			Velocity = InputedVelocity.Normalized()*Speed;
			MoveAndSlide();
		}else if(!ClientSynchronizer.IsServer){ //Clients
			if(ClientSynchronizer.IsLocal){ //Local Client
				//Get inputed velocity from the clients
				InputedVelocity = Input.GetVector("Left","Right","Up","Down");
			}else{ //Non-local Clients
				
			}
		}
	}
	
	//Subtract a value from health.
	public void TakeDamage(float Change){
		Health -= Change;
		//Handle if that was the last of their health
		if(Health <= 0){
			QueueFree();
		}else{
			//Tell clients to update the health bar.
			Rpc(MethodName.SetHealthBar, Change);
		}
	}
	
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetHealthBar(float Change){
		HealthBar.Value -= Change;
	}
	
	//Set the health to a number
	public void SetHealth(float Value){
		Health = Value;
		Rpc(MethodName.SetHealthBar, HealthBar.MaxValue-Value);
	}
	
	//Change the name and then center the name tag
	public void ChangeName(string NewName){
		//Get the pixel width of the name
		var NameTagFont = NameTag.GetThemeFont("font");
		int FontSize = NameTag.GetThemeFontSize("font_size");
		Vector2 TextSize = NameTagFont.GetStringSize(NewName, HorizontalAlignment.Left, -1, FontSize);
		//Set name
		NameTag.Text = NewName;
		//Set the position based on the pixel width
		NameTag.SetSize(new Vector2(-0.5f*NewName.Length, NameTag.Size.Y));
		NameTag.SetPosition(new Vector2(-0.5f*TextSize.X, NameTag.Position.Y));
	}
	
	//Change the color of the sprite
	public void ChangeColor(Color NewColor){
		Sprite.Modulate = NewColor;
	}
	
	
	//Change the player's points
	public void GivePoints(){
		Score += 1;
		Rpc(MethodName.ChangeScore);
	}
	
	//Update the score on the screen
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ChangeScore(){
		if(!ClientSynchronizer.IsLocal){
			return;
		}
		Database.Instance.ChangeScore(1);
	}
}
