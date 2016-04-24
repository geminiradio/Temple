using UnityEngine;
using System.Collections;

public enum ButtonState : int
{
    JustPressed,   // just got pushed down this frame
    JustReleased,   // just got released this frame
    HeldDown,   // is being held down   
    Up     // is not being held down
}

public class WandController : MonoBehaviour {

	public bool triggerDown = false;
	public bool triggerUp = false;
	public bool triggerPressed = false;

	public VRInteractable currentSelection = null;   // what interactable is the wand controller currently touching (eg - for possible future interaction)? this variable is managed by this class     TODO this should probs be a list of all such interactables?
    public VRInteractable currentInteractable = null;  // what interactable is the wand controller currently paired with for interaction, eg - object being carried.  this is always set by the target interactable, not by this class


    // SteamVR variables I don't understand well enough yet
    private Valve.VR.EVRButtonId trigger = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input ((int)trackedObj.index);  }  }
	private SteamVR_TrackedObject trackedObj;


	void Start () {
	
		trackedObj = GetComponent<SteamVR_TrackedObject> ();
	}
	

	void Update () {

		if (controller == null) {
			Debug.Log ("Controller not initialized.");
			return;
		}

		triggerDown = controller.GetPressDown (trigger);
		triggerUp = controller.GetPressUp (trigger);
		triggerPressed = controller.GetPress (trigger);

        ButtonState triggerState;
        if (triggerDown)
            triggerState = ButtonState.JustPressed;
        else if (triggerUp)
            triggerState = ButtonState.JustReleased;
        else if (triggerPressed)
            triggerState = ButtonState.HeldDown;
        else
            triggerState = ButtonState.Up;

        // if we are paired with an interactable, send all messages there, regardless of who the current selection is
        if (currentInteractable != null)
        {
            currentInteractable.InteractedWith(this, triggerState);
        }
        // otherwise, send messages to the current selection
        else if (currentSelection != null)
        {
            currentSelection.InteractedWith(this, triggerState);
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

    // returns the VRInteractable component on the passed-in collider, one of its ancestors, or null
    private VRInteractable GetInteractableFromCollider (Collider col)
    {
        VRInteractable toReturn = col.gameObject.GetComponent<VRInteractable>();

        if (toReturn != null)
            return toReturn;

        GameObject ancestor = null;
        Transform at = col.gameObject.transform.parent;
        if (at != null)
            ancestor = at.gameObject;

        while ((toReturn == null) && (at != null))
        {
            toReturn = ancestor.GetComponent<VRInteractable>();
            at = ancestor.transform.parent;
            if (at != null)
                ancestor = at.gameObject;
        }

        return toReturn;
    }

    // tracks currentInteractable as the wand passes through qualifying objects
    void OnTriggerEnter (Collider other)
	{
        VRInteractable collidedInteractable = GetInteractableFromCollider(other);

        if (collidedInteractable != null)
		{
            currentSelection = collidedInteractable;

            if (currentInteractable != null)
                if (currentSelection != currentInteractable)
                    Debug.Log(this+" just selected "+currentSelection.gameObject+", but my currentInteractable is "+currentInteractable.gameObject+", probably you just pushed the current interactable through something else.");				
		}
		else
		{
            if (other.gameObject != null)
    			Debug.Log("WandController collided with a non-VRInteractable object, so ignoring it; it's "+ other.gameObject);
		}
			
	}


    // tracks currentInteractable as the wand passes through qualifying objects
    void OnTriggerStay (Collider other)
    {
        VRInteractable collidedInteractable = GetInteractableFromCollider(other);

        if (collidedInteractable != null)
        {
            if (currentSelection != null)
            {
                if (currentSelection != collidedInteractable)
                {
                    Debug.Log("Wand is colliding with " + collidedInteractable.gameObject + ", but the current selection is " + currentSelection.gameObject +". Probably two colliders are overlapping.");
                }
            }
            else
            {
                // this interactor is colliding with collidedInteractable, but it has no current selection
                // (i think) this case happens when >1 colliders overlap and this current collider got onenter when some other collider was the current selection, but that other collider got onexit, so now this one could become the currentSelection
                currentSelection = collidedInteractable;
            }
        }

    }


    // tracks currentInteractable as the wand passes through qualifying objects
    void OnTriggerExit (Collider other)
	{
		VRInteractable collidedInteractable = GetInteractableFromCollider(other);

        if (collidedInteractable == null)
			return;

		if (currentSelection != null)
		{
			if (currentSelection == collidedInteractable)
			{
				currentSelection = null;
			}
		}
	}


}
