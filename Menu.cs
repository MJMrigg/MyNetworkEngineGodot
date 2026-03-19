using Godot;
using System;

public partial class Menu : VBoxContainer
{
	[Export] public Button JoinButton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void CreateGame(){
		JoinButton.Visible = false;
		GenericCore.Instance.CreateGame();
	}
	
	public void JoinGame(){
		Visible = false;
		GenericCore.Instance.JoinGame();
	}
	
	public void LeaveGame(){
		Visible = true;
		JoinButton.Visible = true;
		GenericCore.Instance.RemoveMultiplayerPeer();
	}
}
