using UnityEngine;
using System.Collections;

public class Alcove : TempleWallBlock {

    public GameObject[] propList;

    protected override void StartEmerging()
    {
        base.StartEmerging();

        int rand = Random.Range(0, propList.Length);

        GameObject newProp = (GameObject)Instantiate(propList[rand]) as GameObject;

        CodeTools.CopyTransform(this.transform, newProp.transform);
    }
}
