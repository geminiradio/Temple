using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MinMaxValues
{
    public float min = 0f, max = 1f;
}

public static class CodeTools  {

    public static Vector3[] vector3Directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down};

    // "to" is given the transform of "from"
    public static void CopyTransform(Transform from, Transform to)
    {
        CopyTransform(from, to, true, true, true);
    }

    public static void CopyTransform(Transform from, Transform to, bool includePosition, bool includeRotation, bool includeScale)
    {

        if (includePosition)
        {
            Vector3 newPos = from.position;
            to.position = newPos;
        }

        if (includeRotation)
        {
            Quaternion newRot = from.rotation;
            to.rotation = newRot;
        }
        if (includeScale)
        {
            Vector3 newScale = from.localScale;
            to.localScale = newScale;
        }

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


    // CURRENTLY ONLY SUPPORTS:  
    // Rigidbody on root GO,  MeshCollider on child, MeshRenderer on child
    //
    // tests if the passed-in gameObject would collide with anything if it were teleported to passed-in postion
    // it does this by turning renderers off, then changing colliders to triggers, then doing SweepTests in all 6 directions
    public static bool TestForCollision(GameObject go, Vector3 testPosition)
    {
        if (go == null)
        {
            Debug.LogError("go is null");
            return false;
        }
        if (go.transform.childCount != 1)
        {
            Debug.LogError(go+" childCount is not 1");
        }

        Rigidbody rb = go.GetComponent<Rigidbody>();
        Transform child = go.transform.GetChild(0);
        MeshCollider meshCol = child.gameObject.GetComponent<MeshCollider>();
        MeshRenderer meshRend = child.gameObject.GetComponent<MeshRenderer>();

        if (rb == null)
        {
            Debug.LogError(go+" does not have a Rigidbody component");
            return false;
        }
        if (meshCol == null)
        {
            Debug.LogError(child.gameObject+" does not have a meshCollider component");
            return false;
        }
        if (meshRend == null)
        {
            Debug.LogError(child.gameObject + " does not have a meshRenderer component");
            return false;
        }


        bool orig_kin, orig_grav, orig_trig, orig_rend;
        orig_kin = rb.isKinematic;
        orig_grav = rb.useGravity;
        orig_trig = meshCol.isTrigger;
        orig_rend = meshRend.enabled;

        rb.isKinematic = true;
        rb.useGravity = false;
        meshCol.isTrigger = true;
        meshRend.enabled= false;

        Vector3 originalPosition = go.transform.position;

        go.transform.position = testPosition;

        bool doesntFit = false;
        bool didntFitThisTime;
        RaycastHit ray = new RaycastHit();

        int i = 0;
        while ((!doesntFit) && (i<vector3Directions.Length))
        {
            didntFitThisTime = rb.SweepTest(vector3Directions[i], out ray, 0.01f, QueryTriggerInteraction.Collide);
            if (didntFitThisTime)
            {
                doesntFit = true;

                Debug.Log(GetNameOfVector3(vector3Directions[i]) +": " + go + " hit " + ray.collider + " of " + ray.collider.gameObject + " at point " + ray.point);
                GameObject newGO = new GameObject();
                newGO.transform.position = ray.point;
                newGO.name = go.name + ray.collider.gameObject.name;
            }

            i++;
        }

        rb.isKinematic = orig_kin;
        rb.useGravity = orig_grav;
        meshCol.isTrigger = orig_trig;
        go.transform.position = originalPosition;
        meshRend.enabled = orig_rend;

        return !doesntFit;
    }

    public static string GetNameOfVector3(Vector3 v3)
    {
        if (v3 == Vector3.forward)
            return "Forward";
        if (v3 == Vector3.back)
            return "Back";
        if (v3 == Vector3.left)
            return "Left";
        if (v3 == Vector3.right)
            return "Right";
        if (v3 == Vector3.up)
            return "Up";
        if (v3 == Vector3.down)
            return "Down";

        return "(" + v3.x + "," + v3.y + "," + v3.z + ")";
    }

}
