using UnityEngine;
using System.Collections;

public class ArrowTrap : Trap {

    public GameObject arrowPrefab;
    public Transform arrowLocation;  
    public MinMaxValues[] randomizedAngle;
    private Arrow myArrow;

    protected override void Start()
    {
        base.Start();

        if ((randomizedAngle.Length != 0) && (randomizedAngle.Length != 3))
            Debug.Log("randomizedAngle should either be empty or it should have exactly 3 elements corresponding to the X, Y, and Z angles to offset by that amount.");

        CodeTools.ValidateMinMaxArray(randomizedAngle, new MinMaxValues(-20f, 0f), new MinMaxValues(0f, 20f));

        SpawnArrow();

        if (randomizedAngle.Length == 3)
            RandomizeArrowAngle();

        if (triggerSFX == null)
            triggerSFX = GetComponent<RandomizedSFX>();
        if (triggerSFX == null)
            Debug.LogError("triggerSFX not assigned and no RandomizedSFX component detected.");

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

        myArrow = arrowGO.GetComponent<Arrow>();
        if (myArrow == null)
            Debug.LogError(this + " arrowPrefab has no Arrow component!");

        arrowGO.transform.parent = arrowLocation.parent;

        // slam arrow's scale back to 1,1,1 so we can rotate it without side-effects, but note this means the arrow will physically scale with the environment
        arrowGO.transform.localScale = Vector3.one;

    }


    // change the angle of the arrow and make sure the triggervolume is modified to match (it won't follow automatically since it's a rigidBody)
    // do this without any parenting, because with scale that'll cause all kinds of kooky misbehavior
    private void RandomizeArrowAngle()
    {
        // rotate myArrow by the random values expressed in randomizedAngle
        Vector3 angle = Vector3.zero;
        CodeTools.RandomizeVector3WithMinMaxArray(ref angle, randomizedAngle);
        myArrow.transform.Rotate(angle, Space.World);
        triggerVolume.transform.Rotate(angle, Space.World);

//        Debug.Log("Arrowtrap rotation angle = "+angle);
    }

}
