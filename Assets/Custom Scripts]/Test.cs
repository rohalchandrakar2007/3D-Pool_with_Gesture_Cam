using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
    public static Vector3 stickballPos=Vector3.zero;
    public static bool qballCollider = false;
    public GameObject stickBallLocal,stickLocalForBall;
	void Start () 
    {
    }
	
	void Update () {
	
	}
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "QBall")
        {
             ImagePlayback.isPlayerPlayed = true;
            stickballPos = this.transform.position;
             other.gameObject.rigidbody.velocity = (Vector3.Distance(ImagePlayback.stickStillPos,stickLocalForBall.transform.position)/8)*70* Vector3.Normalize(other.gameObject.transform.position - this.transform.position);
            
            GameObject.Find("Main Camera/stick").transform.position = ImagePlayback.stickStillPos;
        }
       
    }
    
}
