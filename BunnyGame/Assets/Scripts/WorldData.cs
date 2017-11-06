using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldData {

    static string map = "Island42";


    //===============================================================================
    public const int cellCount = 150;
    public const float worldSize = 400;
    public const float cellSize = worldSize / cellCount;

    private static Vector2 _xzOffset;
    private static Dictionary<string, float[]> _mapOffsets;
    private static WorldGrid _worldGrid;

    private static bool _ready;
    //===============================================================================
    static WorldData() {
        float[] islandOffsets = { 0, 5 };
        float[] island42Offsets = { 0, 5, 20, 25, 30 };
        _mapOffsets = new Dictionary<string, float[]>();
        _mapOffsets.Add("Island", islandOffsets);
        _mapOffsets.Add("Island42", island42Offsets);
        _xzOffset = new Vector2(
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f), 
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f)
        );

        init();
    }

    public static void init() {
        _ready = false;
                
    }

    public static void clear() { 
        _ready = false;

        _land = null;
        _water = null;
    }
    //===============================================================================
    public static Vector3 landOffset { get { return _landOffset; } }
    public static Vector3 waterOffset { get { return _waterOffset; } }
    public static bool ready { get { return _ready; } set { _ready = value; } }
    public static WorldGrid worldGrid { get { return _worldGrid; } }
    public static worldCellData[,] land { get { return _land; } }
    //===============================================================================

    public static int[] convertWorld2Cell(Vector3 world) {
        int[] cellPos = { 0, 0 };
        world -= landOffset;
        world /= cellSize;

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
    public static worldCellData getCell(bool land, int x, int y) {
        x = clamp(x); y = clamp(y);
        if (land)
            return _land[x, y];
        else
            return _water[x, y];
    }

    public static worldCellData getCell(bool land, Vector3 pos) {
        int[] index = convertWorld2Cell(pos);
        if (land)
            return _land[index[0], index[1]];
        else
            return _water[index[0], index[1]];
    }

    delegate float Line(float x, float y);
    public static float rayCast(bool land, Vector3 start, Vector3 end) {
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
                if (land) {
                    if (_land[x, y].blocked) {
                        if (cellLineCollision(line, _land[x, y]))
                            return Vector3.Distance(start, _land[x, y].pos);
                    }
                } else {
                    if (_water[x, y].blocked) {
                        if (cellLineCollision(line, _land[x, y]))
                            return Vector3.Distance(start, _land[x, y].pos);
                    }
                }
            }
        }
        return float.MaxValue;
    }

    public static bool writeToFile() {
        bool success = true;
        try {
            FileStream fsLand = new FileStream("./Assets/Data/land.nwl", FileMode.Create, FileAccess.Write);
            FileStream fsWater = new FileStream("./Assets/Data/water.nwl", FileMode.Create, FileAccess.Write);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fsLand, getBlocked(_land));
            formatter.Serialize(fsWater, getBlocked(_water));
            fsLand.Close();
            fsWater.Close();
        } catch (Exception e) {
            success = false;
            Debug.Log("Failed to serialize. Reason: " + e.Message);
        }

        return success;
    }

    public static bool readFromFile() {
        bool success = true;
        try {
            FileStream fsLand = new FileStream("./Assets/Data/land.nwl", FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream fsWater = new FileStream("./Assets/Data/water.nwl", FileMode.Open, FileAccess.Read, FileShare.Read);

            BinaryFormatter formatter = new BinaryFormatter();

            setBlocked((bool[,])formatter.Deserialize(fsLand), _land);
            setBlocked((bool[,])formatter.Deserialize(fsWater), _water);

            fsLand.Close();
            fsWater.Close();
        } catch (Exception e) {
            Debug.Log("Failed to open file. Reason: " + e.Message);
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

        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                plane[x, y].plusNeighbours = new List<worldCellData>();
                plane[x, y].crossNeighbours = new List<worldCellData>();
                plane[x, y].neighbours = new List<worldCellData>();
                for (int yi = -1; yi < 2; yi++) {
                    for (int xi = -1; xi < 2; xi++) {
                        int xc = x + xi, yc = y + yi;
                        if (bounds(xc) && bounds(yc) && !(xc == x && yc == y)) {
                            if (xc != x && yc != y) plane[x, y].crossNeighbours.Add(plane[xc, yc]);
                            else if (xc == x || yc == y) plane[x, y].plusNeighbours.Add(plane[xc, yc]);
                            plane[x, y].neighbours.Add(plane[xc, yc]);
                        }
                    }
                }
            }
        }
    }

    private static int clamp(int input) {
        input = (input >= 0) ? input : 0;
        input = (input < cellCount) ? input : cellCount - 1;
        return input;
    }

    private static bool bounds(int input) {
        return (input >= 0 && input < cellCount);
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
