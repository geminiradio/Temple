using UnityEngine;
using System.Collections;

public class DEBUG_Checker : MonoBehaviour {

    public bool checkScaleForChanges = true;
    public Vector3 lastKnownScale;

	// Use this for initialization
	void Start () {

        if (checkScaleForChanges)
        {
            lastKnownScale = transform.localScale;
            Debug.Log("Starting localScale is "+lastKnownScale);
        }
	
	}
	
	// Update is called once per frame
	void Update () {

        if (transform.localScale != lastKnownScale)
        {
            Debug.Log(this.gameObject+" localScale just changed from "+lastKnownScale+" to "+transform.localScale);
            lastKnownScale = transform.localScale;
        }


	
	}
}
