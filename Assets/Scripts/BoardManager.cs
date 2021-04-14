using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : Singleton<BoardManager>
{
    // Square root of three
    private const float sqrtThree = 1.732050807f;

    public int bombCounter = 9;
    public int bombSpawnScore = 1000;

    public float hexagonEdgeLength = 0.54f;

    [Header("Prefabs")]
    public GameObject dotPrefab;
    public GameObject CoordObjPrefab;
    public GameObject bombPrefab;


    [Space]
    public Vector3 CoordinationOffset;
    public float horizontalOffset;
    public float verticalOffset;

    [Header("Grid Values")]
    public int height = 9;
    public int width = 8;

    [Header("Hexagons")]

    //Will be set from editor
    public HexType[] hexTypes;
    [Space]

    // Information about board
    public Hex[,] board;

    // Coordinates for snapping the hexagons
    public GameObject[,] objCoords;

    public Dot[,] dots;

    [HideInInspector]
    public List<BombBehaviour> bombsReferences;

    [HideInInspector]
    public Transform hexParent, dotParent, socketsParent;


    // board state
    //public bool boardChanging = false;

    private Vector3 screenBottomLeftPosition;

    private GameManager gameManager;
    void Awake()
    {
        width = PlayerPrefs.GetInt("GridWidth");
        height = PlayerPrefs.GetInt("GridHeight");
    }

void Start() {
        gameManager = GameManager.Instance;
        GenerateCoordinates(); //Generating

        GenerateHexagons();

        GenerateDots();
        // If there are matching hexes changes them at the start
        RefreshMatchingHexesStart();
    }

    public bool IsBoardChanging() {
        foreach (Hex hex in board) {
            GameObject go = hex.go;
            if(go)
            if (hex.go.GetComponent<HexBehaviour>().state != HexBehaviour.HexState.Idle) {
                return true;
            }
        }
        return false;
    }

    void GenerateCoordinates() {

        objCoords = new GameObject[width, height]; //Creating object - contains coordinate objects

        //
        screenBottomLeftPosition = new Vector3(-Camera.main.orthographicSize * 9 / 16, -Camera.main.orthographicSize, 0.0f);

        socketsParent = new GameObject("HexSockets").transform;
        socketsParent.transform.parent = transform;

        Vector3 position = screenBottomLeftPosition + CoordinationOffset;
       

        //To fix position  //Change maybe ??
        if (width < 7)
        {
            CoordinationOffset.x = CoordinationOffset.x + hexagonEdgeLength * (11f - width);
        }
        if (width < 14 && width >= 7 )
        {
            CoordinationOffset.x = CoordinationOffset.x + hexagonEdgeLength * (12f - width);
        }
        
        // Generating positions of coordinate objects first
        for (int y = 0; y < height; y++) {

            float yPos = y * hexagonEdgeLength * sqrtThree;

            for (int x = 0; x < width; x++) {

                position = screenBottomLeftPosition + CoordinationOffset;

                position.x += (1.5f * hexagonEdgeLength) * x;

                position.y += yPos;


                if (x % 2 == 1) {
                    position.y -= (hexagonEdgeLength * sqrtThree / 2);
                }


                GameObject go = Instantiate(CoordObjPrefab, position, Quaternion.identity);
                go.transform.parent = socketsParent.transform;
                go.name = "HexSocket(" + x + "," + y + ")";

                objCoords[x, y] = go;
            }
        }
    }

    void GenerateHexagons () {

        board = new Hex[width, height];
        hexParent = new GameObject("Hexes").transform;
        hexParent.transform.parent = transform;

        // Generate Hexes that will be placed on sockets
        for (int y = 0; y < height; y++) {

            for (int x = 0; x < width; x++) {
                CreateHex(x, y);
            }
        }

    }

    // Dots will be the master and they'll each have three slaves 
    // that are the sockets position on board that will be controlled by the master dot
    void GenerateDots() {
       
        
        
        dots = new Dot[width - 1, (height - 1) * 2];

        dotParent = new GameObject("Dots").transform;
        dotParent.transform.parent = transform;
        Vector3 position = Vector3.zero;

        for (int y = 0; y < (height - 1) * 2; y++) {
            int yIndex = Mathf.CeilToInt(y / 2);
            for (int x = 0; x < width - 1; x++) {
                bool isLeft = true;
                Vector2Int[] slaves = new Vector2Int[3];

                if (y % 2 == 0) {
                    if (x % 2 == 0) {
                        //left sockets
                        Vector3 left = objCoords[x, yIndex].transform.position;
                        Vector3 top = objCoords[x + 1, yIndex + 1].transform.position;
                        Vector3 bot = objCoords[x + 1, yIndex].transform.position;

                        slaves[0].x = x;
                        slaves[0].y = yIndex;
                        slaves[1].x = x + 1;
                        slaves[1].y = yIndex + 1;
                        slaves[2].x = x + 1;
                        slaves[2].y = yIndex;

                        Vector3 middle = left + bot + top;
                        middle /= 3.0f;

                        position = middle;

                    } else {
                        //rigth sockets
                        isLeft = false;

                        Vector3 right = objCoords[x + 1, yIndex].transform.position;
                        Vector3 top = objCoords[x, yIndex + 1].transform.position;
                        Vector3 bot = objCoords[x, yIndex].transform.position;

                        slaves[0].x = x + 1;
                        slaves[0].y = yIndex;
                        slaves[1].x = x;
                        slaves[1].y = yIndex + 1;
                        slaves[2].x = x;
                        slaves[2].y = yIndex;

                        Vector3 middle = right + bot + top;
                        middle /= 3.0f;

                        position = middle;
                    }

                } else {
                    if (x % 2 == 0) {
                        //right
                        isLeft = false;
                        Vector3 right = objCoords[x + 1, yIndex + 1].transform.position;
                        Vector3 top = objCoords[x, yIndex + 1].transform.position;
                        Vector3 bot = objCoords[x, yIndex].transform.position;

                        slaves[0].x = x + 1;
                        slaves[0].y = yIndex + 1;
                        slaves[1].x = x;
                        slaves[1].y = yIndex + 1;
                        slaves[2].x = x;
                        slaves[2].y = yIndex;

                        Vector3 middle = right + bot + top;
                        middle /= 3.0f;

                        position = middle;

                    }
                    
                    else {
                        //left
                        Vector3 left = objCoords[x, yIndex + 1].transform.position;
                        Vector3 top = objCoords[x + 1, yIndex + 1].transform.position;
                        Vector3 bot = objCoords[x + 1, yIndex].transform.position;

                        slaves[0].x = x;
                        slaves[0].y = yIndex + 1;
                        slaves[1].x = x + 1;
                        slaves[1].y = yIndex + 1;
                        slaves[2].x = x + 1;
                        slaves[2].y = yIndex;

                        Vector3 middle = left + bot + top;
                        middle /= 3.0f;

                        position = middle;

                    }
                }

                GameObject go = Instantiate(dotPrefab, position, Quaternion.Euler(0, isLeft ? 0 : -180.0f, 0));
                go.transform.parent = dotParent.transform;
                go.name = "Dot(" + x + "," + y + ")";

                dots[x, y] = new Dot(go, x, y, slaves);
                var behaviour = go.GetComponent<DotBehaviour>();
                behaviour.dotIndexX = x;
                behaviour.dotIndexY = y;
                behaviour.dotObject = dots[x, y];
                behaviour.isLeft = isLeft;
            }
        }
    }

    public List<Vector2Int> FindMatches() {
        List<Vector2Int> matchPositions = new List<Vector2Int>();

        for (int y = 0; y < height - 1; y++) {
            if (board[0, y].IsSameType(board[0, y + 1])) {
                if (board[0, y + 1].IsSameType(board[1, y + 1])) {
                    matchPositions.Add(new Vector2Int(0, y));
                    matchPositions.Add(new Vector2Int(0, y + 1));
                    matchPositions.Add(new Vector2Int(1, y + 1));
                }
            }
        }

        for (int x = 1; x < width - 1; x++) {
            if (x % 2 == 0) {
                for (int y = 0; y < height - 1; y++) {
                    if (board[x, y].IsSameType(board[x, y + 1])) {

                        if (board[x, y + 1].IsSameType(board[x + 1, y + 1])) {
                            matchPositions.Add(new Vector2Int(x, y));
                            matchPositions.Add(new Vector2Int(x, y + 1));
                            matchPositions.Add(new Vector2Int(x + 1, y + 1));
                        }
                        if (board[x, y + 1].IsSameType(board[x - 1, y + 1])) {
                            matchPositions.Add(new Vector2Int(x, y));
                            matchPositions.Add(new Vector2Int(x, y + 1));
                            matchPositions.Add(new Vector2Int(x - 1, y + 1));
                        }
                    }
                }
            } else {
                for (int y = 0; y < height - 1; y++) {
                    if (board[x, y].IsSameType(board[x, y + 1])) {

                        if (board[x, y + 1].IsSameType(board[x + 1, y])) {
                            matchPositions.Add(new Vector2Int(x, y));
                            matchPositions.Add(new Vector2Int(x, y + 1));
                            matchPositions.Add(new Vector2Int(x + 1, y));
                        }
                        if (board[x, y + 1].IsSameType(board[x - 1, y])) {
                            matchPositions.Add(new Vector2Int(x, y));
                            matchPositions.Add(new Vector2Int(x, y + 1));
                            matchPositions.Add(new Vector2Int(x - 1, y));
                        }
                    }
                }
            }
        }

        for (int y = 0; y < height - 1; y++) {
            if (board[width - 1, y].IsSameType(board[width - 1, y + 1])) {
                if (board[width - 1, y+1 ].IsSameType(board[width - 2, y ])) {//+1
                    matchPositions.Add(new Vector2Int(width - 1, y));
                    matchPositions.Add(new Vector2Int(width - 1, y + 1));
                    matchPositions.Add(new Vector2Int(width - 2, y ));
                }
            }
        }

        return matchPositions;
    }

    // Just for start
    public void RefreshMatchingHexesStart() {
        List<Vector2Int> matchingHexesPositions = FindMatches();
        while (matchingHexesPositions.Count > 0)
        {
            foreach (Vector2Int pos in matchingHexesPositions)
            {
                ChangeHex(board[pos.x, pos.y]);
            }
            matchingHexesPositions = FindMatches();
        }
    }

    public bool RefreshMatchingHexes() {
        bool isThereAnyMatch = false;
        List<Vector2Int> matchingHexesPositions = FindMatches();
        matchingHexesPositions.Sort((a, b) => b.y.CompareTo(a.y)); ;
        if (matchingHexesPositions.Count > 0) {
            isThereAnyMatch = true;
            foreach (Vector2Int pos in matchingHexesPositions) {
                ExplodeHexagon(board[pos.x, pos.y]);
            }
            gameManager.IncreaseMovementCounter();
            gameManager.AddScore(matchingHexesPositions.Count * gameManager.pointMultiplier);
        }
        return isThereAnyMatch;
    }

    public void CreateHex(int x,int y) {
        GameObject socket = objCoords[x, y];
        Debug.Log("create");
        HexType type = RandomHex();
        GameObject go = Instantiate(type.prefab, socket.transform.position+Vector3.up*2.0f, type.prefab.transform.rotation);

        SpriteRenderer go_SpriteRenderer = go.GetComponentInChildren<SpriteRenderer>();
        go_SpriteRenderer.color = type.hexColor;
        //Debug.Log("-x--" + x + "--yy--" + y);
        go.transform.parent = hexParent.transform;
        go.name = "Hex(" + x + "," + y + ")";
        board[x, y] = new Hex(go, type, x, y);
        HexBehaviour hexBehaviour = go.GetComponent<HexBehaviour>();
        hexBehaviour.hexReference = board[x, y];
        hexBehaviour.SetPosition(socket.transform.position);
    }

    public void MoveHexDown(int x,int y) {
        if (y == 0)
            return;
        Hex hexRef = board[x, y];
        board[x, y-1] = hexRef;
        hexRef.y--;
        hexRef.go.GetComponent<HexBehaviour>().SetPosition(objCoords[x, y-1].transform.position);
    }

    public void ExplodeHexagon(Hex hex) {
        // Shift Down the Board
        for (int y = hex.y; y < height - 1; y++) {
            MoveHexDown(hex.x, y + 1);
        }

        // Create New Hex On Top
        int score = gameManager.gameState.Score;
        // Decide if a bomb should be spawned
        if (score > bombSpawnScore && score % bombSpawnScore < gameManager.pointMultiplier * 5 && bombsReferences.Count < 2) {
            CreateBombHex(hex.x, height - 1);
        } else {
            CreateHex(hex.x, height - 1);
        }

        DestroyHex(hex);
    }

    public void DestroyHex(Hex hex) {
        GameObject willBeDestroyed = hex.go;

        // Play particle
        hex.go.GetComponent<HexBehaviour>().particles.Play();
        foreach (var sr in hex.go.GetComponentsInChildren<SpriteRenderer>()) {
            sr.enabled = false;
        }
        Canvas canvas = hex.go.GetComponentInChildren<Canvas>();
        if (canvas)
            canvas.enabled = false;

        // Destroy Game Object after 1 second wait for particles to finish
        Destroy(willBeDestroyed, 1.0f);
    }

    void ChangeHex(Hex hex) {//Change the HEXAGON's type if necessary ()
        Vector3 pos = hex.go.transform.position;
        Quaternion rot = hex.go.transform.rotation;
        string name = hex.go.transform.name;
        Transform parent = hex.go.transform.parent;
        
        HexType type = RandomHex();
        while (type.id == hex.type.id) 
            type = RandomHex();

        hex.type = type;

        GameObject newGO = Instantiate(hex.type.prefab, pos, rot, parent);
        newGO.name = name;
        newGO.GetComponent<HexBehaviour>().SetPosition(hex.go.GetComponent<HexBehaviour>().GetTargetPosition());
        Destroy(hex.go);
        hex.go = newGO;
        SpriteRenderer go_SpriteRenderer = hex.go.GetComponentInChildren<SpriteRenderer>();
        go_SpriteRenderer.color = type.hexColor;
    }

    public void CreateBombHex(int x, int y) {
        GameObject socket = objCoords[x, y];

        HexType type = RandomHex();
        GameObject go = Instantiate(type.prefab, socket.transform.position + Vector3.up * 4.0f, type.prefab.transform.rotation);
        go.transform.parent = hexParent.transform;
        go.name = "BombHexagon(" + x + "," + y + ")";
        board[x, y] = new Hex(go, type, x, y);
        HexBehaviour hexBehaviour = go.GetComponent<HexBehaviour>();
        hexBehaviour.hexReference = board[x, y];

        GameObject bombGO = Instantiate(bombPrefab, socket.transform.position + Vector3.up * 4.11f + Vector3.right*0.07f, Quaternion.identity, go.transform);

        BombBehaviour bb = bombGO.GetComponent<BombBehaviour>();
        bombsReferences.Add(bb);
        bb.Init(bombCounter);
        
        hexBehaviour.SetPosition(socket.transform.position);

    }

    public HexType RandomHex() {
        var a = hexTypes[Random.Range(0, hexTypes.Length)];
           // Debug.Log(a.ToString());
        return a;
    }
}

[System.Serializable]
public struct HexType
{
    public byte id;
    public string typeName;// for debug
    public GameObject prefab;
    public Color hexColor;
}
[System.Serializable]
public class Hex 
{
    public GameObject go; // GameObject reference
    public HexType type;
    public int x, y;// index on the board

    public Hex(GameObject go, HexType type, int x, int y) {
        this.go = go;
        this.type = type;
        this.x = x;
        this.y = y;

    }

    public Hex Clone() {
        return new Hex(this.go, this.type, this.x, this.y);
    }

    public bool IsSameType(Hex other) {
        return this.type.id == other.type.id;
    }

    public void ChangeColor(Color color) {
        this.go.GetComponentInChildren<SpriteRenderer>().color = color;
    }
    
}

[System.Serializable]
public struct Dot
{
    public GameObject go;
    public int x, y;

    public Vector2Int [] slavesBoardIndex;
    public Dot(GameObject go, int x, int y) {
        this.go = go;
        this.x = x;
        this.y = y;
        slavesBoardIndex = new Vector2Int[3];
    }
    public Dot(GameObject go, int x, int y, Vector2Int[] slaves) {
        this.go = go;
        this.x = x;
        this.y = y;
        slavesBoardIndex = slaves;
    }
}

