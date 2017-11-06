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
        _worldGrid = new WorldGrid(map, _mapOffsets[map], _xzOffset, cellCount, worldSize);       
    }

    public static void clear() { 
        _ready = false;
        _worldGrid = null;
    }
    //===============================================================================
    public static Vector2 xzOffset { get { return _xzOffset; } }
    public static bool ready { get { return _ready; } set { _ready = value; } }
    public static WorldGrid worldGrid { get { return _worldGrid; } }
    //===============================================================================
}
