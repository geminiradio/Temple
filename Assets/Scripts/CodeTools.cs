using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MinMaxValues
{
    public float min = 0f, max = 1f;
}

public static class CodeTools  {

    // "to" is given the transform of "from"
    public static void CopyTransform(Transform from, Transform to)
    {
        CopyTransform(from, to, true);
    }

    public static void CopyTransform(Transform from, Transform to, bool includeScale)
    {
        Vector3 newPos = from.position;
        Quaternion newRot = from.rotation;
        Vector3 newScale = from.localScale;

        to.position = newPos;
        to.rotation = newRot;
        if (includeScale)
            to.localScale = newScale;

    }

    // returns a randomly-selected index into the passed-in array of random weights.  
    // eg:  if every element of the weights array is the same number, this returns a completely random index into the array
    // eg:  if the weights array = {1, 1, 6, 1, 1} then there is a 60% chance it will return 2 and a 10% chance apiece it will return each of the other indices
    public static int WeightedRandomSelection (float[] weights)
    {

        if (!Internal_ValidateWeightedRandomArray(weights))
            return 0;

        float sum = 0f;

        for (int i = 0; i < weights.Length; i++)
            sum += weights[i];

        float rand = Random.Range(0, sum);

        int index = 0;
        float sumSoFar = weights[index];

        while ( rand >=  sumSoFar)
        {
            index++;
            sumSoFar += weights[index];
        }

        return index;

    }

    public static List<int> MultipleWeightedRandomSelections(float[] weights, int num_selections, bool allow_repeats)
    {

        List<int> toReturn = new List<int>();

        if ((num_selections > weights.Length) && (!allow_repeats))
        {
            Debug.LogError("num_selections is greater than size of array, and allow_repeats is false!");
            return toReturn;
        }

        if (num_selections == 0)
            return toReturn;

        // note that handling this edge case this way results in an ordered list, whereas we may want it shuffled according to weights?
        if ((num_selections == weights.Length) && (!allow_repeats))
        {
            for (int i = 0; i < weights.Length; i++)
                toReturn.Add(i);

            return toReturn;
        }

        int total_selections_so_far = 0;
        int timesTried = 0;

        while ( (total_selections_so_far < num_selections) && (timesTried < 99999) )
        {
            int candidate = WeightedRandomSelection(weights);
            if ( (allow_repeats) || (!toReturn.Contains(candidate)))
            {
                toReturn.Add(candidate);
                total_selections_so_far++;
            }
            timesTried++;
        }

        if (timesTried == 99999)
            Debug.Log("Warning - aborted from potential infinite loop while attempting to perform MultipleWeightedRandomSelections. Are some of the weights very very different in magnitude than others?");

        return toReturn;
    }

    public static bool ValidateWeightedRandomArray(float[] weights, int size_of_parallel_array_or_list)
    {
        if (weights.Length != size_of_parallel_array_or_list)
        {
            Debug.LogError("Invalid WeightedRandomArray: The array of weights is not the same size as the parallel list or array.");
            return false;
        }

        return Internal_ValidateWeightedRandomArray(weights);
    }


    private static bool Internal_ValidateWeightedRandomArray(float[] weights)
    {
        if (weights.Length == 0)
        {
            Debug.LogError("Invalid WeightedRandomArray:  This array is empty.");
            return false;
        }

        float sum = 0f;
        for (int i=0; i<weights.Length; i++)
        {
            if (weights[i] < 0f)
            {
                Debug.LogError("Invalid WeightedRandomArray:  Element "+i+" is a negative number: "+weights[i]);
                return false;
            }

            sum += weights[i];
        }

        if (sum == 0)
        {
            Debug.LogError("Invalid WeightedRandomArray:  This array contains only zero values.");
            return false;
        }

        return true;
    }

    public static bool ValidateMinMaxArray(MinMaxValues[] array)
    {
        return ValidateMixMaxArray_MinVsMax(array);
    }


    public static bool ValidateMinMaxArray(MinMaxValues[] array, MinMaxValues validMinRange, MinMaxValues validMaxRange)
    {
        bool valid = ValidateMixMaxArray_MinVsMax(array);

        for (int i = 0; i < array.Length; i++)
        {
            
            if (array[i].min < validMinRange.min)
            {
                Debug.LogError("Array of MinMaxValues: [" + i + "], min is invalid at " + array[i].min + ", must be at least " + validMinRange.min);
                valid = false;
            }

            if (array[i].min > validMinRange.max)
            {
                Debug.LogError("Array of MinMaxValues: [" + i + "], min is invalid at " + array[i].min + ", must be no more than " + validMinRange.max);
                valid = false;
            }

            if (array[i].max < validMaxRange.min)
            {
                Debug.LogError("Array of MinMaxValues: [" + i + "], max is invalid at " + array[i].max + ", must be at least " + validMaxRange.min);
                valid = false;
            }

            if (array[i].max > validMaxRange.max)
            {
                Debug.LogError("Array of MinMaxValues: [" + i + "], max is invalid at " + array[i].max + ", must be no more than " + validMaxRange.max);
                valid = false;
            }
        }

        return valid;
    }


    // check to make sure min is never larger than max
    private static bool ValidateMixMaxArray_MinVsMax (MinMaxValues[] array)
    {
        bool valid = true;

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].min > array[i].max)
            {
                Debug.LogError("clips_randPitchRanges[" + i + "], min of " + array[i].min + " is larger than max of " + array[i].max);
                valid = false;
            }

        }

        return valid;
    }

}
