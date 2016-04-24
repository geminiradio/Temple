using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class EditorScripting : MonoBehaviour {

	public bool triggerDistribute = false;
	public GameObject toDistribute;
	public int numToDistribute;
	public float xMin, xMax, yMin, yMax, zMin, zMax;
	public bool rotateOnY = true;

	public GameObject[] distributedObjects;

    public bool triggerStore = false; // toggle this in game mode to copy transforms from toStore to storedTransforms
    public GameObject[] toStoreOrCopy; // assign this in editor
	public Vector3[] storedPositions; // these get stored
	public Vector3[] storedRotations; // these get stored
    public Vector3[] storedScales; // these get stored
    public bool triggerCopy = false; // toggle this in editor to copy transforms from storedTransforms to toStoreOrCopy

	public bool triggerReplace = false;
	public GameObject[] toReplace;  // assign in editor
	public GameObject[] replaceWith;  // it picks from this array at random to replace


	void Start ()
	{
		distributedObjects = new GameObject[200];
		storedPositions = new Vector3[200];
		storedRotations = new Vector3[200];
        storedScales = new Vector3[200];

    }

    // Update is called once per frame
    void Update () {

		if (triggerDistribute)
		{
			triggerDistribute = false;
			RemoveAllOldObjects();
			DistributeObjects();
		}

        if (triggerStore)
        {
            triggerStore = false;
            for (int i = 0; i < toStoreOrCopy.Length; i++)
			{
				storedPositions[i].x = toStoreOrCopy[i].transform.position.x;
				storedPositions[i].y = toStoreOrCopy[i].transform.position.y;
				storedPositions[i].z = toStoreOrCopy[i].transform.position.z;

				storedRotations[i].x = toStoreOrCopy[i].transform.rotation.x;
				storedRotations[i].y = toStoreOrCopy[i].transform.rotation.y;
				storedRotations[i].z = toStoreOrCopy[i].transform.rotation.z;

                storedScales[i].x = toStoreOrCopy[i].transform.localScale.x;
                storedScales[i].y = toStoreOrCopy[i].transform.localScale.y;
                storedScales[i].z = toStoreOrCopy[i].transform.localScale.z;
            }
        }

		if (triggerCopy)
		{
			triggerCopy = false;

//			Vector3 newpos, newrot, 
            Vector3 newscale;

//			newpos = new Vector3();
//			newrot = new Vector3();
            newscale = new Vector3();

            for (int i = 0; i < toStoreOrCopy.Length; i++)
			{
            /*
				newpos.x = storedPositions[i].x;
				newpos.y = storedPositions[i].y;
				newpos.z = storedPositions[i].z;
				toStoreOrCopy[i].transform.position = newpos;

				newrot.x = storedRotations[i].x;
				newrot.y = storedRotations[i].y;
				newrot.z = storedRotations[i].z;
				toStoreOrCopy[i].transform.rotation = Quaternion.Euler(newrot);
                */

                newscale.x = storedScales[i].x;
                newscale.y = storedScales[i].y;
                newscale.z = storedScales[i].z;
                toStoreOrCopy[i].transform.localScale = newscale;
            }


		}


		if (triggerReplace)
		{
			triggerReplace = false;

			Vector3 replacePos, replaceRot;
			replacePos = new Vector3();
			replaceRot = new Vector3();

			//GameObject replacement;

			for (int i = 0; i < toReplace.Length; i++)
			{
				// copy data from the object we are going to replace
				replacePos.x = toReplace[i].transform.position.x;
				replacePos.y = toReplace[i].transform.position.y;
				replacePos.z = toReplace[i].transform.position.z;

				replaceRot.x = toReplace[i].transform.rotation.x;
				replaceRot.y = toReplace[i].transform.rotation.y;
				replaceRot.z = toReplace[i].transform.rotation.z;

				// pick the object we are going to clone
				// below is commented out to avoid warnings
				//replacement = replaceWith[Random.Range(0,replaceWith.Length)];
				//GameObject clone = (GameObject)Instantiate(replacement,replacePos,Quaternion.Euler(replaceRot)) as GameObject;

			}

		}

	
	}

	void RemoveAllOldObjects()
	{

		GameObject go;
		for (int i=0;i<distributedObjects.Length;i++)
		{
			if (distributedObjects[i] != null)
			{
				go = distributedObjects[i];
				DestroyImmediate(go);
			}
		}

	}


	void DistributeObjects()
	{
		GameObject go;
		for (int i=0; i<numToDistribute; i++)
		{
			Vector3 loc = new Vector3 ( Random.Range(xMin,xMax), Random.Range(yMin,yMax), Random.Range(zMin,zMax) );
			Vector3 rot = new Vector3 (0, 0, 0);
			if (rotateOnY)
				rot.y = Random.Range(0,360);
			
			go = (GameObject) Instantiate(toDistribute,loc,Quaternion.Euler(rot));
			go.transform.parent = transform;
			distributedObjects[i] = go;
		}

	}
}
