using UnityEngine;
using System.Collections;

public enum StimReactions
{
    NONE,
    Break
}

public class StimReactor : MonoBehaviour {

    // TODO - probably should be an array of these things?  to support multiple reactions
    public StimType stim;               // what do i respond to
    public float strengthThreshold;     // how strong does it have to be to not be ignored
    public StimReactions reaction;      // what do i do?

    public RandomizedSFX breakSFX;
    public GameObject breakPFX;

    public bool Stimulate(Stimulus incomingStim)
    {
        if (incomingStim.strength < strengthThreshold)
            return false;

        switch (reaction)
        {
            case StimReactions.Break:
                BreakSelf(incomingStim);
                break;
                
        }


        return true;
    }

    // destroy self and spawn pfx and sfx objects that will stick around to do fx stuff after i'm gone
    private void BreakSelf (Stimulus breakStim)
    {
        Debug.Log(gameObject+", breaking self!");

        if (breakSFX != null)
        {
            GameObject sfx = (GameObject)Instantiate(breakSFX.gameObject, transform.position, transform.rotation) as GameObject;
            Destroy(sfx, 30f);

            RandomizedSFX rsfx = sfx.GetComponent<RandomizedSFX>();
            rsfx.PlaySFX();
        }

        if (breakPFX != null)
        {
            GameObject pfx = (GameObject)Instantiate(breakPFX, transform.position, transform.rotation) as GameObject;
            Destroy(pfx, 60f);
        }

        VRInteractable interactable = this.gameObject.GetComponent<VRInteractable>();
        if (interactable != null)
            interactable.GetDropped(false);


        Destroy(this.gameObject);
    }

}
