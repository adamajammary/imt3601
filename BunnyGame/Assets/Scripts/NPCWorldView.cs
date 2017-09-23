using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static class because there is only one world
public static class NPCWorldView {
    public struct worldCellData {
        public bool blocked;
    }

    public const int cellCount = 150;
    public const float cellWorldSize = 2.3f;

    private static object _worldLock;
    private static worldCellData[,] _world;

    static NPCWorldView() {
        _worldLock = new object();
        _world = new worldCellData[cellCount, cellCount];
        for (int y = 0; y < cellCount; y++)
            for (int x = 0; x < cellCount; x++)
                _world[x, y].blocked = false;
    }


    // No convenient way of using get; set; or overloading [,] operator that i found
    public static worldCellData getCell(int x, int y) {
        lock(_worldLock)
            return _world[x, y];
    }

    public static void setCell(int x, int y, worldCellData cell) {
        lock (_worldLock)
            _world[x, y] = cell;
    }
}
