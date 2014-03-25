using UnityEngine;
using System.Collections;
using System;

public class GuiPlacement : MonoBehaviour {
    public GUIText player1, player2,ballAssignedtoPlayer1,ballAssignedtoPlayer2,winQuatGui;
    public GUITexture stickTexture, exitMenuButton, newGameButton;
    private int stickTextureWidth, stickTextureHeight;
	// Use this for initialization
	void Start ()
    {
        stickTextureWidth = Screen.width / 30;
        stickTextureHeight = (int)(Screen.height / 1.3f);
        stickTexture.pixelInset = new Rect(Screen.width - 40, 0, stickTextureWidth, stickTextureHeight);
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        try
        {
            ///////////////// Player1 & Player2 Gui Placement /////////////////////////
            player1.pixelOffset = new Vector2(30, Screen.height - 20);
            player2.pixelOffset = new Vector2(Screen.width - 200, Screen.height - 20);
            winQuatGui.pixelOffset = new Vector2(Screen.width/2,Screen.height/2+200);
            winQuatGui.text = ImagePlayback.winQuats;


            //////////////// Ball Type Assigned Gui Placement ////////////////////////
            ballAssignedtoPlayer1.pixelOffset = new Vector2(30, Screen.height - 80);
            ballAssignedtoPlayer2.pixelOffset = new Vector2(Screen.width - 200, Screen.height - 80);
            ballAssignedtoPlayer1.text = ImagePlayback.playeBallTypeAssignedPlayer1;
            ballAssignedtoPlayer2.text = ImagePlayback.playeBallTypeAssignedPlayer2;

            /////////////// StickTexture Gui Placement //////////////////////////////
            //print(stickTextureHeight +" "+stickTextureWidth);
           // print(ImagePlayback.playerTurn);
            if (ImagePlayback.playerTurn.Equals("Player1"))
            {
               // print("piche!!!");
                stickTexture.pixelInset = new Rect(0, 0, stickTextureWidth, stickTextureHeight);
            }
            else
                stickTexture.pixelInset = new Rect(Screen.width - 40, 0, stickTextureWidth, stickTextureHeight);

            /////////////////////////// Menu buttons GUI //////////////////////////////////////
           // exitMenuButton.pixelOffset = new Vector2(Screen.width/2,(Screen.height/2)-50);
            exitMenuButton.pixelInset = new Rect((Screen.width/2)-160,(Screen.height/2)-100,320,50);
            newGameButton.pixelInset = new Rect((Screen.width / 2) - 160, (Screen.height / 2) +50, 320, 50);

            GuiStatusUpdate();
        }
        catch(Exception e)
        {}
        
	}

    private void GuiStatusUpdate()
    {
        if (ImagePlayback.menuEnable)
        {
            exitMenuButton.gameObject.SetActive(true);
            newGameButton.gameObject.SetActive(true);

            stickTexture.gameObject.SetActive(false);
            ballAssignedtoPlayer1.gameObject.SetActive(false);
            ballAssignedtoPlayer2.gameObject.SetActive(false);
            player1.gameObject.SetActive(false);
            player2.gameObject.SetActive(false);
            
        }
        else 
        {
            exitMenuButton.gameObject.SetActive(false);
            newGameButton.gameObject.SetActive(false);

            stickTexture.gameObject.SetActive(true);
            ballAssignedtoPlayer1.gameObject.SetActive(true);
            ballAssignedtoPlayer2.gameObject.SetActive(true);
            player1.gameObject.SetActive(true);
            player2.gameObject.SetActive(true);
        }
    }
   
}
