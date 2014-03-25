
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Xml;


public class ImagePlayback : MonoBehaviour 
{
    private Texture2D 	 		rgbImage=null;
	private Texture2D	 		labelMapImage=null;
	private short[]		 		depthmap=null;
	private Vector3[]			pos2d=null;
	private float[]             untrusted=new float[2]{0,0};
	private PXCUPipeline 		pp=null;
	private PXCMFaceAnalysis.Landmark.LandmarkData[] 	ldata=new PXCMFaceAnalysis.Landmark.LandmarkData[2];
	private PXCMGesture.GeoNode[] 						ndataRight=new PXCMGesture.GeoNode[5];
    private PXCMGesture.GeoNode[] ndataLeft = new PXCMGesture.GeoNode[5];
    private PXCMGesture.GeoNode[,] ndata = new PXCMGesture.GeoNode[2,10];
    private PXCMGesture.GeoNode[][] nodes = new PXCMGesture.GeoNode[2][] { new PXCMGesture.GeoNode[11], new PXCMGesture.GeoNode[11] };
    private PXCMGesture.Gesture[] gestures = new PXCMGesture.Gesture[2];
    private PXCMGesture.Gesture gdata;
    public GameObject camTarget;
    public GameObject stick,ball,stickBall,secondaryCamera;
    public static Vector3 stickStillPos;
    private Transform stickStillTranform;
    private bool Grabbed = false ,grabbable=false ,queBallMovement=false;
    private float rotSpeed,stickLounchVelocity=0;
    private Vector3 stickBackTransform;
    public static String playerTurn="", playeBallTypeAssignedPlayer1, playeBallTypeAssignedPlayer2,winQuats;
    public static bool isPlayerPlayed ,playerPlayedStopper ,menuEnable;
    private int[] ballPottStatus = new int[]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    private int turnCount;
    private int rotateAroundAxis ;
    static int scrW = Screen.width;
    static int scrH = Screen.height;
    private static double xPos = 0;
    private static double yPos = 0;
    // The distance in the x-z plane to the target
    //private float distance = 10.0;
    // the height we want the camera to be above the target
    private float height = 29.0f;

	
    void Start () {
		/* VoiceRecognition is in a separate thread. See VoiceThread.cs. */
        PXCUPipeline.Mode mode = PXCUPipeline.Mode.GESTURE;// Options.mode & (~PXCUPipeline.Mode.VOICE_RECOGNITION);
        if (mode == 0) return;
		pp=new PXCUPipeline();
        if (!pp.Init(mode))
        {
            print("Unable to initialize the PXCUPipeline");
            return;
        }
        rotSpeed = 0;

        
        RepositionCamera();
        
        playerTurn = "Player1";
        playeBallTypeAssignedPlayer1 = "NA";
        playeBallTypeAssignedPlayer2 = "NA";
        winQuats = "";
        menuEnable = false;
        isPlayerPlayed = false;
        turnCount = 0;
        playerPlayedStopper = false;
        Screen.showCursor = false;
    }
    
    void OnDisable() {
		if (pp==null) return;
		pp.Dispose();
		pp=null;
    }

    void Update()
    {
       if(!menuEnable)
        {
		if (pp==null) return;
		if (!pp.AcquireFrame(false)) return;
              #region  query for Gnode data
        if (pp.QueryGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT, nodes[0])) ;
           // print("geonode palm right (x=" + nodes[0][0].positionImage.x + ", y=" + nodes[0][0].positionImage.y + ")");
        if (pp.QueryGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT, nodes[1])) ;
        //    print("geonode palm left (x=" + nodes[1][0].positionImage.x + ", y=" + nodes[1][0].positionImage.y + ")");
        
        #endregion
        if (nodes[0][0].openness > 70 && nodes[1][0].openness > 70)
        {
            rotSpeed = 0;
        }
        UpdateStickPos();
       
        if (!Grabbed && stickLounchVelocity >= 0.0f)
        {
            stick.transform.Translate(stickLounchVelocity * Vector3.up, Space.Self);
            stickLounchVelocity -= 0.01f;
        }
        if (stickLounchVelocity < 0.0f)
        {
            stickLounchVelocity = 0;
        }
            if(IsNoMovement() && stickLounchVelocity==0 && !Grabbed)
       {
          
      
        stick.transform.position = this.transform.position;
       stick.transform.Translate(0, -10.37f, 7.20f, this.transform);
       if (queBallMovement == true && ball.rigidbody.velocity.magnitude == 0 && IsAllStill())
       {
           turnCount = turnCount + 1;
           print("respawn the cam...");
           if (isPlayerPlayed)
           {
               //************** Code for the turn complition and balls are still********************//
               BallPottStatus();
               TurnCalculation();
               ClearPottPockets();
               isPlayerPlayed = false;
           }
           queBallMovement = false;
           RepositionCamera();
       }
           
       }
        
        #region query for gestures
        
        if (pp.QueryGesture(PXCMGesture.GeoNode.Label.LABEL_ANY, out gdata))
        {
           
            if (gdata.label.ToString() == "LABEL_NAV_SWIPE_LEFT" && !isPlayerPlayed && !Grabbed)
            {
                this.transform.LookAt(camTarget.transform.position);
                //print("Do  It!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                rotSpeed = 100;
                rotateAroundAxis = -1;
               
            }
            else
            {
                if (gdata.label.ToString() == "LABEL_NAV_SWIPE_RIGHT" && !isPlayerPlayed && !Grabbed)
                {
                   
                    this.transform.LookAt(camTarget.transform.position);
                    //print("Do  It!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    rotSpeed = 100;
                    rotateAroundAxis = 1;
                  }
            }
            if (gdata.label.ToString() == "LABEL_NAV_SWIPE_DOWN" && !isPlayerPlayed && !Grabbed)
            {
                print("Swipe down!!!");
                menuEnable = true;
                UpdateGui("Enable");
            }
           

        }
#endregion
        
        #region Camera revolution (rotSpeed)
        if (rotSpeed > 0.5f)
        {
            this.transform.LookAt(camTarget.transform.position);
            this.transform.RotateAround(camTarget.transform.position, new Vector3(0, rotateAroundAxis, 0), rotSpeed * Time.deltaTime);
            rotSpeed = rotSpeed - 0.5f;
           // print(rotSpeed.ToString());
        }
        else
        {
            rotSpeed = 0;
        }
        #endregion



       
        IsAllStillLogic();
            if(nodes[0][0].positionImage.x>0 && nodes[1][0].positionImage.x>0)
        SlowCameraRotateUpdate();
        IsPalmOpenFeedBack();
        GrabForwardTransitionFeedBack();
        //BallPottStatus();

        pp.ReleaseFrame();
        }
        else
        {
        MenuFunction();
        }
    }

    private void MenuFunction()
    {
       if (pp==null) return;
		if (!pp.AcquireFrame(false)) return;
        #region  query for Gnode data
        if (pp.QueryGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, nodes[0])) ;
           // print("geonode palm right (x=" + nodes[0][0].positionImage.x + ", y=" + nodes[0][0].positionImage.y + ")");
       

        //if (pp.QueryGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT, nodes[1])) ;
        //    print("geonode palm left (x=" + nodes[1][0].positionImage.x + ", y=" + nodes[1][0].positionImage.y + ")");
        
        #endregion

        #region query for gestures

        if (pp.QueryGesture(PXCMGesture.GeoNode.Label.LABEL_ANY, out gdata))
        {
                if (gdata.label.ToString() == "LABEL_NAV_SWIPE_UP" && menuEnable)
                {
                    print("Swipe down!!!");
                    if (!winQuats.Equals("")) 
                        Application.LoadLevel("scene");
                    menuEnable = false;
                    UpdateGui("Disable");
                  }
              }
        #endregion
        MouseCurzorUpdate();
        pp.ReleaseFrame();
    }

    private void MouseCurzorUpdate()
    {
        
    }

    private void UpdateGui(String status)
    {
        if (status.Equals("Enable"))
        {
            print("GUI menu Enable...");
            Screen.showCursor = true;

        }
        if (status.Equals("Disable"))
        {
            print("GUI menu Disable...");
            Screen.showCursor = false;
        }
            }

    private void IsAllStillLogic()
    {
          if (isPlayerPlayed && !playerPlayedStopper)
        {
            print("Enter IsStillLogic()...");
            stick.SetActive(false);
            playerPlayedStopper = true;
            secondaryCamera.camera.rect = new Rect(0,0.5f,1,0.5f);
            this.camera.rect = new Rect(0, 0, 1, 0.5f);
        }
        if (IsAllStill() && ball.rigidbody.velocity.magnitude == 0 && playerPlayedStopper)
        {
            print("Exit IsStillLogic()... ");
             stick.SetActive(true);
            playerPlayedStopper = false;
            this.camera.rect = new Rect(0,0,1,1);
            secondaryCamera.camera.rect = new Rect(0,0,0,0);
              }
       


    }

    private void ClearPottPockets()
    {
        for (int i = 0; i < 15; i++)
        {
            if (ballPottStatus[i] == 1)
            {
                GameObject.Find("Ball" + ((i + 1).ToString())).transform.position = new Vector3(-8, 3.4f, 0);// = new Vector3(-8, 3.4, 0);
                ballPottStatus[i] = 2;
            }
            
        }
    }

    private void SlowCameraRotateUpdate()
    {
        float nodeDifference10 = nodes[1][0].positionWorld.y - nodes[0][0].positionWorld.y;
        float nodeDifference01 = nodes[0][0].positionWorld.y - nodes[1][0].positionWorld.y;
        if (((nodes[0][0].openness > 75 || nodes[1][0].openness > 75) || (nodes[1][0].openness > 75 || nodes[0][0].openness > 75)) && !isPlayerPlayed && !Grabbed)
        {
           if (nodes[0][0].positionImage.x < nodes[1][0].positionImage.x)
            {
                if ((nodeDifference10) > 0.15f)
                { 
                  this.transform.LookAt(camTarget.transform.position);
                this.transform.RotateAround(camTarget.transform.position, new Vector3(0, 1, 0), 100 * nodeDifference10 * Time.deltaTime);
                }
            }
            else
            {
                if ((nodeDifference01) > 0.15f)
                {
                    this.transform.LookAt(camTarget.transform.position);
                    this.transform.RotateAround(camTarget.transform.position, new Vector3(0, 1, 0), 100 * nodeDifference01 * Time.deltaTime);
                }
            }



            if (nodes[1][0].positionImage.x > nodes[0][0].positionImage.x)
            {
                if ((nodeDifference01) > 0.15f)
                {
                    this.transform.LookAt(camTarget.transform.position);
                    this.transform.RotateAround(camTarget.transform.position, new Vector3(0, -1, 0), 100 * nodeDifference01 * Time.deltaTime);
                }
            }
            else
            {
                if ((nodeDifference10) > 0.15f)
                {
                    this.transform.LookAt(camTarget.transform.position);
                    this.transform.RotateAround(camTarget.transform.position, new Vector3(0, -1, 0), 100 *nodeDifference10* Time.deltaTime);
                }
            }
        }
    }

    private void UpdateCameraHoldGesture()
    {
       
    }

    private void UpdateStickPos()
    {
        if (Grabbed && stickLounchVelocity == 0 && IsAllStill() && rotSpeed <= 0 && nodes[0][0].positionImage.x != 0 && nodes[1][0].positionImage.x != 0 && (nodes[0][0].openness < 20  ||  nodes[1][0].openness < 20) && (this.rigidbody.velocity.magnitude == 0))
        {

            stick.transform.position = stickStillPos;
            stick.transform.Translate(0, -(nodes[0][0].positionWorld.y - 0.10f) * 50, 0, stick.transform);
        }
    }
	
	void AddProjection() {
		for (int xy=0;xy<pos2d.Length;xy++)
			pos2d[xy].z=depthmap[xy];
		
		Vector2[] posc;
		if (!pp.MapDepthToColorCoordinates(pos2d,out posc)) return;
		
		Color32[] pixels=rgbImage.GetPixels32();
		for (int xy=0;xy<posc.Length;xy++) {
			if (depthmap[xy]==untrusted[0] || depthmap[xy]==untrusted[1]) continue;
			int x=(int)posc[xy].x, y=(int)posc[xy].y;
			if (x<0 || x>=rgbImage.width || y<0 || y>=rgbImage.height) continue;
			pixels[y*rgbImage.width+x]=new Color32(0,255,0,255);
		}
		rgbImage.SetPixels32(pixels);
	}
    private void DisplayGeoNodes()
    {
      
               
    }
    private bool GrabForwardTransition()
    {
        if (grabbable && !IsPalmOpen() && (nodes[0][0].positionWorld.y < 0.35f) && !Grabbed)
        {
            Grabbed = true;
        }
        else
        {
            Grabbed = false;
        }
        return Grabbed;
        }
    private bool IsPalmOpen()
    {
        if ((nodes[0][0].opennessState.ToString() == "LABEL_OPEN" ) || nodes[0][0].body.ToString() == "LABEL_ANY")
        
        {
            grabbable = true;
            return true;
        }
        else
            return false;
       
    }
    private bool IsNoMovement()
    {
        try
        {
            if ( ball.rigidbody.velocity.magnitude < 0.05)
                return true;
            else
                return false;
        }
        catch (Exception e)
        {
            print("Expected exception!!!!");
            return false;
        }
    }
    private void IsPalmOpenFeedBack()
    {
        
        if (nodes[0][0].opennessState.ToString() == "LABEL_OPEN" || nodes[0][0].body.ToString() == "LABEL_ANY" )
        {
            grabbable = true;
            if (Grabbed)
            {
                stickLounchVelocity =  Vector3.Distance(stickStillPos, stick.transform.position)/10;
            }
            Grabbed = false;
          
        }
        if (ball.rigidbody.velocity.magnitude > 0)
            queBallMovement = true;
        

    }
    private void GrabForwardTransitionFeedBack()
    {
        if (grabbable && !IsPalmOpen() && rotSpeed == 0 && !(nodes[0][0].body.ToString() == "LABEL_ANY") && IsNoMovement() && (nodes[0][0].openness < 30 || nodes[1][0].openness < 30) && (nodes[0][0].positionWorld.y < 0.25) && (nodes[1][0].positionWorld.y < 0.25))
        {
            print("inc......");
            Grabbed = true;
            stickStillPos = stick.transform.position;
            stickStillTranform = stick.transform;
            stickBackTransform = stick.transform.TransformDirection(Vector3.down);
           grabbable = false;
        }
        else
        {}
        if (IsPalmOpen())
        {
              Grabbed = false;
        }
    }
    private void LateUpdateo()
    {
        // Early out if we don't have a target
        if (!ball.transform)
            return;

        // Calculate the current rotation angles
        float wantedRotationAngle = ball.transform.eulerAngles.y;
        float wantedHeight = ball.transform.position.y + height;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, 0 * Time.deltaTime);
        
        // Damp the height
       currentHeight = Mathf.Lerp(currentHeight, wantedHeight, 0 * Time.deltaTime);

        // Convert the angle into a rotation
        var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        transform.position = ball.transform.position;
        transform.position -= currentRotation * Vector3.forward * 30;

        // Set the height of the camera
          this.transform.position.Set(this.transform.position.x,29.1f,this.transform.position.z);

        // Always look at the target
        transform.LookAt(ball.transform);
    }
    private Vector3 CalculatePoint(Vector3 a, Vector3 b, float distance)
    {
        Vector3 newPoint = new Vector3(10,0,10);
        Double Magnitude = Math.Sqrt(Math.Pow((b.z - a.z), 2) + Math.Pow((b.x - a.x), 2));
        newPoint.x = (float)(a.x + (distance * ((b.x - a.x) / Magnitude)));
        newPoint.z = (float)(a.z + (distance * ((b.z - a.z) / Magnitude)));
        return newPoint;
    }
    private void RepositionCamera()
    {
       Vector3 temp = CalculatePoint1(new Vector3(0, 0, 0), new Vector3(ball.transform.position.x, 0, ball.transform.position.z), 31.28f - 6.80f);
        this.transform.position = new Vector3(temp.x,29.03f,temp.z);
        this.transform.LookAt(camTarget.transform.position);
        SaveGameStatus();
        //stick.SetActive(true);
    }
    private static Vector3 CalculatePoint1(Vector3 a, Vector3 b, float distance)
    {
        Vector3 tempUnitVector = Vector3.Normalize(b - a);
        //float tempMagnitude = Vector3.Normalize(b - a);

        // d. calculate and Draw the new vector,
        return (new Vector3(distance*tempUnitVector.x,29.03f,distance*tempUnitVector.z) )+ b;
    }
    private void BallPottStatus()
    {
        try
        {
            for (int i = 0; i < 15; i++)
            {
                if (GameObject.Find("Ball" + ((i + 1).ToString())).transform.position.y < 15 && GameObject.Find("Ball" + ((i + 1).ToString())).transform.position.y > 9)
                {
                     ballPottStatus[i] = 1;
                }
                else
                { ballPottStatus[i] = 0; }

                if (GameObject.Find("Ball" + ((i + 1).ToString())).transform.position.y < 3)
                {
                      print("second 1...");
                }
            }
        }
        catch(Exception e)
        {}
    }
    private bool IsAllStill()
    {
        try
        {
            for (int i = 0; i <= 14; i++)
            {
                if (GameObject.Find("Ball" + ((i + 1).ToString())).rigidbody.velocity.magnitude != 0)
                    return false;
                if (i == 14)
                    return true;
            }
        }
        catch (Exception e)
        { }
        return true;
    }
    private void TurnCalculation()
    {
      
        try
        {
             if (playeBallTypeAssignedPlayer1.Equals("NA") && playeBallTypeAssignedPlayer2.Equals("NA"))
            {
                //print(ballPottStatus[7].ToString());
                if (ballPottStatus[7] == 1 && playerTurn.Equals("Player1"))
                {
                    print("Player2 wins...");
                    winQuats = "Player2 wins...";
                    menuEnable = true;
                }
                if (ballPottStatus[7] == 1 && playerTurn.Equals("Player2"))
                {
                    print("Player1 wins...");
                    winQuats = "Player1 wins...";
                    menuEnable = true;
                }
                if (turnCount >= 2)
                {
                    if (PlayerBallAssign().Equals("Both"))
                    { }
                    if (PlayerBallAssign().Equals("Solid"))
                    {
                        if (playerTurn.Equals("Player1"))
                        {
                            playeBallTypeAssignedPlayer1 = "Solid";
                            playeBallTypeAssignedPlayer2 = "Stripe";
                        }
                        if (playerTurn.Equals("Player2"))
                        {
                            playeBallTypeAssignedPlayer2 = "Solid";
                            playeBallTypeAssignedPlayer1 = "Stripe";
                        }
                    }
                    if (PlayerBallAssign().Equals("Stripe"))
                    {
                        if (playerTurn.Equals("Player1"))
                        {
                            playeBallTypeAssignedPlayer1 = "Stripe";
                            playeBallTypeAssignedPlayer2 = "Solid";
                        }
                        if (playerTurn.Equals("Player2"))
                        {
                            playeBallTypeAssignedPlayer2 = "Stripe";
                            playeBallTypeAssignedPlayer1 = "Solid";
                        }
                    }

                }
               
                if (PlayerBallAssign().Equals("Nothing") || ballPottStatus[7] == 1)
                {
                    print(PlayerBallAssign() + "     " + ballPottStatus[0] + "     " + ballPottStatus[1] + "     " + ballPottStatus[2] + "     " + ballPottStatus[3]);
                    print(playerTurn);
                    ChangePlayerTurn();
                    print(playerTurn);
                }
                else
                    print("Dues");

               

            }
            else
            {
               
                if (ballPottStatus[7] == 1 && playerTurn.Equals("Player1"))
                {
                    if (playeBallTypeAssignedPlayer1.Equals(WhichBallClear()))
                    {
                        print("Player1 wins...");
                        winQuats = "Player1 wins...";
                        menuEnable = true;
                    }
                    else
                    {
                        print("Player2 wins...");
                        winQuats = "Player2 wins...";
                        menuEnable = true;
                    }
                }
                if (ballPottStatus[7] == 1 && playerTurn.Equals("Player2"))
                {
                    if (playeBallTypeAssignedPlayer2.Equals(WhichBallClear()))
                    {
                        print("Player2 wins...");
                        winQuats = "Player2 wins...";
                        menuEnable = true;
                    }
                    else
                    {
                        print("Player1 wins...");
                        winQuats = "Player2 wins...";
                        menuEnable = true;
                    }
                }
                if (PlayerBallAssign().Equals("Nothing") || ballPottStatus[7] == 0)
                {
                    ChangePlayerTurn();
                }
                else
                    print("No dues");
            }
            if (ball.transform.position.y < 15)
            {
                print("ball POtted!!!");
                ball.transform.position = new Vector3(6, 16.30572f, 0);
                ChangePlayerTurn();
                RepositionCamera();
            }
         }
        catch(Exception e)
        {}

    }

    private void ChangePlayerTurn()
    {
        if (playerTurn.Equals("Player1"))
        {
            playerTurn = "Player2";
        }
        else
        {
            
                playerTurn = "Player1";
        }
        print("Change player turn!!!");
    }
    private String PlayerBallAssign()
    {
        bool solid = ((ballPottStatus[0] == 1) || (ballPottStatus[1] == 1) || (ballPottStatus[2] == 1) || (ballPottStatus[3] == 1) || (ballPottStatus[4] == 1) || (ballPottStatus[5] == 1) || (ballPottStatus[6] == 1));
        bool stripe = ((ballPottStatus[8] == 1) || (ballPottStatus[9] == 1) || (ballPottStatus[10] == 1) || (ballPottStatus[11] == 1) || (ballPottStatus[12] == 1) || (ballPottStatus[13] == 1) || (ballPottStatus[14] == 1));
        if (solid && stripe)
        {
            return "Both";
        }
        if (solid)
            return "Solid";
        if (stripe)
            return "Stripe";

        return "Nothing";
     }
    private String WhichBallClear()
    {
        if ((ballPottStatus[0] == 0 || ballPottStatus[1] == 0 || ballPottStatus[2] == 0 || ballPottStatus[3] == 0 || ballPottStatus[4] == 0 || ballPottStatus[5] == 0 || ballPottStatus[6] == 0))
            return "";
        else 
            return "Solid";

        if ((ballPottStatus[8] == 0 || ballPottStatus[9] == 0 || ballPottStatus[10] == 0 || ballPottStatus[11] == 0 || ballPottStatus[12] == 0 || ballPottStatus[13] == 0 || ballPottStatus[14] == 0))
            return "";
        else
            return "Stripe";

        return "";
    }
    private void SaveGameStatus()
    {
        XmlTextWriter savedGame = new XmlTextWriter("SavedGame.xml",System.Text.Encoding.UTF8);
        savedGame.WriteStartElement("GameComponents");
        //////////////////////// All the game component here //////////////////////////////
        for (int i = 0; i < 15; i++)
        {
            savedGame.WriteStartElement("Ball" + ((i + 1).ToString()));
            savedGame.WriteStartElement("X");
            
            savedGame.WriteEndElement();
            savedGame.WriteStartElement("Y");
            savedGame.WriteEndElement();
            savedGame.WriteStartElement("Z");
            savedGame.WriteEndElement();
            savedGame.WriteEndElement();
        }

        savedGame.WriteStartElement("Qball");
        savedGame.WriteEndElement();

            ///////////////////////////////////////////////////////////////////////////////////
            savedGame.WriteEndElement();
            savedGame.Close();
    }
    
}
