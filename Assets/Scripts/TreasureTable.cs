using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// a TreasureTable is an array of GameObjects that can be selected from at random (according to the weights in a parallel array)
public class TreasureTable : MonoBehaviour {

    public GameObject[] treasures;
    public float[] treasure_randWeights;

    void Start () {

        if (treasures.Length == 0)
            Debug.LogError("treasures list not assigned.");

        CodeTools.ValidateWeightedRandomArray(treasure_randWeights, treasures.Length);

	}

    public GameObject GetRandomTreasure ()
    {
        return treasures[CodeTools.WeightedRandomSelection(treasure_randWeights)];
    }

    // TODO - support non-repeating random treasures (see CodeTools) for GetRandomTreasures()
    public List<GameObject> GetRandomTreasures (int num_treasures)
    {
        List<GameObject> toReturn = new List<GameObject>();

        for (int i = 0; i < num_treasures; i++)
            toReturn.Add(GetRandomTreasure());

        return toReturn;
    }




}
