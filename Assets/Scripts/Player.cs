using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
     
    private Transform hmd;
    private Collider playerCollider;

    public float playerHeightOffset = -0.05f;  // how far above(pos)/below(neg) the hmd height to scale the player's collider.  hmd height is roughly the player's eyes.

    // Use this for initialization
    void Start () {
	
        hmd = GameObject.Find("Camera (head)").transform;
        if (hmd == null)
            Debug.LogError("Player object could not find object with name Camera (head) - ie - the SteamVR hmd object");

        if (playerCollider == null)
            playerCollider = GetComponent<Collider>();
        if (playerCollider == null)
            Debug.LogError("playerCollider is not assigned and could not be found.");


    }

    // Update is called once per frame
    void Update () {

        UpdateCollider();
	}

    private void UpdateCollider()
    {
        CodeTools.CopyTransform(hmd, playerCollider.transform, false);

        // used for both y position and y scale
        float top = (hmd.position.y / 2) + playerHeightOffset;

        playerCollider.transform.position = new Vector3(playerCollider.transform.position.x, top, playerCollider.transform.position.z);
        playerCollider.transform.localScale = new Vector3(playerCollider.transform.localScale.x, top, playerCollider.transform.localScale.z);

        // don't rotate around X or Z, only Y
        Quaternion rotQ = playerCollider.transform.rotation;
        Vector3 rotE = rotQ.eulerAngles;
        rotE.x = 0;
        rotE.z = 0;
        playerCollider.transform.rotation = Quaternion.Euler(rotE.x, rotE.y, rotE.z);       
    }


}
