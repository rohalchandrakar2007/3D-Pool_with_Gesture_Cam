using UnityEngine;
using System.Collections;

public class ReloadLevelScript : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
       // Application.LoadLevel("scene");
	}
    void OnMouseDown()
    {
        Application.LoadLevel("scene");
    }
}
