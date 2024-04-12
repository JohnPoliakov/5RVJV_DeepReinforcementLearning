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
    public int rows = 3;
    public int column = 3;
    public float gamma = 0.9f;
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
    }

    
    public void SetUpGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                _grid[i, j] = new Tile();
                _grid[i, j].vs = 0f;
                _grid[i, j].Position = new Vector2(j, i);
                _grid[i, j].Direction = (Direction) Random.Range(0, 4);
                tabDirections[i, j] = _grid[i, j].Direction;
                _grid[i, j].Obj = Instantiate(tilePrefab);
                _grid[i, j].Obj.GetComponent<SpriteRenderer>().sprite =
                    SpriteList[(int) _grid[i, j].Direction];
                
                _grid[i, j].Obj.transform.position = _grid[i, j].Position;
            }
        }

        
        
        
        _grid[rows -1, column - 1].vs = 1f;//on definit l'arrivée comme etant la derniere case
        

        foreach (var hole in holeList)
        {
            _grid[(int) hole.y, (int) hole.x].vs = -10;
        }

    }

    public void ValueIteration()
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
                    float temp = _grid[i, j].vs;

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
                temp = _grid[i, j].Direction;
                _grid[i,j].Direction = ArgMax( i, j);

                //listFinal = GetListDirections();
                if (temp != _grid[i,j].Direction)
                {
                    isPolicyStable = false;
                    ChangeArrow(i,j);//fonction qui va actualiser
                }
                
            }
            
        }
        

        if (isPolicyStable)
        {
            return;
        }
        else
        {
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
        _grid[rowNb, columnNb].Obj.GetComponent<SpriteRenderer>().sprite =
            SpriteList[(int) _grid[rowNb, columnNb].Direction];//on recupere le component qui a le sprite affichant la policy de la tile actuelle
        _grid[rowNb, columnNb].Obj.transform.position = _grid[rowNb, columnNb].Position;
        _grid[rowNb, columnNb].Obj.transform.GetChild(0).GetComponent<TMP_Text>().text = (Mathf.Round(_grid[rowNb, columnNb].vs*100f)/100f).ToString();
    }

    Direction ArgMax(int rowNb, int columnNb)
    {
        float[] tempScore = new float[4];

        
        
       
        
        //left neighbour
        if (columnNb <= 0)
        {
            tempScore[0] = -1;
        }
        else
        {
            tempScore[0] = _grid[rowNb , columnNb - 1].vs;
        }
        
        //right neighbour
        if (columnNb >= column - 1)
        {
            tempScore[1] = -1;
        }
        else
        {
            tempScore[1] = _grid[rowNb,columnNb + 1].vs;
        }
        //up neighbour
        if (rowNb >= rows - 1)
        {
            tempScore[2] = -1;
                
        }
        else
        {
           tempScore[2] = _grid[rowNb+ 1, columnNb ].vs;
        }
        
        //down neighbour
        if (rowNb <= 0)
        {
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
        if (columnNb <= 0)
        {
        }
        else
        {
                tempScore[0] = _grid[rowNb , columnNb - 1].vs;
        }
        
        //right neighbour
        if (columnNb >= column - 1)
        {
            tempScore[1] = -1;
        }
        else
        {
                tempScore[1] = _grid[rowNb,columnNb + 1].vs;
        }
        //up neighbour
        if (rowNb >= rows - 1)
        {
            tempScore[2] = -1;
                
        }
        else
        {
            tempScore[2] = _grid[rowNb + 1, columnNb].vs;
        }
        
        //down neighbour
        if (rowNb <= 0)
        {
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
    

    float CheckDirection(Tile tile, int rowNb, int columnNb)
    {
        float tempVs = tile.vs;
        if (tile.Direction == Direction.Right)
        {
            if (columnNb >= column - 1)
            {
                return tile.vs;;
            }
            else
            {
               return gamma * _grid[rowNb, columnNb + 1].vs;
            }
        }
        if (tile.Direction == Direction.Left)
        {
            if (columnNb <= 0)
            {
                return tile.vs;;
            }
            else
            {
                return gamma * _grid[rowNb , columnNb - 1].vs;
            }
            
        }

        if (tile.Direction == Direction.Up)
        {
            if (rowNb >= rows - 1)
            {
                return tile.vs;;
                
            }
            else
            {
                return gamma * _grid[rowNb+ 1, columnNb ].vs;
            }

            
        }
        if (tile.Direction == Direction.Down)
        {
            if (rowNb <= 0)
            {
                return tile.vs;;
            }
            else
            {
                return gamma * _grid[rowNb - 1, columnNb].vs;
            }
            
        }

        return -10000000;
    }
    
}
