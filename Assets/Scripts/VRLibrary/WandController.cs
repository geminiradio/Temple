using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ButtonState : int
{
    JustPressed,   // just got pushed down this frame
    JustReleased,   // just got released this frame
    HeldDown,   // is being held down   
    Up     // is not being held down
}

public class WandController : MonoBehaviour {

    public enum DEBUG_action : int
    {
        NONE,
        DEBUG_PausePlayer
    }

    public DEBUG_action DEBUG_Action = DEBUG_action.NONE;

	public bool triggerDown = false;
	public bool triggerUp = false;
	public bool triggerPressed = false;
    public bool menuDown = false;
    public bool menuUp = false;
    public bool menuPressed = false;

    public List<VRInteractable> currentSelection;   // what interactables is the wand controller currently touching (eg - for possible future interaction)? this variable is managed by this class   
    public VRInteractable currentInteractable = null;  // what interactable is the wand controller currently paired with for interaction, eg - object being carried.  this is always set by the target interactable, not by this class

    public SphereCollider interactPoint;  // this is the trigger that this controller uses to detect collisions with other objects
    public Transform attachPoint;  // this is the default transform that held objects snap to when held (but they have an offset to this transform and if null they don't snap at all)


    // SteamVR variables I don't understand well enough yet
    public SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;
    private Valve.VR.EVRButtonId trigger = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private Valve.VR.EVRButtonId menuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;

	void Start () {

		trackedObj = GetComponent<SteamVR_TrackedObject> ();

        if (interactPoint == null)
            interactPoint = GetComponent<SphereCollider>();

        if (interactPoint == null)
        {
            Debug.Log("Adding default interactPoint (SphereCollider trigger) to "+gameObject);
            interactPoint = gameObject.AddComponent<SphereCollider>();

            // default values for wand controller interactPoint / SphereCollider
            interactPoint.isTrigger = true;
            interactPoint.center = new Vector3(0, -0.06f, 0.03f);
            interactPoint.radius = 0.01f;
        }

        if (attachPoint == null)
        {
            Debug.Log("Assigning default attachPoint (this Transform) for "+gameObject);
            attachPoint = transform;
        }

        currentSelection = new List<VRInteractable>();

    }
	

	void Update () {

		if (controller == null) {
			Debug.Log ("Controller not initialized.");
			return;
		}

		triggerDown = controller.GetPressDown (trigger);
		triggerUp = controller.GetPressUp (trigger);
		triggerPressed = controller.GetPress (trigger);
        menuDown = controller.GetPressDown(menuButton);
        menuUp = controller.GetPressUp(menuButton);
        menuPressed = controller.GetPress(menuButton);

        ButtonState triggerState;
        if (triggerDown)
            triggerState = ButtonState.JustPressed;
        else if (triggerUp)
            triggerState = ButtonState.JustReleased;
        else if (triggerPressed)
            triggerState = ButtonState.HeldDown;
        else
            triggerState = ButtonState.Up;

        // if we are paired with an interactable, first send Update messages there, regardless of who the current selection is
        if (currentInteractable != null)
        {
            currentInteractable.InteractedWith(this, triggerState);
        }

        // next send messages to the current set of selections
        // we iterate in reverse starting at the end so we can legally remove items from the list as we go
        for (int i = currentSelection.Count-1; i >= 0; i--)
        {
            if (currentSelection[i] == null)
                currentSelection.RemoveAt(i);
            else if (currentSelection[i].gameObject == null) 
                currentSelection.RemoveAt(i);
            else if (!currentSelection[i].gameObject.activeInHierarchy)
                currentSelection.RemoveAt(i);
            else
                currentSelection[i].InteractedWith(this, triggerState);

        }



        if ((menuDown) && (DEBUG_Action != DEBUG_action.NONE))
        {
            if (DEBUG_Action == DEBUG_action.DEBUG_PausePlayer)
            {
                Debug.Break();
            }

        }


    }

    // this is called by an interactable when it decides to pair with this controller
    public bool PairWithInteractable (VRInteractable interactable)
    {
        if (currentInteractable != null)
        {
            Debug.LogError("Can't pair with "+interactable.gameObject+" because I'm already paired with "+currentInteractable);
            return false;
        }

        currentInteractable = interactable;
        return true;
    }

    // this is called by an interactable when it decides to unpair with this controller
    public bool UnpairWithInteractable(VRInteractable interactable)
    {
        if (currentInteractable != interactable)
        {
            Debug.LogError("Can't unpair from "+interactable.gameObject+" because I'm not paired with it, I'm paired with "+currentInteractable.gameObject);
            return false;
        }

        currentInteractable = null;
        return true;
    }



    // tracks currentInteractable as the wand passes through qualifying objects
    void OnTriggerEnter (Collider other)
	{
        VRInteractable collidedInteractable = CodeTools.GetComponentFromNearestAncestor<VRInteractable>(other.gameObject);

        if (collidedInteractable != null)
		{
            // if we don't already have this interactable on our list of ones we're selecting, then add it
            if (!currentSelection.Contains(collidedInteractable))
                currentSelection.Add(collidedInteractable);

            if (currentInteractable != null)
                if (collidedInteractable != currentInteractable)
                    Debug.Log(this+" just selected "+ collidedInteractable.gameObject+", but my currentInteractable is "+currentInteractable.gameObject+", probably you just pushed the current interactable through something else.");				
		}
		else
		{
//            if (other.gameObject != null)
//    			Debug.Log("WandController collided with a non-VRInteractable object, so ignoring it; it's "+ other.gameObject);
		}
			
	}


    // tracks currentInteractable as the wand passes through qualifying objects
    void OnTriggerStay (Collider other)
    {
        VRInteractable collidedInteractable = CodeTools.GetComponentFromNearestAncestor<VRInteractable>(other.gameObject);

        if (collidedInteractable != null)
        {
            if (!currentSelection.Contains(collidedInteractable))
            {
                Debug.LogError(this.gameObject + " wand is OnTriggerStay() colliding with " + collidedInteractable.gameObject + " but it's not on the currentSelection list, but it should be!");
            }
        }

    }


    // tracks currentInteractable as the wand passes through qualifying objects
    void OnTriggerExit (Collider other)
	{
		VRInteractable collidedInteractable = CodeTools.GetComponentFromNearestAncestor<VRInteractable>(other.gameObject);

        if (collidedInteractable == null)
			return;

		if (currentSelection.Contains(collidedInteractable))
		{
            currentSelection.Remove(collidedInteractable);
		}
        else
        {
            Debug.LogError(this.gameObject + " wand called OnTriggerExit() with " + collidedInteractable.gameObject + " but it's not on the currentSelection list, but it should be!");
        }
    }


    public Vector3 GetInteractPointPosition ()
    {
        return transform.position + interactPoint.center;
    }

}
