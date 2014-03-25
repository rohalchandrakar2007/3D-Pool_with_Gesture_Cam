using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class MainStickControl : MonoBehaviour {
    private PXCUPipeline pp;
    private PXCMGesture.GeoNode[] ndata = new PXCMGesture.GeoNode[5];
	// Use this for initialization
	void Start () 
    {
        pp = new PXCUPipeline();

	}
	
	// Update is called once per frame
	void Update () 
    {

        if (pp == null) print("");
        if (!pp.AcquireFrame(false)) return;
        if (pp.QueryGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, ndata))
            print("geonode palm (x=" + ndata[0].positionImage.x + ", z=" + ndata[0].positionImage.z + ")");
	}
}
