using UnityEngine;
using System.Collections;

public class TempleWallBlock : MonoBehaviour {

    public TempleWall myWall;
    public WallBlockPosition blockPosition;
    public WallBlockType blockType;

    public enum WallBlockState : int
    {
        Emerging,
        Active,
        Expired
    }
    public WallBlockState blockState = WallBlockState.Emerging;
    private float nextStateTime;

    public GameObject emergeFXPrefab;



    protected virtual void Start()
    {
        if (myWall == null)
        {
            if (transform.parent != null)
            {
                TempleWall parentWall = transform.parent.GetComponent<TempleWall>();
                if (parentWall != null)
                    myWall = parentWall;
            }
        }

        if (myWall == null)
            Debug.LogError("TempleWallBlock "+this+"does not have a parent wall and myWall is not assigned.");

        if (blockType != WallBlockType.Block)
        {
            if (emergeFXPrefab == null)
                Debug.LogError("emergeFXPrefab not assigned.");

            // non-boring blocks start emerging as soon as they are spawned
            StartEmerging();
        }
    }


    protected virtual void Update()
    {
        UpdateState();
    }

    protected virtual void StartEmerging()
    {
        blockState = WallBlockState.Emerging;
        nextStateTime = Time.time + GameplayManager.blockEmergeDuration;

        // EmergeFX are assumed to be on&animating by default when they spawn
        GameObject fx = (GameObject)Instantiate(emergeFXPrefab) as GameObject;
        CodeTools.CopyTransform(transform, fx.transform, true, true, false);
        fx.transform.parent = transform;
        Destroy(fx, (GameplayManager.blockEmergeDuration + 2f));
    }


    private void UpdateState()
    {
        switch (blockState)
        {
            case WallBlockState.Emerging:
                if (Time.time > nextStateTime)
                {
                    blockState = WallBlockState.Active;
                }
                break;

            case WallBlockState.Active:
                break;

            case WallBlockState.Expired:
                break;

        }

    }

    protected void InitExpiredState()
    {
        blockState = WallBlockState.Expired;

    }


}
