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

            // Copiez toutes les propriétés de l'objet actuel dans le nouvel objet
            // Assurez-vous de copier également les objets internes de manière récursive si nécessaire
            copy.positionPlayer = this.positionPlayer;
            // Copiez d'autres propriétés et champs si nécessaire

            // Si "listePositionBox" est un objet mutable (par exemple, une liste), effectuez également une copie profonde de cette liste
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
    public Vector2Int boxTest = new Vector2Int(2, 3);
    public Vector2Int boxTest2 = new Vector2Int(1, 3);
    public Vector2Int pointTest = new Vector2Int(4, 3);
    private List<Vector2Int>  wallList;

    public List<Vector2Int> boxList;
    public List<Vector2Int> pointList;
    //private List<Vector2Int>  holeList;

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
        //boxList = new List<Vector2Int>();
        wallList.Add(wallTest);
        //boxList.Add(boxTest);
        //boxList.Add(boxTest2);

        nbPossibleState = 1 + boxList.Count;
        //nbPossibleSPosition = _grid.Length;
        //nbPossibleSPosition = 3;

        gridTab = new GameObject[rows, column];
        //Debug.Log(11/ column);
        //Debug.Log(11 % column);

        //InitSokoban();
        //PolicyIteration();
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
            if (st1.positionPlayer.x == 2 && st1.positionPlayer.y == 1)
            {
                Debug.Log("bvsiru");
            }
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
        //List<Vector2Int> tempList = new List<Vector2Int>(_boxList);
        
            for (int i = 0; i < gridTab.Length; i++)
            {
                state.listePositionBox.Add(IndexToGrid(i));
                if (state.listePositionBox.Contains(new Vector2Int(1,1)))
                {
                    if (state.listePositionBox.Contains(new Vector2Int(0,0)))
                    {
                            if (state.positionPlayer == new Vector2Int(1,0))
                            {
                                Debug.Log("posBoxMissExist");
                            }
                        
                        
                    }
                }
                
                //Debug.Log(boxList[i]);
                if (index < boxList.Count - 1)
                {
                    GeneratePosition(state,index + 1);
                    state.listePositionBox.Remove(IndexToGrid(i));
                }
                else
                {
                    if (IsValid(state))
                    {
                        //GameState copyState = new GameState();
                        //copyState = state;
                        //List<Vector2Int> copyList = new List<Vector2Int>(state.listePositionBox);
                        //copyState.listePositionBox = copyList;
                        bool isSame = false;
                        
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

                        if (isSame == false)
                        {
                            gameStateList.Add(state.DeepCopy());    
                        }
                        
                        //gameStateList.Add(state.DeepCopy());
                        
                        
                        //state.listePositionBox.Remove(IndexToGrid(i));
                    }
                    state.listePositionBox.Remove(IndexToGrid(i));
                }
                
                
            }

            
            
    }
    
    

    bool IsValid(GameState state)
    {
        GameState copy = state.DeepCopy();
        if (copy.listePositionBox.Contains(new Vector2Int(2,0)))
        {
            copy.listePositionBox.Remove(new Vector2Int(2, 0));
            if (copy.listePositionBox.Contains(new Vector2Int(2,0)))
            {
                if (copy.positionPlayer == new Vector2Int(1,0))
                {
                    Debug.Log("posBoxMissExist");
                }
                        
                        
            }
        }
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
                //Debug.Log(_grid[i, j].Position);
                _grid[i, j].Position = new Vector2Int(j, i);
                //Instantiate(actualGrid.Obj); 
                _grid[i, j].Direction = (Direction) Random.Range(0, 4);
                tabDirections[i, j] = _grid[i, j].Direction;
                Instantiate(backgroundTile, new Vector3(j,i,0.1f),Quaternion.identity);
                _grid[i, j].Obj = Instantiate(tilePrefab);
                _grid[i, j].Obj.GetComponent<SpriteRenderer>().sprite =
                    SpriteList[(int) _grid[i, j].Direction];
                
                //_grid[i, j].Obj.transform.position = _grid[i, j].Position;
                //Debug.Log(_grid[i, j].Direction);
            }
        }

        
        
        //Debug.Log(_grid[0, 0]) ;
        //Debug.Log(rows);
        //Debug.Log(column);
        //Debug.Log(_grid[rows -2, column - 2].vs);
        _grid[rows -1, column - 1].vs = 1f;

        //_grid[(int)wallTest.y,(int)wallTest.x].vs = -10;
        

    }

    public void ValueIteration()
    {
        //SetUpGrid();

        float delta = 1f;
        float teta = 0.02f;

        foreach (var VARIABLE in gameStateList)
        {
            VARIABLE.vs = 0;
        }
        
        while (delta > teta)
        {
            delta = 0;
            foreach (var state in gameStateList)
            {
                GameState temp = state;
                float max = 0;
                bool isBoxPush = false;
                List<Vector2Int> listPossibleMove = CheckAllPossibleMove(state,out isBoxPush);

                GameState result = new GameState();
                
                foreach (var direction in listPossibleMove)
                {
                    Vector2Int posToFindState = state.positionPlayer + direction;
                    GameState stateToFind = new GameState();
                    stateToFind.positionPlayer = posToFindState;
                    
                    stateToFind.listePositionBox = state.listePositionBox;
                    
                    //check if push box
                        for (int i = 0; i < stateToFind.listePositionBox.Count; i++)
                        {
                            if (stateToFind.listePositionBox[i] == stateToFind.positionPlayer)
                            {
                                stateToFind.listePositionBox[i] = stateToFind.listePositionBox[i] + direction;
                            }
                        }
                    

                    result = FindState(stateToFind);
                    if (result.vs > max) max = result.vs;
                }

                if (result.vs > 0)
                {
                    Debug.Log("kcusd");
                }

                state.vs = state.reward + gamma * result.vs;

                delta = Mathf.Max(delta, Mathf.Abs(temp.vs - result.vs));
            }
            
            
            /*
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
                
            
            }*/
        }

        //DebugResultsPolicicy();
        ApplyQs();

    }

    GameState FindState(GameState stateToFind)
    {
        if (stateToFind.listePositionBox.Contains(new Vector2Int(1,1)))
        {
            if (stateToFind.listePositionBox.Contains(new Vector2Int(0,0)))
            {
                if (stateToFind.positionPlayer == new Vector2Int(1,0))
                {
                    Debug.Log("posBoxMissExist");
                }
                        
                        
            }
        }
        int test = 0;
        foreach (var state in gameStateList)
        {
            if (state.listePositionBox.Contains(new Vector2Int(1,1)))
            {
                if (state.listePositionBox.Contains(new Vector2Int(0,0)))
                {
                    if (state.positionPlayer == new Vector2Int(1,0))
                    {
                        Debug.Log("posBoxMissExist");
                        if (stateToFind.listePositionBox.Contains(new Vector2Int(1,1)))
                        {
                            if (stateToFind.listePositionBox.Contains(new Vector2Int(0,0)))
                            {
                                if (stateToFind.positionPlayer == new Vector2Int(1,0))
                                {
                                    Debug.Log("posBoxMissExist");
                                }
                            }
                        }
                    }
                        
                        
                }
            }
            if (stateToFind.listePositionBox.Contains(new Vector2Int(1,1)))
            {
                if (stateToFind.listePositionBox.Contains(new Vector2Int(0,0)))
                {
                    if (stateToFind.positionPlayer == new Vector2Int(1,0))
                    {
                        if (state.listePositionBox.Contains(new Vector2Int(1,1)))
                        {
                            if (state.listePositionBox.Contains(new Vector2Int(0,0)))
                            {
                                if (state.positionPlayer == new Vector2Int(1,0))
                                {
                                    Debug.Log("posBoxMissExist");
                                }
                        
                        
                            }
                        }
                        Debug.Log("posBoxMissExist");
                    }
                        
                        
                }
            }
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
        Debug.Log("no gamestate found");
        
        return new GameState();
    }

    List<Vector2Int> CheckAllPossibleMove(GameState state,out bool isBoxPush)
    {
        List <Vector2Int> possibleDirections= new List<Vector2Int>();
        Vector2Int playerPos = state.positionPlayer;
        //droite
        
        Vector2Int rightTile = playerPos;
        rightTile = new Vector2Int(rightTile.x + 1, rightTile.y);
        if (playerPos.x + 1 < column - 1)
        {
            if (boxList.Contains(rightTile))
            {
                if (!wallList.Contains(new Vector2Int(rightTile.x + 1, rightTile.y)) || rightTile.x + 1 < column - 1)
                {
                    possibleDirections.Add(DirectionV2.Right);
                    isBoxPush = true;
                }
            }
            else if(!wallList.Contains(rightTile))
            {
                possibleDirections.Add(DirectionV2.Right);
            }
        }
        //left
        Vector2Int leftTile = playerPos;
        leftTile = new Vector2Int(leftTile.x - 1, leftTile.y);
        if (playerPos.x - 1 > 0)
        {
            if (boxList.Contains(leftTile))
            {
                if (!wallList.Contains(new Vector2Int(leftTile.x - 1, leftTile.y)) || leftTile.x - 1 > 0)
                {
                    possibleDirections.Add(DirectionV2.Left);
                    isBoxPush = true;
                }
            }
            else if(!wallList.Contains(leftTile))
            {
                possibleDirections.Add(DirectionV2.Left);
            }
        }
        //up
        Vector2Int upTile = playerPos;
        upTile = new Vector2Int(upTile.x, upTile.y + 1);
        if (playerPos.y < rows - 1)
        {
            if (boxList.Contains(upTile))
            {
                if (!wallList.Contains(new Vector2Int(upTile.x, leftTile.y + 1)) || leftTile.y + 1 < rows - 1)
                {
                    possibleDirections.Add(DirectionV2.Up);
                    isBoxPush = true;
                }
            }
            else if(!wallList.Contains(upTile))
            {
                possibleDirections.Add(DirectionV2.Up);
            }
        }
        //down
        Vector2Int downTile = playerPos;
        downTile = new Vector2Int(downTile.x, downTile.y - 1);
        if (playerPos.y > 0)
        {
            if (boxList.Contains(downTile))
            {
                if (!wallList.Contains(new Vector2Int(downTile.x, downTile.y - 1)) || downTile.y - 1 > 0)
                {
                    possibleDirections.Add(DirectionV2.Down);
                    isBoxPush = true;
                }
            }
            else if(!wallList.Contains(downTile))
            {
                possibleDirections.Add(DirectionV2.Down);
            }
        }

        isBoxPush = false;
        return possibleDirections;
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
                //_grid[i, j].Obj.transform.GetChild(0).GetComponent<TMP_Text>().text = (Mathf.Round(_grid[i, j].vs*100f)/100f).ToString();
            }
            
        }
    }

    void PolicyImprovment()
    {
        bool isPolicyStable = true;
        //Direction[,] temp = new Direction[rows,column];
        //Direction[,] listFinal = new Direction[rows,column];
        Direction temp2;
        
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (i == rows - 1 && j == column - 1)
                {
                    break;
                }
                Debug.Log(i + " " + j);
                temp2 = _grid[i, j].Direction;
                Debug.Log("direction avant = " + _grid[i,j].Direction );
                _grid[i,j].Direction = ArgMax( i, j);

                //listFinal = GetListDirections();
                if (temp2 != _grid[i,j].Direction)
                {
                    isPolicyStable = false;
                    ChangeArrow(i,j);
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
        /*
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
        */
        //tester
        foreach (var state in gameStateList)
        {
            bool isbool =false;
            List<Vector2Int> possibleMove = CheckAllPossibleMove(state, out isbool);
            GameState result = new GameState();
            float max = 0;
                
            foreach (var direction in possibleMove)
            {
                Vector2Int posToFindState = state.positionPlayer + direction;
                GameState stateToFind = new GameState();
                stateToFind.positionPlayer = posToFindState;
                stateToFind.listePositionBox = state.listePositionBox;

                result = FindState(stateToFind);
                if (result.vs > max) state.direction = direction;
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
            SpriteList[(int) _grid[rowNb, columnNb].Direction];
        //_grid[rowNb, columnNb].Obj.transform.position = _grid[rowNb, columnNb].Position;
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
            tempScore[0] = -1;
        }
        else
        {
            if (CheckIfWall(rowNb,columnNb))
            {
                tempScore[0] = -1;
            }
            else
            {
                tempScore[0] = _grid[rowNb , columnNb - 1].vs;
            }
            
        }
        
        //right neighbour
        if (columnNb >= column - 1)
        {
            Debug.Log("no right nei");
            tempScore[1] = -1;
        }
        else
        {
            if (CheckIfWall(rowNb,columnNb))
            {
                tempScore[1] = -1;
                
            }
            else
            {
                Debug.Log(tempScore[0]);
                Debug.Log(_grid[rowNb,columnNb + 1].vs);
                tempScore[1] = _grid[rowNb,columnNb + 1].vs;
            }
            
        }
        //up neighbour
        if (rowNb >= rows - 1)
        {
            Debug.Log("no up nei");
            tempScore[2] = -1;
                
        }
        else
        {
            if (CheckIfWall(rowNb,columnNb))
            {
                tempScore[2] = -1;
            }
            else
            {
                tempScore[2] = _grid[rowNb+ 1, columnNb ].vs;
            }
            
        }
        
        //down neighbour
        if (rowNb <= 0)
        {
            Debug.Log("no down nei");
            tempScore[3] = -1;
        }
        else
        {
            if (CheckIfWall(rowNb,columnNb))
            {
                tempScore[3] = -1;
            }
            else
            {
                tempScore[3] = _grid[rowNb - 1, columnNb].vs;
            }
            
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
