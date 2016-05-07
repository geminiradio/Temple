using UnityEngine;
using System.Collections;

// this script checks to see if this object is in position to slot into the assigned Slot/Transform
// then it communicates that to other scripts
// but it doesn't track any game logic about being slotted or trigger side effects - this is basically a dumb "check for correct position and tell other scripts" script
public class SnapToPosition : MonoBehaviour {

    public bool startSlotted = false;  // if true, this object is considered plugged in at start
    public Transform targetTransform;
    public Vector3 targetPosOffset; 
    public Vector3 targetRotOffset;
    public MinMaxValues[] positionTolerance;
    public float rotationTolerance;
    public float checkPeriod = 0.03f; // how often to check in seconds  
    public float firstCheckDelay = 2f;  // how long after check is initiated before a snap can be detected.  meant to solve problem of taking the object out of the slot but it instantly snaps back in.  TODO - better solution, such as "you have to move it a certain distance away first" ?
    public Slot targetSlot;
    public bool becomeNonphysicalWhenSlotted = true;

    private bool activeNow = false;  
    private float timeofNextCheck;



    void Start () {

        if (targetTransform == null)
            Debug.LogError("targetTransform not assigned.");

        if (targetSlot == null)
            targetSlot = targetTransform.gameObject.GetComponent<Slot>();

        if (targetSlot == null)
            Debug.LogError("targetSlot not assigned, and no Slot component found on targetTransform object.");

        if (positionTolerance.Length != 3) 
            Debug.LogError("positionTolerance must be an array of size 3, where element 0 defines X tolerance, element 1 defines Y tolerance, and element 2 defines Z tolerance.");

        MinMaxValues notMoreThanZero = new MinMaxValues();
        notMoreThanZero.min = -999f;
        notMoreThanZero.max = 0f;

        MinMaxValues notLessThanZero = new MinMaxValues();
        notLessThanZero.min = 0f;
        notLessThanZero.max = 999f;

        CodeTools.ValidateMinMaxArray(positionTolerance, notMoreThanZero, notLessThanZero);

        if (rotationTolerance < 0)
            Debug.LogError("rotationTolerance must be great than 0.");

        if (checkPeriod < 0)
            Debug.LogError("checkPeriod must be greater than 0 and should be much less than 1 (try 0.03 maybe).");

        if (startSlotted)
        {
            // TODO - this code is pretty much duplicate copy/pasted from below
            SnapToTargetPosition();
            if (targetSlot != null)
                targetSlot.ObjectHasSlotted(this);
            if (becomeNonphysicalWhenSlotted)
                BecomePhysical(false);   // do this last, because by default at present VRInteractable will make the object physical again when dropped when CommunicateSnapToOtherScripts() is called

        }

    }

    void Update () {

        if ((activeNow) && (Time.time >= timeofNextCheck))
        {
            timeofNextCheck = Time.time + checkPeriod;

            if (CheckForTarget())
            {
                SnapToTargetPosition();
                CommunicateSnapToOtherScripts();
                StopCheckingForSnapTargets();

                if (becomeNonphysicalWhenSlotted)
                    BecomePhysical(false);   // do this last, because by default at present VRInteractable will make the object physical again when dropped when CommunicateSnapToOtherScripts() is called
            }
        }
	}

    // called externally by other scripts
    public void StartCheckingForSnapTargets()
    {
//        Debug.Log(this+" checking for snap target "+targetTransform.gameObject);
        activeNow = true;
        timeofNextCheck = Time.time + firstCheckDelay;

        // if this object was already slotted into something, then tell that thing to unslot
        if (targetSlot.pairedSnapObject == this)
            targetSlot.ObjectHasUnslotted(this);
    }

    public void StopCheckingForSnapTargets()
    {
//        Debug.Log(this + "done looking for snap target " + targetTransform.gameObject);
        activeNow = false;
    }

    // called by external scripts when this object has become unslotted
    public void BecomeUnslotted ()
    {
        BecomePhysical(true);
    }

    public bool CheckForTarget()
    {
        Vector3 targetPos = targetTransform.position + targetPosOffset;
        Vector3 targetRotEuler = targetTransform.rotation.eulerAngles + targetRotOffset;
        Quaternion targetRot = Quaternion.Euler(targetRotEuler);

        /*
        Debug.Log("POS diff x:" + (transform.position.x - targetPos.x) +
            ", y: " + (transform.position.y - targetPos.y) +
            ", z: " + (transform.position.z - targetPos.z) +
            ", ROT diff: " + Quaternion.Angle(transform.rotation, targetRot)
            ); 
            */
            

        // for position:  each of my 3 parameters (xyz) must be at the target values described above, +/- the max/min assigned in the tolerance array
        // for rotation:  difference between my rotation and target rotation must be within rotationTolerance

        if ((transform.position.x >= (targetPos.x + positionTolerance[0].min)) &&
            (transform.position.x <= (targetPos.x + positionTolerance[0].max)) &&
            (transform.position.y >= (targetPos.y + positionTolerance[1].min)) &&
            (transform.position.y <= (targetPos.y + positionTolerance[1].max)) &&
            (transform.position.z >= (targetPos.z + positionTolerance[2].min)) &&
            (transform.position.z <= (targetPos.z + positionTolerance[2].max)) &&
            (Quaternion.Angle(transform.rotation, targetRot) <= rotationTolerance) )
            return true;
        else
            return false;
    }

    private void SnapToTargetPosition()
    {

        // TODO potentially trigger animations or interpolators here

        transform.position = targetTransform.position + targetPosOffset;

        Vector3 targetRot = targetTransform.rotation.eulerAngles + targetRotOffset;
        transform.rotation = Quaternion.Euler(targetRot);

    }

    private void CommunicateSnapToOtherScripts ()
    {
        VRInteractable vrInteractable = GetComponent<VRInteractable>();

        if (vrInteractable != null)
            vrInteractable.DiscontinueBehavior();

        if (targetSlot != null)
            targetSlot.ObjectHasSlotted(this);

    }

    private void BecomePhysical (bool physical)
    {
        Rigidbody myRigidbody = GetComponent<Rigidbody>();

        if (myRigidbody != null)
        {
            myRigidbody.isKinematic = !physical;
            myRigidbody.useGravity = physical;
        }
    }
}
