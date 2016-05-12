using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
     
    private Transform hmd;
    public Transform playerEyes;  // things like inventory hotspots should be parented to this
    public Collider playerCollider;

    public RandomizedSFX damageSound;

    public GameObject bloodHitPFX;
    public GameObject bloodDripPFX;

    private int timesHit = 0;

    public float playerHeightOffset = -0.05f;  // how far above(pos)/below(neg) the hmd height to scale the player's collider.  hmd height is roughly the player's eyes.

    // Use this for initialization
    void Start () {
	
        hmd = GameObject.Find("Camera (head)").transform;
        if (hmd == null)
            Debug.LogError("Player object could not find object with name Camera (head) - ie - the SteamVR hmd object");

        if (playerCollider == null)
            Debug.LogError("playerCollider is not assigned.");

        if (playerEyes == null)
            Debug.LogError("playerEyes is not assigned.");

    }


    void Update () {

        UpdateCollider();
	}


    // change the Player root object's position and rotation
    // change the playerCollider's scale (and only change that scale, no other scales)
    private void UpdateCollider()
    {
        // copy them over, but we're going to modify them from there
        CodeTools.CopyTransform(hmd, transform, true, true, false);

        // used for both y position and y scale
        float top = hmd.position.y  + playerHeightOffset;

        transform.position = new Vector3(transform.position.x, (top/2f), transform.position.z);
        playerCollider.transform.localScale = new Vector3(playerCollider.transform.localScale.x, top, playerCollider.transform.localScale.z);

        // don't rotate around X or Z, only Y
        Quaternion rotQ = transform.rotation;
        Vector3 rotE = rotQ.eulerAngles;
        rotE.x = 0;
        rotE.z = 0;
        transform.rotation = Quaternion.Euler(rotE.x, rotE.y, rotE.z);

        // move player Eyes to be the height of the hmd
        playerEyes.transform.position = new Vector3(transform.position.x, hmd.position.y, transform.position.z);
    }


    // called by external scripts
    // passed-in: what touched me, at what point did it hit me, what was the normal
    public void Stimulate(Stimulus stim, Vector3 pos, Vector3 normal)
    {
        if (stim.stimType == StimType.Arrow)  // could potentially also check for magnitude but right now the arrow's default threshold for stim is good for player damage too
        {
            timesHit++;

            SpawnBloodPFX(pos, normal);

            if (TakeDamage(1))
            {
                Debug.Log("*** PLAYER GOT KILT!! ***");
            }
        }
    }

    private void SpawnBloodPFX (Vector3 pos, Vector3 normal)
    {
        GameObject wound = new GameObject();
        wound.transform.position = pos;
        wound.transform.rotation = Quaternion.Euler(normal);  // TODO - this rarely seems to actually point back to where the arrow came from, closer to 90 degrees most of the time
        wound.name = "Wound" + timesHit.ToString();
        wound.transform.parent = transform;

        GameObject blood;

        if (bloodHitPFX != null)
        {
            blood = (GameObject)Instantiate(bloodHitPFX, wound.transform.position, wound.transform.rotation) as GameObject;
            blood.transform.parent = wound.transform;
        }
        if (bloodDripPFX != null)
        {
            blood = (GameObject)Instantiate(bloodDripPFX, wound.transform.position, wound.transform.rotation) as GameObject;
            blood.transform.parent = wound.transform;
        }

    }

    // returns true if this damage killed player, otherwise false
    private bool TakeDamage(int damageAmount)
    {
        if (damageSound != null)
            damageSound.PlaySFX();

        return false;
    }

}
