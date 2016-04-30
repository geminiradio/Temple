using UnityEngine;
using System.Collections;

public enum RelativeDirection : int
{
    NONE,
    Front,
    Back,
    Left,
    Right
}

public enum WallBlockPosition : int
{
    NONE,
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public enum WallBlockType : int { 
    NONE,
    Block,
    Alcove,
    ArrowTrap,
    PitTrap,
    DeadfallTrap,
    BeamTrap,
    PressurePlate
}

public class EnvironmentManager : MonoBehaviour {

    // global game variables
    public static float floorSize = 3f;  // assumes the floor is a square
    public static float blockThickness = 0.5f; // this is the dimension that faces the player

    public TempleWall frontWall, backWall, leftWall, rightWall;

    public RelativeDirection DEBUG_whichWall = RelativeDirection.Left;
    public WallBlockPosition DEBUG_whichPos = WallBlockPosition.TopLeft;
    public WallBlockType DEBUG_whatToSpawn = WallBlockType.Alcove;

    // parallel arrays to define several randomly-placed populations
    public int[] DEBUG_numRandomOnes;
    public WallBlockType[] DEBUG_typeRandomOnes;

    // all prefabs needed to build environment
    public GameObject alcovePrefab_short, alcovePrefab_tall;
    public GameObject arrowTrapPrefab_bottom, arrowTrapPrefab_middle, arrowTrapPrefab_top;

    void Start () {

        CollectWalls();
        CheckWalls();

        // just some debug playtesty spawning for fun

        // this one you get to choose where to put it
        if (DEBUG_whatToSpawn != WallBlockType.NONE)
            SpawnBlock (DEBUG_whatToSpawn, GetWallAtDirection(DEBUG_whichWall), DEBUG_whichPos);

        // go through parallel arrays and place those too
        for (int i = 0; i < DEBUG_numRandomOnes.Length; i++)
        {
            for (int j = 0; j < DEBUG_numRandomOnes[i]; j++)
            {
                SpawnBlock(DEBUG_typeRandomOnes[i], GetWallAtDirection(RandomRelativeDirection()), RandomWallBlockPosition());
            }
        }

	}
	


    private void CollectWalls ()
    {
        foreach (Transform child in transform)
        {
            TempleWall wall = child.GetComponent<TempleWall>();

            if (wall != null)
            {
                if ((wall.whichWall == RelativeDirection.Front) && (frontWall == null))
                    frontWall = wall;

                if ((wall.whichWall == RelativeDirection.Back) && (backWall == null))
                    backWall = wall;

                if ((wall.whichWall == RelativeDirection.Left) && (leftWall == null))
                    leftWall = wall;

                if ((wall.whichWall == RelativeDirection.Right) && (rightWall == null))
                    rightWall = wall;
            }


        }

    }

    private void CheckWalls ()
    {
        if (frontWall == null)
            Debug.LogError("frontWall not detected");

        if (backWall == null)
            Debug.LogError("backWall not detected");

        if (leftWall == null)
            Debug.LogError("leftWall not detected");

        if (rightWall == null)
            Debug.LogError("rightWall not detected");

    }



    public void SpawnBlock (WallBlockType blockType, RelativeDirection wallDir, WallBlockPosition wallPos)
    {
        SpawnBlock(blockType, GetWallAtDirection(wallDir), wallPos);
    }

    // if goalPiece is non-null, the newly spawned block should attempt to include it 
    public void SpawnBlock (WallBlockType blockType, TempleWall wall, WallBlockPosition wallPos)
    {
        // fetch the GO prefab using the enum
        GameObject toSpawn = WallBlockGO(blockType, wall, wallPos);

        GameObject newBlockGO = (GameObject)Instantiate(toSpawn) as GameObject;

        newBlockGO.transform.parent = wall.transform;
        TempleWallBlock newBlock = newBlockGO.GetComponent<TempleWallBlock>();
        newBlock.blockPosition = wallPos;

        wall.IntegrateNewBlock(newBlock, blockType, wallPos);
    }



    private TempleWall GetWallAtDirection (RelativeDirection dir)
    {
        switch (dir)
        {
            case RelativeDirection.Front:
                return frontWall;

            case RelativeDirection.Back:
                return backWall;

            case RelativeDirection.Left:
                return leftWall;

            case RelativeDirection.Right:
                return rightWall;

        }

        return null;

    }

    public RelativeDirection RandomRelativeDirection ()
    {

        int r = Random.Range(1, 5);

        switch (r)
        {
            case 1:
                return RelativeDirection.Front;

            case 2:
                return RelativeDirection.Back;

            case 3:
                return RelativeDirection.Left;

            case 4:
                return RelativeDirection.Right;
        }

        Debug.LogError("Deeply meaningful warning: Apparently I don't know how to use Random.Range!!");
        return RelativeDirection.Front;

    }


    public WallBlockPosition RandomWallBlockPosition()
    {

        int r = Random.Range(1, 10);

        switch (r)
        {
            case 1:
                return WallBlockPosition.TopLeft;

            case 2:
                return WallBlockPosition.TopCenter;

            case 3:
                return WallBlockPosition.TopRight;

            case 4:
                return WallBlockPosition.MiddleLeft;

            case 5:
                return WallBlockPosition.MiddleCenter;

            case 6:
                return WallBlockPosition.MiddleRight;

            case 7:
                return WallBlockPosition.BottomLeft;

            case 8:
                return WallBlockPosition.BottomCenter;

            case 9:
                return WallBlockPosition.BottomRight;

        }

        Debug.LogError("Deeply meaningful warning: Apparently I don't know how to use Random.Range!!");
        return WallBlockPosition.TopLeft;
    }


    private GameObject WallBlockGO(WallBlockType b, TempleWall wall, WallBlockPosition pos)
    {
        switch (b)
        {
            case WallBlockType.Alcove:
                if (wall.PositionIsMiddle(pos))
                    return alcovePrefab_tall;
                else
                    return alcovePrefab_short;

            case WallBlockType.ArrowTrap:
                if (wall.PositionIsTop(pos))
                    return arrowTrapPrefab_top;
                else if (wall.PositionIsMiddle(pos))
                    return arrowTrapPrefab_middle;
                else if (wall.PositionIsBottom(pos))
                    return arrowTrapPrefab_bottom;
                else
                {
                    Debug.LogError("Invalid wall position, apparently.");
                    return null;
                }

        }

        return null;
    }


    public bool DoesItFit (WallBlockType blockType, RelativeDirection wallDir, WallBlockPosition blockPos)
    {
        return DoesItFit(blockType, GetWallAtDirection(wallDir), blockPos);
    }


    public bool DoesItFit(WallBlockType blockType, TempleWall wall, WallBlockPosition blockPos)
    {
        if (wall.GetBlockAtPosition(blockPos).blockType == WallBlockType.Block)
            return true;
        else
            return false;

    }

}
