using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SokobanManager : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
    
    public struct DirectionV2
    {
        public static Vector2Int Left = new Vector2Int(-1, 0);
        public static Vector2Int Right = new Vector2Int(1, 0);
        public static Vector2Int Up = new Vector2Int(0, 1);
        public static Vector2Int Down = new Vector2Int(0, -1);
    }
    public class Tile
    {
        public GameObject Obj;
        public Vector2Int Position = new Vector2Int();
        public Direction Direction;
        public float vs = 0;
    }
    
    public class GameState
    {
        public Vector2Int positionPlayer;
        public List<Vector2Int> listePositionBox;
        public Vector2Int direction;
        public float vs;
        public int reward;
        
        public GameState DeepCopy()
        {
            GameState copy = new GameState();
            
            copy.positionPlayer = this.positionPlayer;
           
            copy.listePositionBox = new List<Vector2Int>(this.listePositionBox);

            return copy;
        }
    }

    public GameObject tilePrefab;
    public GameObject backgroundTile;
    public int rows = 3;
    public int column = 3;
    public float gamma = 0.9f;
    public Vector2Int wallTest = new Vector2Int(3, 3);
    private List<Vector2Int>  wallList;

    public List<Vector2Int> boxList;
    public List<Vector2Int> pointList;

    public List<GameObject> prefabList;
    public List<Sprite> SpriteList;
    private int IndexIteration;

    public GameObject defaultTile;
    
    
    private Tile[,] _grid;
    private Direction[,] tabDirections;
    private int nbPossibleState;
    private int nbPossibleSPosition;
    public Vector2Int positionPlayerStart;
    private List<GameState> gameStateList;
    
    private GameObject[,] gridTab;
        
    // Start is called before the first frame update
    void Start()
    {
        _grid = new Tile[rows,column];
        tabDirections = new Direction[rows, column];
        wallList = new List<Vector2Int>();
        wallList.Add(wallTest);

        nbPossibleState = 1 + boxList.Count;

        gridTab = new GameObject[rows, column];
        
    }

    public void InitSokoban()
    {//init d'abord chaque position player, puis chaque box
        gameStateList = new List<GameState>();
        InitGrid();
        nbPossibleSPosition = gridTab.Length;

        GameState st1 = new GameState();
        st1.listePositionBox = new List<Vector2Int>();
        
        for (int i = 0; i < nbPossibleSPosition; i++)
        {
            st1.positionPlayer = IndexToGrid(i);
            
            int index = 0;
            //positionPlayer = 
            GeneratePosition(st1, index);
            
        }

        GameState initialState = new GameState();
        initialState.vs = 0;
        initialState.positionPlayer = positionPlayerStart;
        initialState.listePositionBox = boxList;
        
        foreach (var s in gameStateList)
        {
            foreach (var box in s.listePositionBox)
            {
                if (pointList.Contains(box))
                {
                    s.reward += 1;
                }
            }
        }
        
        
        DisplayState(initialState);
        
    }

    void InitGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                gridTab[i, j] = Instantiate(defaultTile,new Vector3(i,j),Quaternion.identity);
            }
        }
    }

    void DisplayState(GameState state)
    {
        gridTab[(int)state.positionPlayer.x,(int)state.positionPlayer.x].GetComponent<SpriteRenderer>().color = Color.blue;

        foreach (var point in pointList)
        {
            gridTab[(int)point.x,(int)point.y].GetComponent<SpriteRenderer>().color = Color.green;
        }
        foreach (var box in state.listePositionBox)
        {
            gridTab[(int)box.x,(int)box.y].GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }
    private void GeneratePosition(GameState state, int index)
    {
        
            for (int i = 0; i < gridTab.Length; i++)
            {
                state.listePositionBox.Add(IndexToGrid(i));
                
                if (index < boxList.Count - 1)
                {
                    GeneratePosition(state,index + 1);
                    state.listePositionBox.Remove(IndexToGrid(i));
                }
                else
                {
                    if (IsValid(state))
                    {
                        bool isSame = false;
                        
                        /*//                                                    avec verification 1800 states, sans = 3360 = nombre que vous m'avez indiqué 
                        foreach (var listGameStateIndex in gameStateList)
                        {
                            int temp = 0;
                            if (listGameStateIndex.positionPlayer == state.positionPlayer)
                            {
                                for (int j = 0; j < state.listePositionBox.Count; j++)
                                {
                                    if (listGameStateIndex.listePositionBox.Contains(state.listePositionBox[j]) )
                                    {
                                        temp += 1;
                                    }
                                }

                                if (temp == listGameStateIndex.listePositionBox.Count)
                                {
                                    isSame = true;
                                    break;
                                }
                            }
                        }
                        */

                        if (isSame == false)
                        {
                            gameStateList.Add(state.DeepCopy());    
                        }
                    }
                    state.listePositionBox.Remove(IndexToGrid(i));
                }
                
                
            }

            
            
    }
    
    
    

    bool IsValid(GameState state)
    {
        //assertion nombre de box
        if (state.listePositionBox.Count != boxList.Count)
        {
            return false;
        }
        
        for (int i = 0; i < state.listePositionBox.Count; i++)
        {
            if (state.positionPlayer == state.listePositionBox[i])
            {
                return false;
            }
        }

        List<Vector2Int> temp = new List<Vector2Int>();

        for (int i = 0; i < state.listePositionBox.Count; i++)
        {
            if (temp.Contains(state.listePositionBox[i]) )
                return false;
            
            temp.Add(state.listePositionBox[i]);
        }
        return true;
    }
    

    Vector2Int IndexToGrid(int index)
    {
        return new Vector2Int(index % column, index / column);
    }
    
    public void SetUpGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                
                _grid[i, j] = new Tile();
                _grid[i, j].vs = 0f;
                _grid[i, j].Position = new Vector2Int(j, i);
                _grid[i, j].Direction = (Direction) Random.Range(0, 4);
                tabDirections[i, j] = _grid[i, j].Direction;
                Instantiate(backgroundTile, new Vector3(j,i,0.1f),Quaternion.identity);
                _grid[i, j].Obj = Instantiate(tilePrefab);
                _grid[i, j].Obj.GetComponent<SpriteRenderer>().sprite =
                    SpriteList[(int) _grid[i, j].Direction];
            }
        }
        
        _grid[rows -1, column - 1].vs = 1f;
    }

    public void ValueIteration()
    {

        float delta = 0f;
        float teta = 10f;

        foreach (var VARIABLE in gameStateList)
        {
            VARIABLE.vs = 0;
        }
        
        while (delta < teta)
        {
            delta = 0;
            foreach (var state in gameStateList)
            {
                GameState tempInitial = state.DeepCopy();
                GameState temp = state.DeepCopy();
                float max = 0;
                bool isBoxPush = false;
                List<Vector2Int> listPossibleMove = CheckAllPossibleMove(temp);

                GameState result = new GameState();
                
                foreach (var direction in listPossibleMove)
                {
                    Vector2Int posToFindState = temp.positionPlayer + direction;
                    GameState stateToFind = new GameState();
                    stateToFind.positionPlayer = posToFindState;
                    
                    stateToFind.listePositionBox = temp.listePositionBox;
                    bool temp2 = false;
                    //check if push box
                        for (int i = 0; i < stateToFind.listePositionBox.Count; i++)
                        {
                            
                            if (stateToFind.listePositionBox[i] == stateToFind.positionPlayer)
                            {
                                stateToFind.listePositionBox[i] = stateToFind.listePositionBox[i] + direction;
                                temp2 = true;
                            }
                        }
                    

                    result = FindState(stateToFind);
                    if (result.vs > max) max = result.vs;
                }
                

                state.vs = state.reward + gamma * result.vs;

                delta = Mathf.Max(delta, Mathf.Abs(tempInitial.vs - result.vs));
            }
            
            
            
        }
        ApplyQs();
        GameState initialState = new GameState();
        initialState.positionPlayer = positionPlayerStart;
        initialState.listePositionBox = boxList;

        initialState = FindState(initialState);
        
        DisplayState(initialState);
        
        

        StartCoroutine(LaunchIA(initialState));
        
    }
    IEnumerator LaunchIA(GameState state)
    {
        
        DisplayState(state);
            
        yield return new WaitForSeconds(2);
        
        
        GameState temp = state.DeepCopy();
        temp.positionPlayer = temp.positionPlayer + temp.direction;
        temp = FindState(temp);
        LaunchIA(temp);
    }

    GameState FindState(GameState stateToFind)
    {
        
        int test = 0;
        foreach (var state in gameStateList)
        {
            test += 1;
            if (stateToFind.positionPlayer == state.positionPlayer)
            {
                int temp = 0;
                GameState copy = new GameState();
                copy =  state.DeepCopy();
                foreach (var boxPos in stateToFind.listePositionBox)
                {
                    
                    if (copy.listePositionBox.Contains(boxPos))
                    {
                        temp += 1;
                        copy.listePositionBox.Remove(boxPos);
                    }
                }
                if (temp == state.listePositionBox.Count)
                {
                    return state;
                }
                
            }
            
        }
        
        return new GameState();
    }

    List<Vector2Int> CheckAllPossibleMove(GameState state)
    {
        List <Vector2Int> possibleDirections= new List<Vector2Int>();
        Vector2Int playerPos = state.positionPlayer;
        //droite
        
        Vector2Int rightTile = playerPos;
        rightTile = playerPos + DirectionV2.Right;
        if (playerPos.x + 1 < column)
        {
            if (state.listePositionBox.Contains(rightTile))
            {
                if ((rightTile + DirectionV2.Right).x < column - 1)//pour l'emplacement ou serait deplacé la box
                {
                    if (!state.listePositionBox.Contains((rightTile + DirectionV2.Right)))
                    {
                        possibleDirections.Add(DirectionV2.Right);
                    }
                }
            }
            else
            {
                possibleDirections.Add(DirectionV2.Right);
            }
            
            
        }
        //left
        Vector2Int leftTile = playerPos;
        leftTile = playerPos + DirectionV2.Left;
        if (playerPos.x - 1 >= 0)
        {
            if (state.listePositionBox.Contains(leftTile))
            {
                if ((leftTile + DirectionV2.Left).x - 1 > 0)//pour l'emplacement ou serait deplacé la box
                {
                    if (!state.listePositionBox.Contains((leftTile + DirectionV2.Left)))
                    {
                        possibleDirections.Add(DirectionV2.Left);
                    }
                }
            }
            else
            {
                possibleDirections.Add(DirectionV2.Left);
            }
            
            
        }
        //up
        Vector2Int upTile = playerPos;
        upTile = playerPos + DirectionV2.Up;
        if (playerPos.y + 1 < rows)
        {
            if (state.listePositionBox.Contains(upTile))
            {
                if ((upTile + DirectionV2.Up).y < rows - 1)//pour l'emplacement ou serait deplacé la box
                {
                    if (!state.listePositionBox.Contains((upTile + DirectionV2.Up)))
                    {
                        possibleDirections.Add(DirectionV2.Up);
                    }
                }
            }
            else
            {
                possibleDirections.Add(DirectionV2.Up);
            }

            
            
        }
        //down
        Vector2Int downTile = playerPos;
        downTile = playerPos + DirectionV2.Down;
        if (playerPos.y - 1 >= 0)
        {
            if (state.listePositionBox.Contains(downTile))
            {
                if ((downTile + DirectionV2.Down).y > 0)//pour l'emplacement ou serait deplacé la box
                {
                    if (!state.listePositionBox.Contains((downTile + DirectionV2.Down)))
                    {
                        possibleDirections.Add(DirectionV2.Down);
                    }
                }
            }
            else
            {
                possibleDirections.Add(DirectionV2.Down);
            }
            
            
        }

        
        return possibleDirections;
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
            }
            
        }
    }

    void PolicyImprovment()
    {
        bool isPolicyStable = true;
        Direction temp2;
        
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (i == rows - 1 && j == column - 1)
                {
                    break;
                }
                temp2 = _grid[i, j].Direction;
                _grid[i,j].Direction = ArgMax( i, j);
                
                if (temp2 != _grid[i,j].Direction)
                {
                    isPolicyStable = false;
                    ChangeArrow(i,j);
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
        foreach (var state in gameStateList)
        {
            GameState temp = state.DeepCopy();
            List<Vector2Int> possibleMove = CheckAllPossibleMove(temp);
            GameState result = new GameState();
            float max = 0;
                
            foreach (var direction in possibleMove)
            {
                Vector2Int posToFindState = temp.positionPlayer + direction;
                GameState stateToFind = new GameState();
                stateToFind.positionPlayer = posToFindState;
                stateToFind.listePositionBox = temp.listePositionBox;
                
                for (int i = 0; i < stateToFind.listePositionBox.Count; i++)
                {
                            
                    if (stateToFind.listePositionBox[i] == stateToFind.positionPlayer)
                    {
                        stateToFind.listePositionBox[i] = stateToFind.listePositionBox[i] + direction;
                    }
                }

                result = FindState(stateToFind);
                if (result.vs > max) state.direction = direction;
            }
        }
    }

    void ChangeArrow(int rowNb, int columnNb)
    {
        _grid[rowNb, columnNb].Obj.GetComponent<SpriteRenderer>().sprite =
            SpriteList[(int) _grid[rowNb, columnNb].Direction];
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
