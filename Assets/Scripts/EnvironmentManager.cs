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
    private static float baseFloorSize = 3f;  // assumes base floor tile is a square
    public float floorSizeX
    {
        get { return baseFloorSize*transform.lossyScale.x; }
    }
    public float floorSizeZ
    {
        get { return baseFloorSize * transform.lossyScale.z; }
    }

    public static float blockThickness = 0.5f; // this is the dimension that faces the player

    public TempleWall frontWall, backWall, leftWall, rightWall;

    // DEBUG stuff
    public RelativeDirection DEBUG_whichWall = RelativeDirection.Left;
    public WallBlockPosition DEBUG_whichPos = WallBlockPosition.TopLeft;
    public WallBlockType DEBUG_whatToSpawn = WallBlockType.Alcove;

    // parallel arrays to define several randomly-placed populations
    public int[] DEBUG_numRandomOnes;
    public WallBlockType[] DEBUG_typeRandomOnes;

    // all prefabs needed to build environment
    public GameObject alcovePrefab_bottom, alcovePrefab_middle, alcovePrefab_top;
    public GameObject arrowTrapPrefab_bottom, arrowTrapPrefab_middle, arrowTrapPrefab_top, arrowTrapPrefab_topAngled;

    public GameObject propsFolder;

    private GameplayManager gameplayManager;

    void Start () {

        CollectWalls();
        CheckWalls();

        // TODO - examine calibrated room scale and use that to dynamically scale environment
        if (transform.localScale != Vector3.one)
            ScaleEnvironment();

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

        if (propsFolder == null)
            propsFolder = GameObject.Find("Props");

        if (propsFolder == null)
        {
            propsFolder = new GameObject();
            propsFolder.name = "Props";
        }

        gameplayManager = GameObject.FindObjectOfType<GameplayManager>();
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

    public void SpawnBlock (WallBlockType blockType, TempleWall wall, WallBlockPosition wallPos)
    {
        // fetch the GO prefab using the enum
        GameObject toSpawn = WallBlockGO(blockType, wall, wallPos);

        GameObject newBlockGO = (GameObject)Instantiate(toSpawn) as GameObject;

        newBlockGO.transform.parent = wall.transform;
        TempleWallBlock newBlock = newBlockGO.GetComponent<TempleWallBlock>();
        newBlock.blockPosition = wallPos;

        // this happens automatically assuming the new block gets parented under "Environment" so, not necessary
//        if (transform.localScale != Vector3.one)
 //           newBlock.ScaleSelf(transform.localScale);

        newBlockGO.transform.localScale = Vector3.Scale(newBlockGO.transform.localScale, transform.localScale);


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

    public static RelativeDirection RandomRelativeDirection ()
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


    public static WallBlockPosition RandomWallBlockPosition()
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
                if (TempleWall.PositionIsTop(pos))
                    return alcovePrefab_top;
                else if (TempleWall.PositionIsMiddle(pos))
                    return alcovePrefab_middle;
                else if (TempleWall.PositionIsBottom(pos))
                    return alcovePrefab_bottom;
                else
                {
                    Debug.LogError("Invalid wall position, apparently.");
                    return null;
                }


            case WallBlockType.ArrowTrap:
                if (TempleWall.PositionIsTop(pos))
                {
                    if (gameplayManager.DEBUG_angledTopArrow)
                        return arrowTrapPrefab_topAngled;
                    else
                        return arrowTrapPrefab_top;
                }
                else if (TempleWall.PositionIsMiddle(pos))
                {
                    return arrowTrapPrefab_middle;
                }
                else if (TempleWall.PositionIsBottom(pos))
                {
                    return arrowTrapPrefab_bottom;
                }
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


    // make the floor footprint smaller and move/scale everything to fit
    // TODO - if floor blocks get too small, reduce the number in the grid, eg - support 2x3 or 2x2 and not just 3x3
    private void ScaleEnvironment()
    {
        if (transform.localScale.y != 1f)
            Debug.LogError("When scaling environment, there's no need to scale Y!  It's not supported.");

        if (transform.localScale.x != transform.localScale.z)
            Debug.LogError("When scaling environment, make sure x and z are scaled to the same value.");

        // do important things here if there are any?

    }

    public int BlockCount (WallBlockType blockType)
    {
        int toReturn = 0;

        toReturn += frontWall.BlockCount(blockType);
        toReturn += backWall.BlockCount(blockType);
        toReturn += leftWall.BlockCount(blockType);
        toReturn += rightWall.BlockCount(blockType);

        return toReturn;
    }


}
