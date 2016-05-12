using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

    public float impulseMag = 15f;  // how quickly it fires
    public float velocityThresholdForStim = 10f;  // velocity collision must be at least this to generate Stim

    private Rigidbody rb;
    private MeshCollider mc;

    private float secsBeforeColliderOn = 0.05f; // enough time for the arrow to have definitely cleared its own base
    private float enableCollisionTime = -1f;

    private int originalLayer;
    private static int dontCollideWithPlayerLayer = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError(this + " Arrow must have a Rigidbody");
        else
            rb.isKinematic = true;

        mc = GetComponentInChildren<MeshCollider>();
        if (mc == null)
            Debug.LogError(this + " Arrow must have a MeshCollider");

        IgnoreAllArrowTrapChildren(true);

        originalLayer = gameObject.layer;

    }

    public void ShootArrow()
    {
        gameObject.layer = originalLayer;
        rb.isKinematic = false;
        rb.AddRelativeForce(Vector3.forward * impulseMag, ForceMode.Impulse);

        enableCollisionTime = Time.time + secsBeforeColliderOn;
    }

    void FixedUpdate()
    {

        // after secsBeforeColliderOn has passed, the arrow can now collide with its own siblings again
        if ((enableCollisionTime != -1) && (Time.time > enableCollisionTime))
        {
            enableCollisionTime = -1;
            IgnoreAllArrowTrapChildren(false);
        }

        // the first time an arrow goes to sleep, it can no longer collide with the player
        // TODO - should this be using hard-coded layers?  that seems to be the default option provided by Unity
        if (gameObject.layer == originalLayer)
            if (rb.IsSleeping())
            {
                gameObject.layer = dontCollideWithPlayerLayer;
                Debug.Log(" Arrow " + this.gameObject + " switching game layer at " + Time.time);
            }

    }


    void OnCollisionEnter (Collision col)
    {
//                Debug.Log(" Arrow "+this.gameObject+" hit " + col.collider.gameObject.name + "Relative velocity was " + col.relativeVelocity + ", magnitude " + col.relativeVelocity.magnitude+" at time "+Time.time);

        if (col.relativeVelocity.magnitude > velocityThresholdForStim)
        {
            Stimulus stim = new Stimulus(StimType.Arrow, col.relativeVelocity.magnitude);

            Player player = null;
            if ((col.collider.transform.parent != null) && (col.collider.transform.parent.gameObject != null))
                player = col.collider.transform.parent.gameObject.GetComponent<Player>();  // TODO - assumes the parent of the collider contains the Player component
            if (player != null)
            {
                player.Stimulate(stim, col.contacts[0].point, col.contacts[0].normal);
            }

            StimReactor reactor = null;
            if ((col.collider.transform.parent != null) && (col.collider.transform.parent.gameObject != null))
                reactor = col.collider.transform.parent.gameObject.GetComponent<StimReactor>();  // TODO - assumes the parent of the collider contains the StimReactor component
            if (reactor != null)
            {
                reactor.Stimulate(stim);
            }
        }


    }

    // finds the nearest ancestor with the ArrowTrap component, and assumes that to be the ArrowTrap construction that this Arrow is a part of
    // then finds all the children (and later descendants) of that ArrowTrap that have a Collider and turns on/off (based on the passed-in ignore) the collion between this arrow and those colliders
    private void IgnoreAllArrowTrapChildren(bool ignore)
    {
        ArrowTrap myTrap = CodeTools.GetComponentFromNearestAncestor<ArrowTrap>(this.gameObject);

        if (myTrap == null) 
        {
            Debug.LogError(this.gameObject+" does not have an ancestor that contains the ArrowTrap component.");
            return;
        }

        Component[] colliders = myTrap.gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
            Physics.IgnoreCollision(col, mc, ignore);
    }
}


