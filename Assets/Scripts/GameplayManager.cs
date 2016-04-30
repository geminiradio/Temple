using UnityEngine;
using System.Collections;

public class GameplayManager : MonoBehaviour {

    public EnvironmentManager environmentManager;

    public Slot[] goalSlots;
    public SnapToPosition[] goalPieces;
    public SnapToPosition readyForInsertion;
    public BoxCollider limbo; // this is where important props live when they are not in the player-accesible scene

    public float timeBetweenBlocks = 15f;
    private float timeNextBlock;

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
                Debug.LogError(" goalPiece["+i+"], "+goalPieces[i]+" should have its starting position inside limbo.");
        }



        timeNextBlock = Time.time + timeBetweenBlocks;
	
	}
	
	void Update () {

        if (Time.time < timeNextBlock)
            return;

        timeNextBlock = Time.time + timeBetweenBlocks;

        WallBlockType nextBlockType;

        if (Random.Range(0, 2) == 0)
            nextBlockType = DEBUG_blockType1;
        else
            nextBlockType = DEBUG_blockType2;


        RelativeDirection wallDir = environmentManager.RandomRelativeDirection();
        WallBlockPosition blockPos = environmentManager.RandomWallBlockPosition();

        int tries = 0;
        while ((!environmentManager.DoesItFit(nextBlockType, wallDir, blockPos)) && (tries < 100))
        {
            wallDir = environmentManager.RandomRelativeDirection();
            blockPos = environmentManager.RandomWallBlockPosition();
            tries++;
        }

        if (tries == 100)
            return;

        // decide whether to impose any prop spawns 
        if ((nextBlockType == WallBlockType.Alcove) && (Random.Range(0f, 1f) < 0.5f))
        {
            if (!NothingInLimbo())
                readyForInsertion = RandomAvailableGoalPiece();
            else
                Debug.Log("Can't impose goal spawns because nothing is in limbo."); 
               
        }

        environmentManager.SpawnBlock(nextBlockType, wallDir, blockPos);


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
