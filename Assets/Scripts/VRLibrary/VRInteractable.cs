using UnityEngine;
using System.Collections;

// what inputs does this object respond to
// TODO this should totes be a bitfield
public enum VRInteractedBy : int {
	WandTrigger
}

// what response does this object have to being interacted with
public enum VRInteractResponse : int {
	PickUp_Physical,
    PickUp_NonPhysical,
    Openable,
    DEBUG_scale
}



public class VRInteractable : MonoBehaviour {

	public VRInteractedBy interactInput;    // what inputs do i respond to
	public VRInteractResponse interactResponse;   // how do i respond
	public WandController currentInteractor;   // what input is currently interacting w/ me   (eg - the specific wand controller)

    // pick up (and throw) behavior 
    public Transform attachPointOffset;  // how do I position and orient myself relative to the currentInteractor's attachPoint.  if null, no snapping will occur.
    public float pickUpJointBreakForce = 4000f;  // how much force will cause me to let go of this object
    private Transform originalParent = null;
    private FixedJoint pickUpJoint;

    // openable behavior
    private SpringJoint openJoint;
    public float openJointStrength = 4000f;  // spring force to keep controller attached to openable
    public float openJointBreakForce = 4000f;  // how much force will cause me to let go of this object

    // DEBUG_scaling behavior
    private Vector3 originalScale;
    private float originalInteractorY;
    private float scaleMaxDistance = 0.5f;
    private float scaleAtMaxDistance = 5f;

    void Start()
    {
        // pick up / throw behavior
        originalParent = this.transform.parent;
    }


    // called every frame that a WandController or other interactor is touching this interactable
    // the state of the controls on that interactor (so far just the trigger) are passed in
    public void InteractedWith (WandController interactor, ButtonState triggerState)
    {

        if (currentInteractor == null)
        {
            if (triggerState == ButtonState.JustPressed)
            {
                PairWithInteractor(interactor);

                switch (interactResponse)
                {
                    case VRInteractResponse.PickUp_Physical:
                    case VRInteractResponse.PickUp_NonPhysical:
                        GetPickedUp();
                        SnapToPosition snap = GetComponent<SnapToPosition>();
                        if (snap != null)
                            snap.StartCheckingForSnapTargets();
                        break;

                    case VRInteractResponse.Openable:
                        AttachOpenableToController();
                        break;

                    case VRInteractResponse.DEBUG_scale:
                        StartScaling();
                        break;
                }

            }
        }
        else  // currentInteractor is not null
        {
            if (interactor != currentInteractor)
            {
                Debug.Log("Currently paired with "+currentInteractor.gameObject+", but "+interactor.gameObject+" is selecting me too. Ignoring the latter.");
            }
            else  // this is an update from my currentInteractor - ie - the interactor i am currently paired with
            {
                if (triggerState == ButtonState.JustReleased)
                {
                    switch (interactResponse)
                    {
                        case VRInteractResponse.PickUp_Physical:
                        case VRInteractResponse.PickUp_NonPhysical:
                            LetGoOfHeldObject(true);
                            break;

                        case VRInteractResponse.Openable:
                            UnattachOpenableToController();
                            UnpairWithInteractor(interactor);
                            break;

                        case VRInteractResponse.DEBUG_scale:
                            StopScaling();
                            UnpairWithInteractor(interactor);
                            break;
                    }


                }   // currentInteractor just released me

                else if (triggerState == ButtonState.HeldDown)
                {
                    switch (interactResponse)
                    {
                        case VRInteractResponse.DEBUG_scale:
                            UpdateScale();
                            break;
                    }

                }  // still being held down by currentInteractor

            } // update from currentInteractor

        } // currentInteractor != null

    } 

    // called by external scripts, eg - when the object you were holding snaps into a slot and tells you to stop holding it
    public void DiscontinueBehavior()
    {
        switch (interactResponse)
        {
            case VRInteractResponse.PickUp_Physical:
            case VRInteractResponse.PickUp_NonPhysical:
                // this assumes it was a SnapToTarget that called this method, in which case you don't have to tell it to stop checking for snap targets, because it already called that method on itself
                GetDropped(false);
                UnpairWithInteractor(currentInteractor);
                break;
        }


    }

    private void PairWithInteractor (WandController interactor)
    {
        if (currentInteractor != null)
        {
            Debug.LogError("Cannot pair with " + interactor.gameObject + " because I already am paired with interactor " + currentInteractor.gameObject);
        }
        else
        {
            if (interactor.PairWithInteractable(this))
                currentInteractor = interactor;
            else
                Debug.LogError(interactor.gameObject+" declined to pair with me, and we don't really handle that case yet.");
        }

    }

    private void UnpairWithInteractor (WandController interactor)
    {
        if (interactor != currentInteractor)
        {
            Debug.LogError("Cannot unpair from " + interactor.gameObject + " because I am not paired with it; I'm paired with " + currentInteractor.gameObject);
        }
        else
        {
            currentInteractor = null;
            if (interactor.UnpairWithInteractable(this) == false)
                Debug.LogError(interactor.gameObject+" declined to unpair with me, and we don't really handle that case yet.");
        }
    }


    // for pick up / throw behavior - one method that encapsulates all the things you do when you release a held object
    private void LetGoOfHeldObject(bool applyThrowForce)
    {
        GetDropped(applyThrowForce);

        SnapToPosition snap = GetComponent<SnapToPosition>();
        if (snap != null)
            snap.StopCheckingForSnapTargets();

        UnpairWithInteractor(currentInteractor);
    }


    // for pick up / throw behavior
    private void GetPickedUp ()
	{
		Rigidbody myRigidbody = GetComponent<Rigidbody>();
        if (myRigidbody == null)
        {
            Debug.LogError(this + " doesn't have a rigidbody.");
            return;
        }

        if (attachPointOffset != null) 
        {
            // TODO - don't snap to this position unless there is no collision if you do snap to it
            // so introduce a "WaitingToSnapToAttachPoint state" where it tests target position until its valid
            transform.position = currentInteractor.attachPoint.position + attachPointOffset.localPosition;
            transform.rotation = currentInteractor.attachPoint.rotation * attachPointOffset.localRotation;
        }

        if (interactResponse == VRInteractResponse.PickUp_NonPhysical)
        { 
            this.transform.parent = currentInteractor.transform;
            myRigidbody.isKinematic = true;
             //            rigidbody.detectCollisions = false;
        }
        else
        {
            Rigidbody controllerRB = GetControllerRigidbody();

            if (controllerRB != null)
            {
                myRigidbody.useGravity = false;  // is this necessary?

                if (pickUpJoint != null)
                { 
                    Debug.LogError(this.gameObject+ " already had a pickUpJoint on it, so destroying that, but it probs shouldn't have had one?");
                    DestroyImmediate(pickUpJoint);
                }

                pickUpJoint = gameObject.AddComponent<FixedJoint>();
                pickUpJoint.breakForce = pickUpJointBreakForce;
                pickUpJoint.connectedBody = controllerRB;
                pickUpJoint.enablePreprocessing = false;
            }
            else
            {
                Debug.LogError(currentInteractor.gameObject + " has no rigidbody component, despite attempts to add one, so can't attach a FixedJoint to it.");
            }
        }

		
	}

    // by default when GetDropped is called, it applies a throw impulse 
    private void GetDropped ()
    {
        GetDropped(true);
    }

    // for pick up / throw behavior
    private void GetDropped (bool applyThrowImpulse)
	{

        Rigidbody myRigidbody = GetComponent<Rigidbody>();
        if (myRigidbody == null)
        {
            Debug.LogError(this + " doesn't have a rigidbody.");
            return;
        }


        if (interactResponse == VRInteractResponse.PickUp_NonPhysical)
        {
            this.transform.parent = originalParent;
        }
        else
        {
            myRigidbody.useGravity = true;

            // break the spring and remove the rb from the controller
//            Rigidbody controllerRB = joint.connectedBody;

            if (pickUpJoint != null)
                Destroy(pickUpJoint);
            else
                Debug.LogError(this.gameObject+ " didn't have a pickUpJoint on it, but it should have!");

            /*  don't remove controller's rigidbody if we had to add it
            if (controllerRB != null)
                Destroy(controllerRB);
            else
                Debug.LogError(this.gameObject + " either didn't have a joint, or its joint didn't have a connectedBody, which it should have!");
                */

        }


        myRigidbody.isKinematic = false;
        //            rigidbody.detectCollisions = true;

        if (applyThrowImpulse)
        {
            myRigidbody.velocity = currentInteractor.controller.velocity;
            myRigidbody.angularVelocity = currentInteractor.controller.angularVelocity;
            myRigidbody.maxAngularVelocity = currentInteractor.controller.angularVelocity.magnitude;
        }
        

	}


    // pick up / throw behavior  AND openable behavior
    // TODO - this doesn't detect to make sure the correct joint broke 
    void OnJointBreak(float breakForce)
    {
        if (interactResponse == VRInteractResponse.PickUp_Physical)
            LetGoOfHeldObject(false);

        else if (interactResponse == VRInteractResponse.Openable)
        {
            Debug.Log("OnJointBreak called on frame"+Time.frameCount);
            UnattachOpenableToController();
        }
    }

    // pick up / drop  AND  openable behavior
    private Rigidbody GetControllerRigidbody ()
    {
        Rigidbody controllerRB = currentInteractor.GetComponent<Rigidbody>();
        if (controllerRB == null)
        {
            Debug.Log("Controller " + currentInteractor + " didn't have a Rigidbody, so adding one. Is that really what we want?");
            controllerRB = currentInteractor.gameObject.AddComponent<Rigidbody>();
            controllerRB.isKinematic = true;
        }

        return controllerRB;
    }


    // Openable behavior
    // adds a spring between the controller and this object
    private void AttachOpenableToController ()
    {
        if (openJoint != null)
            Debug.LogError(this.gameObject + " already had an openJoint, but it should not have.");

        openJoint = gameObject.AddComponent<SpringJoint>();

        openJoint.connectedBody = GetControllerRigidbody();
        openJoint.spring = openJointStrength;
        openJoint.breakForce = openJointBreakForce;
        //  this didn't seem to do what i wanted it to.  openJoint.damper = 5f;   // we don't want it to oscilate like a spring, so lots of dampening

//        Debug.Log(this.gameObject+" just paired with "+currentInteractor.gameObject+" by attaching spring "+openJoint+" on frame "+Time.frameCount);
    }

    // Openable behavior
    // destroys spring that existed between controller and this object
    private void UnattachOpenableToController()
    {
        if (openJoint == null)
        {
            Debug.LogError(this.gameObject + " did not have an openJoint, but it should have had one.");
            return;
        }

//        Debug.Log(this.gameObject + " is unpairing with " + currentInteractor.gameObject + " by destroying spring " + openJoint + " on frame " + Time.frameCount);


        Destroy(openJoint);
        openJoint = null;
    }

    // DEBUG_scale behavior
    private void StartScaling ()
    {
        originalScale = transform.localScale;
        originalInteractorY = currentInteractor.transform.position.y;
    }

    // DEBUG_scale behavior
    private void StopScaling ()
    {

    }

    // DEBUG_scale behavior
    // note that IntreractedWith gets called every frame by every interactor in contact with this interactable
    void UpdateScale()
    {
        float finalScale;

        // scaleMult should go from 0 to 1 (although it's not clamped, so if you exceed scaleMaxDistance it can exceed 1)
        float scaleMult = ((currentInteractor.transform.position.y - originalInteractorY) / scaleMaxDistance);

        if (scaleMult >= 0)   // scaling up / raising the controller
            finalScale = 1f + (scaleMult * scaleAtMaxDistance);
        else                  // scaling down / lowering the controller
            finalScale = 1f / (((1f / scaleAtMaxDistance) - scaleMult) * scaleAtMaxDistance);

        Vector3 newScale = originalScale * finalScale;

        transform.localScale = newScale;

    }








    //// BELOW HERE = depricated throw code, which manually tracked the history of positions of the controller and based on that calculated impulse at the moment of release

    /*
    private bool currentlyTrackingPositions = false;
    private int trackingPeriod = 3;  // once every how many frames do we keep track of where the controller was located?
    private int lookBackIndex = 7;  //  how far back in the position list do we check to calculate the throw trajectory?  this number x trackingPeriod/90 = how many seconds we look back
    private float throwForceMult = 14; // this one is a constant (editable in the editor) for tuning purposes
    // index 0 is the most recent frame, index 10 would be 10 x trackingPeriod / 90 seconds ago
    private Vector3[] positionList;

        void Start()
    {
        // pick up / throw behavior
        if (lookBackIndex < 5)
            Debug.LogError("lookBackIndex must be at least 5.");
        positionList = new Vector3[20];  // with trackingPeriod of 3, this tracks 2/3 of one second
        ResetTransformList();
    }


        void FixedUpdate()
	{
        // pick up / throw behavior
		if (currentlyTrackingPositions)
		{
			// track every trackingPeriod-th frame by marking a position
			if ((Time.frameCount % trackingPeriod) == 0)
			{
                for (int i = positionList.Length-1; i >= 1; i--)
                    positionList[i] = positionList[i - 1];

				positionList[0] = new Vector3(transform.position.x, transform.position.y, transform.position.z);
			}

		}

	}


        // for pick up / throw behavior
    private void GetPickedUp ()
	{
    -- at the beginning of this method
        currentlyTrackingPositions = true;


        private void GetDropped (bool applyThrowImpulse)
	{
        currentlyTrackingPositions = false;

    --instead of setting velocity and angular velocity:
    //                myRigidbody.AddForceAtPosition(CalculateThrowImpulse(), currentInteractor.GetInteractPointPosition(), ForceMode.Impulse);

    --- at the very end:
            ResetTransformList();

    }


        // for pick up / throw behavior
    private Vector3 CalculateThrowImpulse()
    {
        if (V3IsVoid(positionList[lookBackIndex]))
        {
            // we don't have enough tracked data, so we'll use the furthest-back-in-time point we can find

            int i = lookBackIndex - 1;
            while ( (V3IsVoid(positionList[i])) && (i>0) )
                i--;

            if (i == 0)
                return Vector3.zero;

            Vector3 diff = transform.position - positionList[i];

            // the closest parallel to how we calculate the force if we have more data
            return (diff * diff.magnitude * throwForceMult);
        }
        else // we do have at least lookBackIndex number of data points
        {
            // old simple method
            //  Vector3 diff = transform.position - positionList[lookBackIndex];

            // direction vector (which includes some magnitude info) will be the average of 3 recent samples
            Vector3 diff1 = transform.position - positionList[lookBackIndex];
            Vector3 diff2 = transform.position - positionList[lookBackIndex - 2];
            Vector3 diff3 = transform.position - positionList[lookBackIndex - 4];

            Vector3 diff = (diff1 + diff2 + diff3) / 3f;
            //Debug.Log("diff vector = " + diff + ", magnitude = " + diff.magnitude);

            // force multiplier will be the largest magnitude of any vector in the positionList - ie - the furthest away on record that the object has been from the release point
            int i = 0;
            Vector3 magVector = transform.position - positionList[i];
            float mag = magVector.magnitude;
            float userForceMult = mag;
            while (((i + 1) < (positionList.Length - 1)) && (!V3IsVoid(positionList[i + 1])))
            {
                i++;
                magVector = transform.position - positionList[i];
                mag = magVector.magnitude;

                if (mag > userForceMult)
                    userForceMult = mag;
            }

            //Debug.Log("largest mag = " + userForceMult);

            Vector3 toReturn = diff * userForceMult * throwForceMult;
            //            Debug.Log("Magnitude of impulse applied: " + toReturn.magnitude);

            return toReturn;
        }


    }

    // pick up / throw behavior
    private void ResetTransformList ()
	{
		for (int i=0; i<positionList.Length; i++)
		{
			positionList[i].x=-99f;
			positionList[i].y=-99f;
			positionList[i].z=-99f;
		}
	}

    // pick up / throw behavior
    private bool V3IsVoid (Vector3 toTest)
	{
		return ( (toTest.x == -99f) && (toTest.y == -99f) && (toTest.z == -99f) );
	}



    */
}
