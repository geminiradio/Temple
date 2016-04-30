using UnityEngine;
using System.Collections;

// a slot is a thing that other things plug into (eg - pieces can be inserted into slots)
// SnapToPosition is the script that checks to see if the object is in the correct place to plug into this slot
// this script's job is to:  a) trigger the side-effects of things being slotted in, and b) work with the SnapToPosition script to check to see if it ever becomes unslotted 
public class Slot : MonoBehaviour {

    public SnapToPosition pairedSnapObject = null;


    void Update()
    {
        if (pairedSnapObject != null)
        {
            // check to see if the pairedSnapObject has moved away
            Rigidbody rb = pairedSnapObject.GetComponent<Rigidbody>();
            if (rb != null)
                if (!rb.IsSleeping())
                {
                    if (!pairedSnapObject.CheckForTarget())
                        ObjectHasUnslotted(pairedSnapObject);
                }
        }

    }

    // called when an object has snapped into this Slot
    public void ObjectHasSlotted (SnapToPosition obj)
    {
        if (obj == null)
        {
            Debug.LogError("null value passed into Slot.ObjectHasSlotted");
            return;
        }

        if (pairedSnapObject != null)
            Debug.Log(obj.gameObject+" has just requested to slot into "+this.gameObject+", but I already have a pairedSnapObject, which is "+pairedSnapObject.gameObject+", pairing with new object anyway.");

        pairedSnapObject = obj;

        TriggerSlotEffects();

    }

    // called when an object has been removed from this slot
    public void ObjectHasUnslotted (SnapToPosition obj)
    {
        if (obj == null)
        {
            Debug.LogError("null value passed into Slot.ObjectHasUnslotted");
            return;
        }

        if (pairedSnapObject != obj)
        {
            Debug.LogError(obj.gameObject+" has requested to unslot from "+this.gameObject+", but my current pairedSnapObject is something else, specifically "+pairedSnapObject.gameObject);
            return;
        }

        pairedSnapObject = null;

        TriggerUnslotEffects();
    }

    protected virtual void TriggerSlotEffects ()
    {
        Debug.Log("Triggering Slot Effects!");

        AudioSource audio;
        audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.pitch = 2f;
            audio.Play();
        }

    }

    protected virtual void TriggerUnslotEffects ()
    {
        Debug.Log("Triggering Unslot Effects!");

        AudioSource audio;
        audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.pitch = 0.5f;
            audio.Play();
        }
    }

}
