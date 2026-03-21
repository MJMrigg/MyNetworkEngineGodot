using Godot;
using System;

public partial class Menu : VBoxContainer
{
	[Export] public VBoxContainer NetworkingSection;
	[Export] public Button EndButton;
	[Export] public LineEdit PortText;
	
	public string IpAddress = "";
	public string PortNumber = "";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("Pause") && GenericCore.Instance.ThinksItsConnected){
			Visible = !Visible;
		}
	}
	
	//When the server is started, hide the join button
	public void CreateGame(){
		NetworkingSection.Visible = false;
		EndButton.Text = "End Game";
		GenericCore.Instance.CreateGame();
	}
	
	//When the player joins a game, hide the menu and the join button
	public void JoinGame(){
		NetworkingSection.Visible = false;
		Visible = false;
		//Change port and IP address
		if(IpAddress != "" && IpAddress != null){
			GenericCore.Instance.DefaultServerIP = IpAddress;
		}
		if(PortNumber != "" && PortNumber != null){
			GenericCore.Instance.Port = int.Parse(PortNumber);
		}
		GenericCore.Instance.JoinGame();
	}
	
	//If the player was disconnected for any reason
	public void Disconnected(){
		Visible = true;
		EndButton.Text = "Quit";
		NetworkingSection.Visible = true;
	}
	
	//When the player leaves the game, unhide them menu and its options
	public void LeaveGame(){
		Disconnected();
		GenericCore.Instance.RemoveMultiplayerPeer();
	}
	
	//Make sure the port is an integer at all times
	public void ManagePort(string Change){
		try{
			if(Change == "" || Change == null){
				return;
			}
			int.Parse(Change);
			PortNumber = Change;
		}catch{
			PortText.Text = PortNumber;
			PortText.CaretColumn = PortNumber.Length;
		}
	}
}
