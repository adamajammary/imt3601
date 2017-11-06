using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
        public int x, y;
        public bool blocked;
        //Got "manhatten" plusNeighbours and diag plusNeighbours, because not all path finding will allow diag pathing.
        public List<Cell> plusNeighbours;
        public List<Cell> crossNeighbours;
        public List<Cell> neighbours;
        public Vector3[] corners = new Vector3[4];

        private Vector3 _pos;

        // A* data
        private float _h;
        private float _g;
        private float _f;
        private worldCellData _parent;

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
        public worldCellData parent {
            get {
                return _parent;
            }
            set {
                _parent = value;
            }
        }
    }

    private Cell[,,] _worldGrid;
    private float[] _yOffsets;
    private Vector2 _xzOffsets;
    private int _cellCount;
    private float _worldSize;
    private float _cellSize;

    public WorldGrid() {
        this._worldGrid = new Cell[10,10,10];
    }

    //================================
    public int cellCount { get => this._cellCount;  set { this._cellCount = value; _cellSize = worldSize / cellCount; } }
    public float worldSize { get => this._worldSize; set { this._worldSize = value; _cellSize = worldSize / cellCount; } }
    public float cellSize { get => this._cellSize; }

    //================================

    public Cell getCell(int x, int y, int z) {
        return this._worldGrid[x, y, z];
    }

    public Cell getCell(Vector3 pos) {
        return this._worldGrid[x, y, z];
    }

    public static int[] convertWorld2Cell(Vector3 world) {
        int[] cellPos = { 0, 0 };
        world -= _xzOffsets;
        world /= cellSize;

        cellPos[0] = clamp((int)world.x);
        cellPos[1] = clamp((int)world.z);

        return cellPos;
    }

    public void resetAStarData() {
        foreach (Cell cell in this._worldGrid) {
            cell.g = 9999999;
            cell.h = 9999999;
            cell.parent = null;
        }
    }
}
