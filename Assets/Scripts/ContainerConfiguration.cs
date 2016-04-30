using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// this class can procedurally layout the contents of an alcove, chest, or similar using the data assigned in the prefab
public class ContainerConfiguration : MonoBehaviour {

    public Transform[] objectSpots;
    public float[] spots_randWeights;
    public int numSpotsToFill;

    public float[] goalSpots_randWeights;  // if a goal object is selected to be placed in here, where should it go?


    void Start ()
    {
        CodeTools.ValidateWeightedRandomArray(spots_randWeights, objectSpots.Length);
        CodeTools.ValidateWeightedRandomArray(goalSpots_randWeights, objectSpots.Length);

        if ((numSpotsToFill < 0) || (numSpotsToFill > objectSpots.Length))
            Debug.LogError("numSpotsToFill must be at least 0 and no more than the number of elements in objectSpots, but instead it is "+numSpotsToFill);

        foreach (Transform spot in objectSpots)
        {
            if (TreasureTable.FindTreasureTable(spot.gameObject) == null)
                Debug.LogError("No TreasureTable found on " + spot.gameObject + " nor any of its ancestors.");

        }

    }

    // called by external scripts to select a procedural config for this container
    public void InitializeContainerConfiguration ()
    {
        // figure out which objectSpots we're going to use
        List<int> indices = CodeTools.MultipleWeightedRandomSelections(spots_randWeights, numSpotsToFill, false);

        if (indices.Count == 0)
            return;

        GameplayManager gm = GameObject.FindObjectOfType<GameplayManager>();

        if (gm == null)
        {
            Debug.LogError("GameplayManager not found!");
            return;
        }

        int usedByGoalObject = -1;
        if (gm.readyForInsertion != null)
        {
            usedByGoalObject = GetIndexForGoalObject();
            CodeTools.CopyTransform(objectSpots[usedByGoalObject].transform, gm.readyForInsertion.transform);
            Debug.Log(gm.readyForInsertion + " teleported to newly-emerging alcove.");
            gm.readyForInsertion = null;
        }

        // spawn treasures at each of those spots
        foreach (int index in indices)
        {
            if (index != usedByGoalObject)  // if a goal object has been placed in this slot, then skip it
            {
                GameObject spot = objectSpots[index].gameObject;
                TreasureTable tt = TreasureTable.FindTreasureTable(spot);
                GameObject treasurePrefab = null;
                if (tt == null)
                    Debug.LogError("No TreasureTable found on " + spot + " nor any of its ancestors.");
                else
                    treasurePrefab = tt.GetRandomTreasure();

                GameObject newTreasure = (GameObject)Instantiate(treasurePrefab) as GameObject;
                CodeTools.CopyTransform(spot.transform, newTreasure.transform);

                // parent this new object with my parent, since my parent is presumably the container
                newTreasure.transform.parent = transform.parent;
            }
        }

    }

    // returns an index into objectSpots[] appropriate for a goalObject to be placed
    private int GetIndexForGoalObject()
    {
        return CodeTools.WeightedRandomSelection(goalSpots_randWeights);
    }




}
