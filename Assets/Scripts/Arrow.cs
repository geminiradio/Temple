using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

    public float impulseMag = 15f;
    public float velocityThresholdForDamage = 10f;

    private Rigidbody rb;
    private MeshCollider mc;

    private float secsBeforeColliderOn = 0.05f; // enough time for the arrow to have definitely cleared its own base
    private float enableCollisionTime = -1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError(this + " Arrow must have a Rigidbody");
        else
            rb.isKinematic = true;

        mc = GetComponent<MeshCollider>();
        if (mc == null)
            Debug.LogError(this + " Arrow must have a MeshCollider");

        IgnoreAllSiblingColliders(true);

    }

    public void ShootArrow()
    {
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
            IgnoreAllSiblingColliders(false);
        }

    }


    void OnCollisionEnter (Collision col)
    {
//        Debug.Log(" Arrow collider hit " + col.collider.gameObject.name + "Relative velocity was " + col.relativeVelocity + ", magnitude " + col.relativeVelocity.magnitude);

        if (col.collider.gameObject.name == "Player")
            if (col.relativeVelocity.magnitude > velocityThresholdForDamage)
                Debug.Log("***** PLAYER INJURED BY ARROW *****");


    }


    private void IgnoreAllSiblingColliders(bool ignore)
    {
        foreach (Transform sibling in transform.parent)
            if (sibling.gameObject != this.gameObject)
            {
                Collider siblingCol = sibling.GetComponent<Collider>();   // NOTE - won't work if siblings have more than 1 collider
                if (siblingCol != null)
                    Physics.IgnoreCollision(siblingCol, mc, ignore);
            }
    }
}


