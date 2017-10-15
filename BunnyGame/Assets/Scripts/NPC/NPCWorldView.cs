using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;

// Static class because there is only one world
public static class NPCWorldView {
    //===============================================================================
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
    //===============================================================================
    public enum WorldPlane : int {
        LAND = 0, 
        WATER
    }
    //===============================================================================
    // Not doing any locking here, if multiple NPCs were to do A* at the same time,
    // they would need their own copy anyways to get personal g, h, f values
    public class worldCellData {
        public int x, y;
        public bool blocked;
        public List<worldCellData> neighbours;
        public Vector3[] corners = new Vector3[4];

        private Vector3 _pos;
        private float _h;
        private float _g;
        private float _f;

        public Vector3 pos {
            get {
                return _pos;
            }
            set {
                _pos = value;
                corners[0] = _pos + new Vector3(cellSize / 2, 0, cellSize / 2);
                corners[1] = _pos + new Vector3(-cellSize / 2, 0, cellSize / 2);
                corners[2] = _pos + new Vector3(cellSize / 2, 0, -cellSize / 2);
                corners[3] = _pos + new Vector3(-cellSize / 2, 0, -cellSize / 2);
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
    }
    //===============================================================================
    public class GameCharacter { //Representation of living creatures in the game, from NPCs to players
        private Vector3 _pos;
        private Vector3 _dir;
        private int _id;

        public GameCharacter(int id) {
            this._id = id;
            this._pos = Vector3.zero;
            this._dir = Vector3.zero;
        }

        public GameCharacter(int id, Vector3 pos, Vector3 dir) {
            this._id = id;
            this._pos = pos;
            this._dir = dir;
        }
        public void update(Vector3 pos, Vector3 dir) {
            lock (this) {
                this._dir = dir;
                this._pos = pos;
            }
        }   
        
        public Vector3 getPos() {
            lock (this) 
                return this._pos;
        }    

        public Vector3 getDir() {
            lock (this) 
                return this._dir;
        }

        public int getId() {
            lock (this) 
                return this._id;
        }
    }
    //===============================================================================
    //===============================================================================
    public const int cellCount = 300;
    public const float worldSize = 400;
    public const float cellSize = worldSize/cellCount;

    private static Dictionary<int,GameCharacter> _npcs;
    private static Dictionary<int, GameCharacter> _players;
    private static FireWall.Circle _fireWall;
    private static bool _runNPCThread;

    private static worldCellData[,] _land;
    private static worldCellData[,] _water;

    private static Vector3 _landOffset;
    private static Vector3 _waterOffset;

    private static bool _ready;
    //===============================================================================
    static NPCWorldView() {
        init();
    }
    public static void init() {
        _runNPCThread = true;
        _ready = false;

        _npcs = new Dictionary<int, GameCharacter>();
        _players = new Dictionary<int, GameCharacter>();

        _land = new worldCellData[cellCount, cellCount];
        _water = new worldCellData[cellCount, cellCount];

        _landOffset = new Vector3(
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f),
            -15,
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f)
        );
        _waterOffset = new Vector3(
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f),
            -18,
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f)
        );

        initPlane(_land, _landOffset);
        initPlane(_water, _waterOffset);
    }
    //===============================================================================
    public static Vector3 landOffset { get { return _landOffset; } }
    public static Vector3 waterOffset { get { return _waterOffset; } }
    public static bool ready { get { return _ready; } set { _ready = value; } }
    public static FireWall.Circle FireWall { get { return _fireWall; } set { _fireWall = value; } }
    //===============================================================================
    public static void resetAStarData() {
        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                _land[x, y].g = 9999999;
                _land[x, y].h = 9999999;
                _water[x, y].g = 9999999;
                _water[x, y].h = 9999999;
            }
        }
    }

    public static int[] convertWorld2Cell(Vector3 world) {
        int[] cellPos = { 0, 0 };
        //new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2) + _offset;
        world -= landOffset;
        world /= cellSize;
        //world.x -= NPCWorldView.cellSize / 2;
        //world.z -= NPCWorldView.cellSize / 2;

        cellPos[0] = clamp((int)world.x);
        cellPos[1] = clamp((int)world.z);

        return cellPos;
    }

    // No convenient way of using get; set; or overloading [,] operator that i found
    //========================================================================================
    // Not using any locking for these, because only one thread writes to them              //
    // and only one thread reads them, so there won't be any read/write conflicts.          //
    // If thread A writes to a object that thread B reads, the consequence would            //
    // only be that the data that thread B reads is 1 frame old, which is better then having//
    // the threads block, slowing down the execution.                                       //                   
    //========================================================================================
    public static worldCellData getCell(WorldPlane plane, int x, int y) {
        if (plane == WorldPlane.LAND)
            return _land[x, y];
        if (plane == WorldPlane.WATER)
            return _water[x, y];
        else
            return null;
    }

    public static Dictionary<int, GameCharacter> getPlayers() {
        return _players;
    }
    public static Dictionary<int, GameCharacter> getNpcs() {
        return _npcs;
    }   

    public static bool getRunNPCThread() {
         return _runNPCThread;
    }
    // Only called from NPCManager when game quits, so no need for locking
    public static void setRunNPCThread(bool run) {
         _runNPCThread = run;
    }

    delegate float Line(float x, float y);
    public static float rayCast(WorldPlane plane, Vector3 start, Vector3 end) {
        //Check collisions against the firewall
        float wallCollsion = lineWallCollision(start, (end - start).normalized);
        if (wallCollsion <= Vector3.Distance(start, end))
            return wallCollsion;
        
        float a = (end.z - start.z) / (end.x - start.x + 0.000001f); //Don't want to divide by zero
        Line line = (x, y) => a * (x - start.x) - (y - start.z);

        int[] startIndex = convertWorld2Cell(start);
        int[] endIndex = convertWorld2Cell(end);
        int xStart = clamp((startIndex[0] < endIndex[0]) ? startIndex[0] : endIndex[0]);
        int xEnd = clamp(((startIndex[0] > endIndex[0]) ? startIndex[0] : endIndex[0]) + 1);
        int yStart = clamp((startIndex[1] < endIndex[1]) ? startIndex[1] : endIndex[1]);
        int yEnd = clamp(((startIndex[1] > endIndex[1]) ? startIndex[1] : endIndex[1]) + 1);

        for (int y = yStart; y < yEnd; y++) {
            for (int x = xStart; x < xEnd; x++) {
                if (plane == WorldPlane.LAND) {
                    if (_land[x, y].blocked || !_water[x, y].blocked) {
                        if (cellLineCollision(line, _land[x, y]))
                            return Vector3.Distance(start, _land[x, y].pos);
                    }
                } else if (plane == WorldPlane.WATER) {
                    if (!_land[x, y].blocked || _water[x, y].blocked) {
                        if (cellLineCollision(line, _land[x, y]))
                            return Vector3.Distance(start, _land[x, y].pos);
                    }
                }
            }
        }
        return float.MaxValue;
    }

    public static float lineWallCollision(Vector3 start, Vector3 dir) {
        // Solving the quadratic equation obtained by the intersection of 
        // a circle and a line. 
        //Line = Start + t*dir
        //Line.x = start.x + t*dir.x 
        //Line.y = start.y + t*dir.y 
        //intersection = (line.x - circle.x)^2 + (line.y - circle.y)^2 - r = 0
        //The below lines are already expanded and grouped to form the components of the quadratic equation:
        // ax^2 + bx + c = 0
        float a = dir.x*dir.x + dir.y*dir.y;
        float b = 2 * (start.x - FireWall.pos.x) * dir.x + 2 * (start.y - FireWall.pos.y) * dir.y;
        float c = Mathf.Pow(start.x - FireWall.pos.x, 2) + Mathf.Pow(start.y - FireWall.pos.y, 2) - FireWall.radius;

        float root = b * b - 4 * a * c;
        if (root < 0) return float.MaxValue;
        float t = (-b + Mathf.Sqrt(root)) / (2 * a);
        return (start + dir * t).magnitude;
    }

    public static bool writeToFile() {
        bool success = true;
        FileStream fsLand = new FileStream("./Assets/Data/land.nwl", FileMode.Create);
        FileStream fsWater = new FileStream("./Assets/Data/water.nwl", FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();
        try {
            formatter.Serialize(fsLand, getBlocked(_land));
            formatter.Serialize(fsWater, getBlocked(_water));
        } catch (SerializationException e) {
            success = false;
            Debug.Log("Failed to serialize. Reason: " + e.Message);
        }
        finally {
            fsLand.Close();
            fsWater.Close();
        }
        return success;
    }

    public static bool readFromFile() {
        bool success = true;
        try {
            FileStream fsLand = new FileStream("./Assets/Data/land.nwl", FileMode.Open);
            FileStream fsWater = new FileStream("./Assets/Data/water.nwl", FileMode.Open);

            BinaryFormatter formatter = new BinaryFormatter();
            try {
                setBlocked((bool[,])formatter.Deserialize(fsLand), _land);
                setBlocked((bool[,])formatter.Deserialize(fsWater), _water);
            } catch (SerializationException e) {
                success = false;
                Debug.Log("Failed to deserialize. Reason: " + e.Message);
            }
            finally {
                fsLand.Close();
                fsWater.Close();
            }
        } catch(FileNotFoundException e) {
            Debug.Log("Failed to open file. Reason: " + e.Message);
            success = false;
        } catch (EndOfStreamException e) {
            Debug.Log("Failed to read file. Reason: " + e.Message);
            success = false;
        }       
        return success;
    }
    //===============================================================================
    private static bool cellLineCollision(Line line, worldCellData cell) {
        int sign = 0;
        foreach (var corner in cell.corners) 
            sign += Math.Sign(line(corner.x, corner.z));
        if (Math.Abs(sign) != 4)
            return true;
        return false;
    }

    private static void initPlane(worldCellData[,] plane, Vector3 offset) {
        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                plane[x, y] = new worldCellData();
                plane[x, y].blocked = false;
                plane[x, y].pos = new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2) + offset;
                plane[x, y].x = x;
                plane[x, y].y = y;
                plane[x, y].g = 9999999;
                plane[x, y].h = 9999999;
            }
        }

        // Add neighbours for the cells
        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                plane[x, y].neighbours = new List<worldCellData>();
                for (int i = -1; i < 2; i += 2) {
                    if (x + i >= 0 && x + i < cellCount)
                        plane[x, y].neighbours.Add(plane[x + i, y]);
                    if (y + i >= 0 && y + i < cellCount)
                        plane[x, y].neighbours.Add(plane[x, y + i]);
                }
            }
        }
    }

    private static int clamp(int input) {
        input = (input >= 0) ? input : 0;
        input = (input < NPCWorldView.cellCount) ? input : NPCWorldView.cellCount - 1;
        return input;
    }

    private static bool[,] getBlocked(worldCellData[,] plane) {
        bool[,] blocked = new bool[cellCount, cellCount];
        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                blocked[x, y] = plane[x, y].blocked;
            }
        }
        return blocked;
    }

    private static void setBlocked(bool[,] blocked, worldCellData[,] plane) {
        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
               plane[x, y].blocked = blocked[x, y];
            }
        }
    }
}
