using UnityEngine;
using System.Collections;

public class GameplayManager : MonoBehaviour {

    public EnvironmentManager environmentManager;

    public int trapCount, alcoveCount;
    public Slot[] goalSlots;
    public SnapToPosition[] goalPieces;
    public SnapToPosition readyForInsertion;
    public BoxCollider limbo; // this is where important props live when they are not in the player-accesible scene

    public bool DEBUG_angledTopArrow = false;

    public float timeBetweenBlocks = 15f;
    private float timeNextBlock;
    public float maxTimeTilGoalObject = 60f; // time in seconds before the chance of a goal object showing up = 100%
    private float timeOfLastGoalObject = 0f;

    static public float blockEmergeDuration = 6f;  // how many seconds it takes from when blocks first start emerging to when they complete the emerging process.

    public WallBlockType DEBUG_blockType1 = WallBlockType.Alcove;
    public WallBlockType DEBUG_blockType2 = WallBlockType.ArrowTrap;

    void Start () {

        if (environmentManager == null)
            Debug.LogError("environmentManager not assigned.");

        if (goalSlots.Length == 0)
            Debug.LogError("goalSlots not assigned.");

        if (goalPieces.Length == 0)
            Debug.LogError("goalPieces not assigned.");

        if (limbo == null)
            Debug.LogError("limbo not assigned.");

        for (int i=0; i<goalPieces.Length; i++)
        {
            if (!IsInLimbo(i))
                Debug.LogError(" goalPiece["+i+"], "+goalPieces[i]+", should have its starting position inside limbo.");
        }



        timeNextBlock = Time.time + timeBetweenBlocks;
	
	}
	
	void Update () {

        if (Time.time < timeNextBlock)
            return;

        timeNextBlock = Time.time + timeBetweenBlocks;

        WallBlockType nextBlockType;

        if (DEBUG_blockType1 != WallBlockType.NONE)
        {
            if (Random.Range(0, 2) == 0)
                nextBlockType = DEBUG_blockType1;
            else
                nextBlockType = DEBUG_blockType2;
        }
        else
        {
            if (Random.Range(1, (trapCount + alcoveCount + 3)) < (trapCount + 1))
                nextBlockType = WallBlockType.Alcove;
            else
                nextBlockType = WallBlockType.ArrowTrap;
        }


        bool weirdTrapThisTime = (Random.Range(0f, 1f) < 0.2f) ? true : false;

        DEBUG_angledTopArrow = weirdTrapThisTime;

        RelativeDirection wallDir = EnvironmentManager.RandomRelativeDirection();
        WallBlockPosition blockPos = EnvironmentManager.RandomWallBlockPosition();

        if ((!weirdTrapThisTime) && (nextBlockType == WallBlockType.ArrowTrap) && (TempleWall.PositionIsBottom(blockPos)) )
        {
            while (TempleWall.PositionIsBottom(blockPos))
                blockPos = EnvironmentManager.RandomWallBlockPosition();
        }

        int tries = 0;
        while ((!environmentManager.DoesItFit(nextBlockType, wallDir, blockPos)) && (tries < 100))
        {
            wallDir = EnvironmentManager.RandomRelativeDirection();
            blockPos = EnvironmentManager.RandomWallBlockPosition();
            if ((!weirdTrapThisTime) && (nextBlockType == WallBlockType.ArrowTrap) && (TempleWall.PositionIsBottom(blockPos)))
            {
                while (TempleWall.PositionIsBottom(blockPos))
                    blockPos = EnvironmentManager.RandomWallBlockPosition();
            }

            tries++;
        }

        if (tries == 100)
            return;

        // decide whether to impose any prop spawns 
        // this is super TMP chance for now
        float percentChance = ((Time.time-timeOfLastGoalObject) / (timeOfLastGoalObject + maxTimeTilGoalObject));

        if ((nextBlockType == WallBlockType.Alcove) && (Random.Range(0f, 1f) < percentChance))
        {
            if (!NothingInLimbo())
            {
                readyForInsertion = RandomAvailableGoalPiece();
                timeOfLastGoalObject = Time.time;
            }
//            else
 //               Debug.Log("Can't impose goal spawns because nothing is in limbo."); 
               
        }

        environmentManager.SpawnBlock(nextBlockType, wallDir, blockPos);

        trapCount = environmentManager.BlockCount(WallBlockType.ArrowTrap);
        alcoveCount = environmentManager.BlockCount(WallBlockType.Alcove);
    }

    // returns true iff the transform of goalPieces[ the passed-in index] is within the bounds of the Limbo boxcollider
    private bool IsInLimbo (int goalIndex)
    {
        return (limbo.bounds.Contains(goalPieces[goalIndex].transform.position));
    }

    private bool IsInLimbo (SnapToPosition piece)
    {
        return (limbo.bounds.Contains(piece.transform.position));
    }

    private bool NothingInLimbo()
    {
        bool nothingInLimbo = true;

        foreach (SnapToPosition piece in goalPieces)
            if (IsInLimbo(piece))
                nothingInLimbo = false;

        return nothingInLimbo;
    }

    private SnapToPosition RandomAvailableGoalPiece ()
    {
        if (NothingInLimbo())
        {
            Debug.LogError("All goalPieces are current unavailable.");
            return null;
        }

        int rand;

        rand = Random.Range(0, goalPieces.Length);

        while (!IsInLimbo(rand))
            rand = Random.Range(0, goalPieces.Length);

        return goalPieces[rand];
    }
}
