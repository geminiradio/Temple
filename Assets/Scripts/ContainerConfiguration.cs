using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// this class can procedurally layout the contents of an alcove, chest, or similar using the data assigned in the prefab
public class ContainerConfiguration : MonoBehaviour {

    public Transform[] objectSpots;
    public float[] spots_randWeights;
    public int numSpotsToFill;

    public float[] goalSpots_randWeights;  // if a goal object is selected to be placed in here, where should it go?

    private EnvironmentManager environment;
    private GameplayManager gameplayManager;

    void Awake ()
    {
        CodeTools.ValidateWeightedRandomArray(spots_randWeights, objectSpots.Length);
        CodeTools.ValidateWeightedRandomArray(goalSpots_randWeights, objectSpots.Length);

        if ((numSpotsToFill < 0) || (numSpotsToFill > objectSpots.Length))
            Debug.LogError("numSpotsToFill must be at least 0 and no more than the number of elements in objectSpots, but instead it is "+numSpotsToFill);

        foreach (Transform spot in objectSpots)
        {
            if (CodeTools.GetComponentFromNearestAncestor<TreasureTable>(spot.gameObject) == null)
                Debug.LogError("No TreasureTable found on " + spot.gameObject + " nor any of its ancestors.");

        }

        environment = GameObject.FindObjectOfType<EnvironmentManager>();
        if (environment == null)
            Debug.LogError("EnvironmentManager not found.");

        gameplayManager = GameObject.FindObjectOfType<GameplayManager>();
        if (gameplayManager == null)
            Debug.LogError("GameplayManager not found!");

        GameObject collisionMarkers;
        collisionMarkers = GameObject.Find("CollisionMarkers");
        if (collisionMarkers == null)
            collisionMarkers = new GameObject("CollisionMarkers");

        if (environment.propsFolder != null)
            collisionMarkers.transform.parent = environment.propsFolder.transform;

    }

    // called by external scripts to select a procedural config for this container
    // TODO - this whole thing is garbage and needs to be rewritten, maybe when i know more about what GameplayManager wants to do
    public void InitializeContainerConfiguration ()
    {
        // figure out which objectSpots we're going to use
        List<int> indices = CodeTools.MultipleWeightedRandomSelections(spots_randWeights, numSpotsToFill, false);

        if (indices.Count == 0)
            return;

        int usedByGoalObject = -1;
        if (gameplayManager.readyForInsertion != null)
        {
            usedByGoalObject = GetIndexForGoalObject();
            CodeTools.CopyTransform(objectSpots[usedByGoalObject].transform, gameplayManager.readyForInsertion.transform, true, true, false);
//            Debug.Log(gameplayManager.readyForInsertion + " teleported to newly-emerging alcove.");
            gameplayManager.readyForInsertion = null;
        }

        // spawn treasures at each of those spots
        foreach (int index in indices)
        {
            if (index != usedByGoalObject)  // if a goal object has been placed in this slot, then skip it
            {
                GameObject spot = objectSpots[index].gameObject;
                TreasureTable tt = CodeTools.GetComponentFromNearestAncestor<TreasureTable>(spot);
                if (tt == null)
                    Debug.LogError("No TreasureTable found on " + spot + " nor any of its ancestors.");

                // pick a treasure that fits in the chosen spot
                // TODO - this logic should go more like:  there's a method that finds an unused spot this treasure will fit and puts it there.  iterating over treasures, not spots.

                GameObject treasurePrefab = null;
                bool itFitsHere = false;
                int tries = 0;
                GameObject newTreasure = null;

                while ((!itFitsHere) && (tries < 20))  // TODO - this is an unacceptable amount of sweeptests for performance
                {
                    tries++;

                    treasurePrefab = tt.GetRandomTreasure();
                    newTreasure = (GameObject)Instantiate(treasurePrefab) as GameObject;
                    CodeTools.CopyTransform(spot.transform, newTreasure.transform, false, true, false);  // rotate the object to match the spot

                    itFitsHere = CodeTools.TestForCollision(newTreasure, spot.transform.position);

                    if (itFitsHere)
                    {
                        Debug.Log(newTreasure + " fits without collision at " + spot.transform.position);
                        if (spot.transform.position == Vector3.zero)
                            Debug.Log("Why is it zero?");
                    }
                    else
                    {
                        Debug.Log(newTreasure + " does not fit at " + spot.transform.position);
                        DestroyImmediate(newTreasure);  // TODO - needless to say this is terrible
                    }
                }

                if (itFitsHere)
                {
                    // now you can teleport it there
                    CodeTools.CopyTransform(spot.transform, newTreasure.transform, true, true, false);
                    newTreasure.transform.parent = (environment.propsFolder != null) ? environment.propsFolder.transform : null;
                }
                else
                {
                    Debug.LogError("Couldn't find any treasures that will fit in spot "+spot.gameObject+" which is at "+spot.transform.position);
                }

            }
        }

    }

    // returns an index into objectSpots[] appropriate for a goalObject to be placed
    private int GetIndexForGoalObject()
    {
        return CodeTools.WeightedRandomSelection(goalSpots_randWeights);
    }




}
