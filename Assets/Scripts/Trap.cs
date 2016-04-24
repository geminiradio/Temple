using UnityEngine;
using System.Collections;

public class Trap : TempleWallBlock {

    public bool oneShot = true;
    public Collider triggerVolume;
    public AudioSource triggerSFX;

    public bool DEBUG_TriggerNow = false;
    private int triggerCount = 0;

    protected override void Start ()
    {
        base.Start();

        if (triggerVolume == null)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.name == "TriggerVolume")
                {
                    Collider col = child.GetComponent<Collider>();
                    if (col != null)
                        triggerVolume = col;
                }
            }
        }

        if (triggerVolume == null)
            Debug.LogError(this + " ArrowTrap has no triggerVolume assigned and no child named TriggerVolume that has a collider component.");

    }


    // this is the base Trigger(), individual traps should override this for specific behavior when triggered
    protected virtual bool Trigger ()
    {
        if (blockState != WallBlockState.Active)
            return false;

        if (triggerSFX != null)
            triggerSFX.PlayOneShot(triggerSFX.clip);

        return true;
    }


    // this should be called on the same frame as the trigger happens
    protected virtual void PostTrigger()
    {
        triggerCount++;

        if ((oneShot) && (triggerCount > 0))
           InitExpiredState();

    }

    protected override void Update()
    {
        base.Update();

        if (DEBUG_TriggerNow)
        {
            DEBUG_TriggerNow = false;
            if (Trigger())
                PostTrigger();
        }

    }

    // by default, traps are triggered every frame the player is inside their TriggerVolume
    protected virtual void OnTriggerStay(Collider col)
    {
        if (col.gameObject.name == "Player")
        {
            // TODO - there is no way w/ OnTriggerEnter to make sure it was TriggerVolume that was the specific trigger (in case there are more than one trigger in the children of the trap object)
            if (Trigger())
                PostTrigger();
        }
    }


}
