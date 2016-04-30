using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class TempleWall : MonoBehaviour {


    public TempleWallBlock topLeft, topCenter, topRight, middleLeft, middleCenter, middleRight, bottomLeft, bottomCenter, bottomRight;

    public bool editorUpdateNowTrigger = false;
    public RelativeDirection whichWall;
    public bool staggerBlocks = true;

    private float staggerMin = -0.03f;
    private float staggerMax = 0.03f;

    private bool staggerBlocksNow = false;

    void Start ()
    {
        if (Application.isPlaying)
        {
            LayoutWall();
            StaggerBlocks();

            CollectBlocks();
            CheckBlocks();
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            // we DON'T want this to happen on the same frame the wall is moved and rotated
            if (staggerBlocksNow)
            {
                StaggerBlocks();
                staggerBlocksNow = false;
            }

            if (editorUpdateNowTrigger)
            {
                editorUpdateNowTrigger = false;
                LayoutWall();
            }

        }

    }

    void LayoutWall()
    {
        // move wall to correct location
        Vector3 newPos = Vector3.zero;
        float distFromCenter = newPos.z = (EnvironmentManager.floorSize / 2) + (EnvironmentManager.blockThickness / 2); 

        switch (whichWall)
        {
            case RelativeDirection.Front:
                newPos.z = distFromCenter;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case RelativeDirection.Back:
                newPos.z = -distFromCenter;
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;

            case RelativeDirection.Right:
                newPos.x = distFromCenter;
                newPos.z = 0;
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;

            case RelativeDirection.Left:
                newPos.x = -distFromCenter;
                newPos.z = 0;
                transform.rotation = Quaternion.Euler(0, 270, 0);
                break;
        }

        transform.position = newPos;

        // can't stagger the blocks this frame or they wind up in the wrong place
        staggerBlocksNow = true;

    }

    private void StaggerBlocks()
    {
        if (staggerBlocks)
        {
            foreach (Transform child in transform)
            {
                child.transform.position = new Vector3(child.transform.position.x, child.transform.position.y, 0f);
                child.Translate(Vector3.forward * Random.Range(staggerMin, staggerMax), Space.Self);
            }
        }
    }

    private void CollectBlocks ()
    {
        foreach (Transform child in transform)
        {
            TempleWallBlock block = child.GetComponent<TempleWallBlock>();

            if (block != null)
            {
                if ((block.blockPosition == WallBlockPosition.TopLeft) && (topLeft == null))
                    topLeft = block;

                if ((block.blockPosition == WallBlockPosition.TopCenter) && (topCenter == null))
                    topCenter = block;

                if ((block.blockPosition == WallBlockPosition.TopRight) && (topRight == null))
                    topRight = block;

                if ((block.blockPosition == WallBlockPosition.MiddleLeft) && (middleLeft == null))
                    middleLeft = block;

                if ((block.blockPosition == WallBlockPosition.MiddleCenter) && (middleCenter == null))
                    middleCenter = block;

                if ((block.blockPosition == WallBlockPosition.MiddleRight) && (middleRight == null))
                    middleRight = block;

                if ((block.blockPosition == WallBlockPosition.BottomLeft) && (bottomLeft == null))
                    bottomLeft = block;

                if ((block.blockPosition == WallBlockPosition.BottomCenter) && (bottomCenter == null))
                    bottomCenter = block;

                if ((block.blockPosition == WallBlockPosition.BottomRight) && (bottomRight == null))
                    bottomRight = block;
            }


        }

    }


    private void CheckBlocks()
    {
        if (topLeft == null)
            Debug.LogError("topLeft not detected");

        if (topCenter == null)
            Debug.LogError("topCenter not detected");

        if (topRight == null)
            Debug.LogError("topRight not detected");

        if (middleLeft == null)
            Debug.LogError("middleLeft not detected");

        if (middleCenter == null)
            Debug.LogError("middleCenter not detected");

        if (middleRight == null)
            Debug.LogError("middleRight not detected");

        if (bottomLeft == null)
            Debug.LogError("bottomLeft not detected");

        if (bottomCenter == null)
            Debug.LogError("bottomCenter not detected");

        if (bottomRight == null)
            Debug.LogError("bottomRight not detected");

    }

    public TempleWallBlock GetBlockAtPosition (WallBlockPosition wallPos)
    {
        switch (wallPos)
        {
            case WallBlockPosition.TopLeft :
                return topLeft;

            case WallBlockPosition.TopCenter:
                return topCenter;

            case WallBlockPosition.TopRight:
                return topRight;

            case WallBlockPosition.MiddleLeft:
                return middleLeft;

            case WallBlockPosition.MiddleCenter:
                return middleCenter;

            case WallBlockPosition.MiddleRight:
                return middleRight;

            case WallBlockPosition.BottomLeft:
                return bottomLeft;

            case WallBlockPosition.BottomCenter:
                return bottomCenter;

            case WallBlockPosition.BottomRight:
                return bottomRight;

        }

        return null;

    }

    private void AssignBlockToPosition (TempleWallBlock newBlock, WallBlockPosition wallPos)
    {
        switch (wallPos)
        {
            case WallBlockPosition.TopLeft:
                topLeft = newBlock;
                break;

            case WallBlockPosition.TopCenter:
                topCenter = newBlock;
                break;

            case WallBlockPosition.TopRight:
                topRight = newBlock;
                break;

            case WallBlockPosition.MiddleLeft:
                middleLeft = newBlock;
                break;

            case WallBlockPosition.MiddleCenter:
                middleCenter = newBlock;
                break;

            case WallBlockPosition.MiddleRight:
                middleRight = newBlock;
                break;

            case WallBlockPosition.BottomLeft:
                bottomLeft = newBlock;
                break;

            case WallBlockPosition.BottomCenter:
                bottomCenter = newBlock;
                break;

            case WallBlockPosition.BottomRight:
                bottomRight = newBlock;
                break;

        }

        return;

    }

    public void IntegrateNewBlock (TempleWallBlock newBlock, WallBlockType blockType, WallBlockPosition wallPos)
    {
        Transform oldBlockTransform = GetBlockTransform(wallPos);

        CodeTools.CopyTransform(oldBlockTransform, newBlock.transform, false);

        DisableBlock(wallPos);

        AssignBlockToPosition(newBlock, wallPos);

    }

    private void DisableBlock (WallBlockPosition wallPos)
    {
        TempleWallBlock block = GetBlockAtPosition(wallPos);
        Destroy(block.gameObject);
    }

    public Transform GetBlockTransform (WallBlockPosition wallPos)
    {
        TempleWallBlock block = GetBlockAtPosition(wallPos);
        return block.gameObject.transform;

    }


    public bool PositionIsTop (WallBlockPosition wallPos)
    {
        return ((wallPos == WallBlockPosition.TopCenter) || (wallPos == WallBlockPosition.TopLeft) || (wallPos == WallBlockPosition.TopRight));
    }

    public bool PositionIsMiddle (WallBlockPosition wallPos)
    {
        return ((wallPos == WallBlockPosition.MiddleCenter) || (wallPos == WallBlockPosition.MiddleLeft) || (wallPos == WallBlockPosition.MiddleRight));
    }

    public bool PositionIsBottom (WallBlockPosition wallPos)
    {
        return ((wallPos == WallBlockPosition.BottomCenter) || (wallPos == WallBlockPosition.BottomLeft) || (wallPos == WallBlockPosition.BottomRight));

    }


}
