using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GridWorldManager : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
    public class Tile
    {
        public GameObject Obj;
        public Vector2 Position = new Vector2();
        public Direction Direction;
        public float vs = 0;
    }

    public GameObject tilePrefab;
    public GameObject backgroundTile;
    public int rows = 3;
    public int column = 3;
    public float gamma = 0.9f;
    //public Vector2 wallTest = new Vector2(3, 3);
    //public Vector2 holeTest = new Vector2(4, 3);
    public List<Vector2>  wallList;
    private List<Vector2>  holeList;

    public List<GameObject> prefabList;
    public List<Sprite> SpriteList;
    
    
    private Tile[,] _grid;
    private Direction[,] tabDirections;
        
    // Start is called before the first frame update
    void Start()
    {
        
        _grid = new Tile[rows,column];
        tabDirections = new Direction[rows, column];
        wallList = new List<Vector2>();
        holeList = new List<Vector2>();
       //wallList.Add(wallTest);
        //holeList.Add(holeTest);
        //PolicyIteration();
    }

    
    public void SetUpGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                //initialisation de la grid qui contiendra les infos de chaque state a telle case
                _grid[i, j] = new Tile();
                _grid[i, j].vs = 0f;
                //Debug.Log(_grid[i, j].Position);
                _grid[i, j].Position = new Vector2(j, i);
                //Instantiate(actualGrid.Obj); 
                _grid[i, j].Direction = (Direction) Random.Range(0, 4);
                tabDirections[i, j] = _grid[i, j].Direction;
                Instantiate(backgroundTile, new Vector3(j,i,0.1f),Quaternion.identity);
                _grid[i, j].Obj = Instantiate(tilePrefab);
                _grid[i, j].Obj.GetComponent<SpriteRenderer>().sprite =
                    SpriteList[(int) _grid[i, j].Direction];
                
                _grid[i, j].Obj.transform.position = _grid[i, j].Position;
                //Debug.Log(_grid[i, j].Direction);
            }
        }

        
        
        //Debug.Log(_grid[0, 0]) ;
        //Debug.Log(rows);
        //Debug.Log(column);
        //Debug.Log(_grid[rows -2, column - 2].vs);
        _grid[rows -1, column - 1].vs = 1f;//on definit l'arrivée comme etant la derniere case

        //_grid[(int)wallTest.y,(int)wallTest.x].vs = -10;

        foreach (var hole in holeList)
        {
            _grid[(int) hole.y, (int) hole.x].vs = -10;
        }

    }

    public void ValueIteration()
    {
        //SetUpGrid();

        float delta = 1f;
        float teta = 0.02f;
        
        while (delta > teta)
        {
            delta = 0f;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (i == rows - 1 && j == column - 1)
                    {
                        break;
                    }
                    //Debug.Log(i + " " + j);
                    float temp = _grid[i, j].vs;
                    
                    _grid[i,j].vs = gamma * ArgMaxVs( i, j);
                    delta = Mathf.Max(delta, Mathf.Abs(temp - _grid[i, j].vs));
                    
                }
            
            }
        }

        DebugResultsPolicicy();
        ApplyQs();

    }

    

    void PolicyIteration()
    {
        SetUpGrid();
        PolicyEvaluation();
        
    }

    public void PolicyEvaluation()
    {
        float delta = 1f;
        float teta = 0.02f;
        
        while (delta > teta)
        {
            delta = 0f;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (i == rows - 1 && j == column - 1)
                    {
                        break;
                    }
                    //Debug.Log(i + " " + j);
                    float temp = _grid[i, j].vs;
                    if (i == 3 && j == 4)
                    {
                        Debug.Log("vs");
                    }

                    foreach (var wall in wallList)
                    {
                        if (wall.x == j && wall.y == i)
                        {
                            break;
                        }
                        else
                        {
                            _grid[i,j].vs = CheckDirection(_grid[i, j], i, j);
                            delta = Mathf.Max(delta, Mathf.Abs(temp - _grid[i, j].vs));
                        }
                    }
                    
                    
                }
            
            }
        }
        
        DebugResultsPolicicy();
        
        PolicyImprovment();
    }

    void DebugResultsPolicicy()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                Debug.Log(i + " " + j + " vs = " + _grid[i,j].vs);
                // = CheckDirection(_grid[i, j], i, j);
                _grid[i, j].Obj.transform.GetChild(0).GetComponent<TMP_Text>().text = (Mathf.Round(_grid[i, j].vs*100f)/100f).ToString();
            }
            
        }
    }

    void PolicyImprovment()//pour actualiser la policy
    {
        bool isPolicyStable = true;//
        Direction temp;
        
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (i == rows - 1 && j == column - 1)
                {
                    break;
                }
                Debug.Log(i + " " + j);
                temp = _grid[i, j].Direction;
                Debug.Log("direction avant = " + _grid[i,j].Direction );
                _grid[i,j].Direction = ArgMax( i, j);

                //listFinal = GetListDirections();
                if (temp != _grid[i,j].Direction)
                {
                    isPolicyStable = false;
                    ChangeArrow(i,j);//fonction qui va actualiser
                }
                
                
                
                Debug.Log(i + " " + j + " vs = " + _grid[i,j].Direction);
            }
            
        }
        

        if (isPolicyStable)
        {
            return;
        }
        else
        {
            Debug.Log("newIteration");
            PolicyEvaluation();
        }
        
    }
    void ApplyQs()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (i == rows - 1 && j == column - 1)
                {
                    break;
                }

                _grid[i, j].Direction = ArgMax(i, j);
                ChangeArrow(i,j);
            }
            
        }
    }

    void ChangeArrow(int rowNb, int columnNb)
    {
        //DestroyImmediate(_grid[rowNb,columnNb].Obj);
        //_grid[rowNb,columnNb].Obj.SetActive(false);
        //Debug.Log("creation nouveau obj");
        //_grid[rowNb, columnNb].Obj = Instantiate(prefabList[(int) _grid[rowNb, columnNb].Direction]);
        
        
        _grid[rowNb, columnNb].Obj.GetComponent<SpriteRenderer>().sprite =
            SpriteList[(int) _grid[rowNb, columnNb].Direction];//on recupere le component qui a le sprite affichant la policy de la tile actuelle
        _grid[rowNb, columnNb].Obj.transform.position = _grid[rowNb, columnNb].Position;
        _grid[rowNb, columnNb].Obj.transform.GetChild(0).GetComponent<TMP_Text>().text = (Mathf.Round(_grid[rowNb, columnNb].vs*100f)/100f).ToString();
    }

    Direction ArgMax(int rowNb, int columnNb)
    {
        float[] tempScore = new float[4];

        if (rowNb == 3 && columnNb == 4)
        {
            Debug.Log("vs");
        }
        
        Debug.Log("current tile = " + rowNb + " " + columnNb);
        
        //left neighbour
        if (columnNb <= 0)
        {
            Debug.Log("no left nei");
            tempScore[0] = -1;
        }
        else
        {
            tempScore[0] = _grid[rowNb , columnNb - 1].vs;
        }
        
        //right neighbour
        if (columnNb >= column - 1)
        {
            Debug.Log("no right nei");
            tempScore[1] = -1;
        }
        else
        {
            Debug.Log(tempScore[0]);
            Debug.Log(_grid[rowNb,columnNb + 1].vs);
            tempScore[1] = _grid[rowNb,columnNb + 1].vs;
        }
        //up neighbour
        if (rowNb >= rows - 1)
        {
            Debug.Log("no up nei");
            tempScore[2] = -1;
                
        }
        else
        {
           tempScore[2] = _grid[rowNb+ 1, columnNb ].vs;
        }
        
        //down neighbour
        if (rowNb <= 0)
        {
            Debug.Log("no down nei");
            tempScore[3] = -1;
        }
        else
        {
            tempScore[3] = _grid[rowNb - 1, columnNb].vs;
        }

        return (Direction) System.Array.IndexOf(tempScore, tempScore.Max());
    }
    float ArgMaxVs(int rowNb, int columnNb)
    {
        float[] tempScore = new float[4];
        
        Debug.Log("current tile = " + rowNb + " " + columnNb);
        
        //left neighbour
        if (columnNb <= 0)
        {
            Debug.Log("no left nei");
            //tempScore[0] = -1;
        }
        else
        {
            
                tempScore[0] = _grid[rowNb , columnNb - 1].vs;
            
            
        }
        
        //right neighbour
        if (columnNb >= column - 1)
        {
            Debug.Log("no right nei");
            tempScore[1] = -1;
        }
        else
        {
            
                Debug.Log(tempScore[0]);
                Debug.Log(_grid[rowNb,columnNb + 1].vs);
                tempScore[1] = _grid[rowNb,columnNb + 1].vs;
            
            
        }
        //up neighbour
        if (rowNb >= rows - 1)
        {
            Debug.Log("no up nei");
            tempScore[2] = -1;
                
        }
        else
        {
            
            
                tempScore[2] = _grid[rowNb+ 1, columnNb ].vs;
            
            
        }
        
        //down neighbour
        if (rowNb <= 0)
        {
            Debug.Log("no down nei");
            tempScore[3] = -1;
        }
        else
        {
            
            
                tempScore[3] = _grid[rowNb - 1, columnNb].vs;
            
            
        }

        return tempScore.Max();
    }

    bool CheckIfWall(int rowNb, int columnNb)
    {
        foreach (var wall in wallList)
        {
            if (rowNb == wall.y && columnNb == wall.x)
            {
                return false;
            }
        }

        return true;
    }

    Direction[,] GetListDirections()
    {
        Direction[,] temp;
        temp = new Direction[rows, column];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                temp[i, j] = _grid[i, j].Direction;
            }
            
        }

        return temp;
    }

    float CheckDirection(Tile tile, int rowNb, int columnNb)
    {
        Debug.Log(tile.Direction);
        float tempVs = tile.vs;
        if (tile.Direction == Direction.Right)
        {
            Debug.Log(columnNb);
            if (columnNb >= column - 1)
            {
                Debug.Log("bvsoiu");
                return tile.vs;;
            }
            else
            {
               return gamma * _grid[rowNb, columnNb + 1].vs;
            }
        }
        if (tile.Direction == Direction.Left)
        {
            Debug.Log(columnNb);
            if (columnNb <= 0)
            {
                Debug.Log("bvsoiu");
                return tile.vs;;
            }
            else
            {
                return gamma * _grid[rowNb , columnNb - 1].vs;
            }
            
        }

        if (tile.Direction == Direction.Up)
        {
            Debug.Log(rowNb);
            if (rowNb >= rows - 1)
            {
                Debug.Log("bvsoiu");
                return tile.vs;;
                
            }
            else
            {
                return gamma * _grid[rowNb+ 1, columnNb ].vs;
            }

            
        }
        if (tile.Direction == Direction.Down)
        {
            Debug.Log(rowNb);
            if (rowNb <= 0)
            {
                Debug.Log("bvsoiu");
                return tile.vs;;
            }
            else
            {
                return gamma * _grid[rowNb - 1, columnNb].vs;
            }
            
        }

        return -10000000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
