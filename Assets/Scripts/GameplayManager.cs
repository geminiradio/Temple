using UnityEngine;
using System.Collections;

public class GameplayManager : MonoBehaviour {

    public EnvironmentManager environmentManager;

    public float timeBetweenBlocks = 15f;
    private float timeNextBlock;

    static public float blockEmergeDuration = 6f;  // how many seconds it takes from when blocks first start emerging to when they complete the emerging process.

    public WallBlockType DEBUG_blockType1 = WallBlockType.Alcove;
    public WallBlockType DEBUG_blockType2 = WallBlockType.ArrowTrap;

    void Start () {

        if (environmentManager == null)
            Debug.LogError("environmentManager not assigned.");

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

        if (tries < 100)
            environmentManager.SpawnBlock(nextBlockType, wallDir, blockPos);


    }
}
