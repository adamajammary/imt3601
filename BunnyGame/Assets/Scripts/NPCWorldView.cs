using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static class because there is only one world
public static class NPCWorldView {
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

    public class worldCellData {
        public int x, y;
        public Vector3 pos;
        public bool blocked;
        public List<worldCellData> neighbours;      

        private float _h;
        private float _g;
        private float _f;

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

    public const int cellCount = 150;
    public const float cellSize = 2.3f;
   
    private static object _worldLock;
    private static worldCellData[,] _world;
    private static Vector3 _offset;

    static NPCWorldView() {
        _offset = new Vector3(-(cellCount * cellSize / 2.0f + cellSize / 2.0f),
                              -15,
                              -(cellCount * cellSize / 2.0f + cellSize / 2.0f));

        _worldLock = new object();
        _world = new worldCellData[cellCount, cellCount];
        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                _world[x, y] = new worldCellData();
                _world[x, y].blocked = false;
                _world[x, y].pos = new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2) + _offset;
                _world[x, y].x = x;
                _world[x, y].y = y;
                _world[x, y].g = 9999999;
                _world[x, y].h = 9999999;
            }
        }

        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                _world[x, y].neighbours = new List<worldCellData>();
                for (int i = -1; i < 2; i += 2) {
                    if (x + i >= 0 && x + i < cellCount)
                        _world[x, y].neighbours.Add(_world[x + i, y]);
                    if (y + i >= 0 && y + i < cellCount)
                        _world[x, y].neighbours.Add(_world[x, y + i]);
                }
            }
        }
    }

    public static Vector3 offset { get { return _offset; } }
    public static void resetAStarData() {
        for (int y = 0; y < cellCount; y++) {
            for (int x = 0; x < cellCount; x++) {
                _world[x, y].g = 9999999;
                _world[x, y].h = 9999999;
            }
        }
    }

    // No convenient way of using get; set; or overloading [,] operator that i found
    public static worldCellData getCell(int x, int y) {
        lock(_worldLock)
            return _world[x, y];
    }
}
