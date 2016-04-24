using UnityEngine;
using System.Collections;

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
}
