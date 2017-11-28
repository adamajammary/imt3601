using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

public class WorldGrid {
    //Comparer that allows duplicate keys, usefull for A*
    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y) {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1;   // Handle equality as beeing greater
            else
                return result;
        }

        #endregion
    }

    public class Cell {
        public int x, y, z;
        public bool blocked;
        //Got "manhatten" plusNeighbours and diag crossNeighbours, because not all path finding will allow diag pathing.
        public List<Cell> plusNeighbours;
        public List<Cell> crossNeighbours;
        public List<Cell> neighbours;
        public Vector3[] corners = new Vector3[4];

        private Vector3 _pos;

        // A* data
        private float _h;
        private float _g;
        private float _f;
        private Cell _parent;

        public Vector3 pos {
            get {
                return _pos;
            }
            set {
                _pos = value;
                corners[0] = _pos + new Vector3(_cellSize / 2, 0, _cellSize / 2);
                corners[1] = _pos + new Vector3(-_cellSize / 2, 0, _cellSize / 2);
                corners[2] = _pos + new Vector3(_cellSize / 2, 0, -_cellSize / 2);
                corners[3] = _pos + new Vector3(-_cellSize / 2, 0, -_cellSize / 2);
            }
        }

        public float h {
            get {
                return _h;
            }
            set {
                _h = value;
                _f = _h + _g;
            }
        }
        public float g {
            get {
                return _g;
            }
            set {
                _g = value;
                _f = _g + _h;
            }
        }
        public float f {
            get {
                return _f;
            }
        }
        public Cell parent {
            get {
                return _parent;
            }
            set {
                _parent = value;
            }
        }
    }

    private Cell[,,] _worldGrid;
    private List<Cell>[] _openCells;
    private List<Cell>[] _blockedCells;
    private static string _name;
    private static float[] _yOffsets;
    private static Vector2 _xzOffsets;
    private static int _cellCount;
    private static float _worldSize;
    private static float _cellSize;
    //=============================Init code==============================================
    public WorldGrid(string name, float[] yOffsets, Vector2 xzOffsets, int cellCount, float worldSize) {
        this._worldGrid = new Cell[cellCount, yOffsets.Length, cellCount];
        _name = name;
        _yOffsets = yOffsets;
        _xzOffsets = xzOffsets;
        _cellCount = cellCount;
        _worldSize = worldSize;
        _cellSize = worldSize / cellCount;

        initGrid();
    }

    public void lateInit() {
        this._openCells = new List<Cell>[planeCount];
        this._blockedCells = new List<Cell>[planeCount];
        for (int i = 0; i < planeCount; i++) {
            this._openCells[i] = new List<Cell>();
            this._blockedCells[i] = new List<Cell>();
        }

        foreach (Cell cell in this._worldGrid) {
            if (cell.blocked)
                this._blockedCells[cell.y].Add(cell);
            else
                this._openCells[cell.y].Add(cell);
        }
    }

    private void initGrid() {
        for (int y = 0; y < yOffsets.Length; y++) {
            for (int z = 0; z < cellCount; z++) {
                for (int x = 0; x < cellCount; x++) {
                    initCell(x, y, z);
                }
            }

            for (int z = 0; z < cellCount; z++) {
                for (int x = 0; x < cellCount; x++) {
                    addNeighbours(x, y, z);
                }
            }
        }
    }

    private void initCell(int x, int y, int z) {
        this._worldGrid[x, y, z] = new Cell();
        this._worldGrid[x, y, z].blocked = false;
        this._worldGrid[x, y, z].pos = new Vector3(x * cellSize + cellSize / 2, 0, z * cellSize + cellSize / 2) 
                                     + new Vector3(xzOffsets.x, yOffsets[y], xzOffsets.y);
        this._worldGrid[x, y, z].x = x;
        this._worldGrid[x, y, z].y = y;
        this._worldGrid[x, y, z].z = z;
        this._worldGrid[x, y, z].g = 9999999;
        this._worldGrid[x, y, z].h = 9999999;
    }

    private void addNeighbours(int x, int y, int z) {
        this._worldGrid[x, y, z].plusNeighbours = new List<Cell>();
        this._worldGrid[x, y, z].crossNeighbours = new List<Cell>();
        this._worldGrid[x, y, z].neighbours = new List<Cell>();
        for (int zi = -1; zi < 2; zi++) {
            for (int xi = -1; xi < 2; xi++) {
                int xc = x + xi, zc = z + zi;
                if (bounds(xc) && bounds(zc) && !(xc == x && zc == z)) {
                    if (xc != x && zc != z) this._worldGrid[x, y, z].crossNeighbours.Add(this._worldGrid[xc, y, zc]);
                    else if (xc == x || zc == z) this._worldGrid[x, y, z].plusNeighbours.Add(this._worldGrid[xc, y, zc]);
                    this._worldGrid[x, y, z].neighbours.Add(this._worldGrid[xc, y, zc]);
                }
            }
        }
    }

    private bool bounds(int input) {
        return (input >= 0 && input < cellCount);
    }
    //=============================Accessors==============================================
    //================================
    public string name { get { return _name; } }
    public int cellCount { get { return _cellCount; } }
    public int planeCount { get { return yOffsets.Length; } }
    public float worldSize { get {return _worldSize; } }
    public float cellSize { get { return _cellSize; } }
    public float[] yOffsets { get { return _yOffsets; } }
    public Vector2 xzOffsets { get { return _xzOffsets; } }
    //================================

    public Cell getCell(int x, int y, int z) {
        x = clamp(x);
        y = clamp(y);
        z = clamp(z);
        return this._worldGrid[x, y, z];
    }

    public Cell getCell(Vector3 pos, int y) {
        y = clamp(y);
        int[] index = convertWorld2Cell(pos);
        return this._worldGrid[index[0], y, index[1]];
    }

    public Cell getCell(Vector3 pos) {
        int[] index = convertWorld2Cell(pos);
        return this._worldGrid[index[0], getClosestLevel(pos), index[1]];
    }

    public Cell getCellNoWater(Vector3 pos) {
        int[] index = convertWorld2Cell(pos);
        return this._worldGrid[index[0], getClosestLevelNoWater(pos), index[1]];
    }

    public Cell getRandomCell(bool blocked, int level) {
        if (blocked)
            return this._blockedCells[level][UnityEngine.Random.Range(0, this._blockedCells[level].Count)];
        else
            return this._openCells[level][UnityEngine.Random.Range(0, this._openCells[level].Count)];
    }

    public int getClosestLevel(Vector3 pos) {
        int level = 0;
        for (int i = 1; i < yOffsets.Length; i++) {
            if (Mathf.Abs(yOffsets[i] - pos.y) < Mathf.Abs(yOffsets[level] - pos.y))
                level = i;
        }
        return level;
    }

    public int getClosestLevel(Vector3 pos, int start, int end) {
        int level = start;
        for (int i = start + 1; i < end; i++) {
            if (Mathf.Abs(yOffsets[i] - pos.y) < Mathf.Abs(yOffsets[level] - pos.y))
                level = i;
        }
        return level;
    }

    public int getClosestLevelNoWater(Vector3 pos) {
        return getClosestLevel(pos, 1, yOffsets.Length);
    }

    public int[] convertWorld2Cell(Vector3 world) {
        int[] cellPos = { 0, 0 };
        world -= new Vector3(_xzOffsets.x, 0, _xzOffsets.y);
        world /= _cellSize;

        cellPos[0] = clamp((int)world.x);
        cellPos[1] = clamp((int)world.z);

        return cellPos;
    }

    private int clamp(int input) {
        input = (input >= 0) ? input : 0;
        input = (input < _cellCount) ? input : _cellCount - 1;
        return input;
    }

    //=============================File I/O functions==============================================
    public bool writeToFile() {
        bool success = true;
        try {
            FileStream fs = new FileStream("./Assets/Data/" + _name + ".bd", FileMode.Create, FileAccess.Write);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, getBlocked(_worldGrid));
            fs.Close();
        } catch (Exception e) {
            success = false;
            Debug.Log("Failed to serialize. Reason: " + e.Message);
        }

        return success;
    }

    public bool readFromFile() {
        bool success = true;
        try {
            Directory.CreateDirectory("./Assets/Data/");
            FileStream fs = new FileStream("./Assets/Data/" + _name + ".bd", FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter formatter = new BinaryFormatter();
            setBlocked((bool[,,])formatter.Deserialize(fs), _worldGrid);
            fs.Close();
        } catch (Exception e) {
            Debug.Log("Failed to open file. Reason: " + e.Message);
            success = false;
        }

        return success;
    }

    private static bool[,,] getBlocked(Cell[,,] grid) {
        bool[,,] blocked = new bool[_cellCount, _yOffsets.Length, _cellCount];
        for (int y = 0; y < _yOffsets.Length; y++) {
            for (int z = 0; z < _cellCount; z++) {
                for (int x = 0; x < _cellCount; x++) {
                    blocked[x, y, z] = grid[x, y, z].blocked;
                }
            }
        }
        return blocked;
    }

    private static void setBlocked(bool[,,] blocked, Cell[,,] grid) {
        for (int y = 0; y < _yOffsets.Length; y++) {
            for (int z = 0; z < _cellCount; z++) {
                for (int x = 0; x < _cellCount; x++) {
                    grid[x, y, z].blocked = blocked[x, y, z];                }
            }
        }
    }
    //=============================AI Helpers==============================================

    public void resetAStarData() {
        foreach (Cell cell in this._worldGrid) {
            cell.g = 9999999;
            cell.h = 9999999;
            cell.parent = null;
        }
    }

    //---------- Functions that let you raycast in the WorldGrid----------------
    delegate float Line(float x, float z);
    public float rayCast(int level, Vector3 start, Vector3 end) {
        float a = (end.z - start.z) / (end.x - start.x + 0.000001f); //Don't want to divide by zero
        Line line = (x, z) => a * (x - start.x) - (z - start.z);

        int[] startIndex = convertWorld2Cell(start);
        int[] endIndex = convertWorld2Cell(end);
        int xStart = clamp((startIndex[0] < endIndex[0]) ? startIndex[0] : endIndex[0]);
        int xEnd = clamp(((startIndex[0] > endIndex[0]) ? startIndex[0] : endIndex[0]) + 1);
        int zStart = clamp((startIndex[1] < endIndex[1]) ? startIndex[1] : endIndex[1]);
        int zEnd = clamp(((startIndex[1] > endIndex[1]) ? startIndex[1] : endIndex[1]) + 1);

        for (int z = zStart; z < zEnd; z++) {
            for (int x = xStart; x < xEnd; x++) {                
                if (_worldGrid[x, level, z].blocked) {
                    if (cellLineCollision(line, _worldGrid[x, level, z]))
                        return Vector3.Distance(start, _worldGrid[x, level, z].pos);
                }                
            }
        }
        return float.MaxValue;
    }

    private bool cellLineCollision(Line line, Cell cell) {
        int sign = 0;
        foreach (var corner in cell.corners)
            sign += Math.Sign(line(corner.x, corner.z));
        if (Math.Abs(sign) != 4)
            return true;
        return false;
    }
}
