using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
public class MazeSpawner : MonoBehaviour
{
    public enum MazeGenerationAlgorithm
    {
        PureRecursive
    }
    public MazeGenerationAlgorithm Algorithm = MazeGenerationAlgorithm.PureRecursive;
    public bool FullRandom = false;
    public int RandomSeed = 12345;
    public GameObject Floor = null;
    public GameObject Wall = null;
    public GameObject Pillar = null;
    public int Rows = 5;
    public int Columns = 5;
    public float CellWidth = 5;
    public float CellHeight = 5;
    public bool AddGaps = false;
    public GameObject GoalPrefab = null;
    private BasicMazeGenerator mMazeGenerator = null;

    public GameObject coffe;

    public static T SafeDestroy<T>(T obj) where T : Object
    {
        if (!obj) return null;

        if (Application.isEditor && !Application.isPlaying)
        {
            Object.DestroyImmediate(obj);
        }  
        else
            Object.Destroy(obj);

        return null;
    }

    public GameObject GenerateCoffe()
    {
        foreach (CoffeScript go in FindObjectsOfType<CoffeScript>() as CoffeScript[])
        {
            SafeDestroy(go.gameObject);
        }

        Vector3 newPos = GetMazePosition();
        newPos.y += .11f;

        return Instantiate(coffe, newPos, coffe.transform.rotation);
    }

    public Vector3 GetMazePosition()
    {
        List<Transform> floorList = new List<Transform>();
        for (var i = gameObject.transform.childCount - 1; i >= 0; i--)
        {
            if (gameObject.transform.GetChild(i).CompareTag("Floor"))
            {
                floorList.Add(gameObject.transform.GetChild(i).transform);
            }
        }
        return floorList[Random.Range(0, floorList.Count)].position;
    }

#if UNITY_EDITOR
    [ButtonMethod]
#endif
    private void CreateMaze()
    {
        RandomSeed = Random.Range(0, 99999);
        for (var i = gameObject.transform.childCount - 1; i >= 0; i--)
        {
            SafeDestroy(gameObject.transform.GetChild(i).gameObject);
        }
        if (!FullRandom)
        {
            Random.InitState(RandomSeed);
        }
        switch (Algorithm)
        {
            case MazeGenerationAlgorithm.PureRecursive:
                mMazeGenerator = new RecursiveMazeAlgorithm(Rows, Columns);
                break;
        }
        mMazeGenerator.GenerateMaze();
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                float x = column * (CellWidth + (AddGaps ? .2f : 0));
                float z = row * (CellHeight + (AddGaps ? .2f : 0));
                MazeCell cell = mMazeGenerator.GetMazeCell(row, column);
                GameObject tmp;
                tmp = Instantiate(Floor, new Vector3(x, 0, z), Quaternion.Euler(0, 0, 0)) as GameObject;
                tmp.transform.parent = transform;
                if (cell.WallRight)
                {
                    tmp = Instantiate(Wall, new Vector3(x + CellWidth / 2, 0, z) + Wall.transform.position, Quaternion.Euler(0, 90, 0)) as GameObject;// right
                    tmp.transform.parent = transform;
                }
                if (cell.WallFront)
                {
                    tmp = Instantiate(Wall, new Vector3(x, 0, z + CellHeight / 2) + Wall.transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;// front
                    tmp.transform.parent = transform;
                }
                if (cell.WallLeft)
                {
                    tmp = Instantiate(Wall, new Vector3(x - CellWidth / 2, 0, z) + Wall.transform.position, Quaternion.Euler(0, 270, 0)) as GameObject;// left
                    tmp.transform.parent = transform;
                }
                if (cell.WallBack)
                {
                    tmp = Instantiate(Wall, new Vector3(x, 0, z - CellHeight / 2) + Wall.transform.position, Quaternion.Euler(0, 180, 0)) as GameObject;// back
                    tmp.transform.parent = transform;
                }
                if (cell.IsGoal && GoalPrefab != null)
                {
                    tmp = Instantiate(GoalPrefab, new Vector3(x, 1, z), Quaternion.Euler(0, 0, 0)) as GameObject;
                    tmp.transform.parent = transform;
                }
            }
        }
        if (Pillar != null)
        {
            for (int row = 0; row < Rows + 1; row++)
            {
                for (int column = 0; column < Columns + 1; column++)
                {
                    float x = column * (CellWidth + (AddGaps ? .2f : 0));
                    float z = row * (CellHeight + (AddGaps ? .2f : 0));
                    GameObject tmp = Instantiate(Pillar, new Vector3(x - CellWidth / 2, 0, z - CellHeight / 2) + Pillar.transform.position, Pillar.transform.rotation) as GameObject;
                    tmp.transform.parent = transform;
                }
            }
        }

        GenerateCoffe();
    }
}
