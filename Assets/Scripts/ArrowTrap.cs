using UnityEngine;
using System.Collections;

public class ArrowTrap : Trap {

    public GameObject arrowPrefab;
    public Transform arrowLocation;
    private Arrow myArrow;

    protected override void Start()
    {
        base.Start();

        SpawnArrow();

        if (triggerSFX == null)
            triggerSFX = GetComponent<RandomizeSFX>();
        if (triggerSFX == null)
            Debug.LogError("triggerSFX not assigned and no RandomizeSFX component detected.");

    }

    protected override bool Trigger ()
    {
        bool cont = base.Trigger();
        if (!cont)
            return false;

        myArrow.ShootArrow();
        return true;
    }


    private void SpawnArrow()
    {
        if (myArrow != null)
        {
            Debug.LogError(this+" ArrowTrap already has myArrow assigned but is trying to spawn a new arrow");
            return;
        }

        GameObject arrowGO = (GameObject)Instantiate(arrowPrefab, arrowLocation.position, arrowLocation.rotation) as GameObject;

        // TODO - this causes arrows to scale size, do we want that?
        arrowGO.transform.parent = transform;

        myArrow = arrowGO.GetComponent<Arrow>();

        if (myArrow == null)
            Debug.LogError(this+" arrowPrefab has no Arrow component!");
    }


}
