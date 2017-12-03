using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldData {
    //===============================================================================
    private static int          _cellCount;
    private static float        _worldSize;
    private static float        _cellSize;
    private static float[]      _yOffsets;
    private static Vector2      _xzOffset;
    private static WorldGrid    _worldGrid;
    private static string       _name;
    private static bool         _ready;
    //===============================================================================
    static WorldData() {
        clear();
    }

    public static void init(IslandData data) {
        _ready = false;

        _cellCount  = data.cellCount;
        _worldSize  = data.worldSize;
        _cellSize   = worldSize / cellCount;
        _yOffsets   = data.yOffsets;
        _name       = data.name;

        _xzOffset = new Vector2(
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f),
            -(cellCount * cellSize / 2.0f + cellSize / 2.0f)
        );

        _worldGrid = new WorldGrid(name, yOffsets, xzOffset, cellCount, worldSize);          
    }

    public static void clear() { 
        _ready = false;
        _worldGrid = null;
    }
    //===============================================================================
    public static int cellCount { get { return _cellCount; } }
    public static float worldSize { get { return _worldSize; } }
    public static float cellSize { get { return _cellSize; } }
    public static int planeCount { get { return yOffsets.Length; } }
    public static float[] yOffsets { get { return _yOffsets; } }
    public static Vector2 xzOffset { get { return _xzOffset; } }
    public static bool ready { get { return _ready; } set { _ready = value; } }
    public static string name { get { return _name; } }
    public static WorldGrid worldGrid { get { return _worldGrid; } }
    //===============================================================================
}
