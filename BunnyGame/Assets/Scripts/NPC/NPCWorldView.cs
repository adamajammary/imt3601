using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

// Static class because there is only one world
public static class NPCWorldView {   
    //===============================================================================
    public class GameCharacter { //Representation of living creatures in the game, from NPCs to players
        private Vector3 _pos;
        private Vector3 _dir;
        private Vector3 _goal;
        private WorldGrid _grid;
        private int _id;
        private bool _alive;

        public GameCharacter(int id) {
            this._id = id;
            this._pos = Vector3.zero;
            this._dir = Vector3.zero;
            this._goal = Vector3.negativeInfinity;
            this._alive = true;
            this._grid = WorldData.worldGrid;
        }

        public GameCharacter(int id, Vector3 pos, Vector3 dir) {
            this._id = id;
            this._pos = pos;
            this._dir = dir;
        }
        public void update(Vector3 pos, Vector3 dir, Vector3 goal) {
            lock (this) {
                this._dir = dir;
                this._pos = pos;
                this._goal = goal;
            }
        }

        public Vector3 getPos() {
            lock (this)
                return this._pos;
        }

        public WorldGrid.Cell getCellNoWater() {
            lock (this)
                return this._grid.getCellNoWater(this._pos);
        }

        public int getLevelNoWater() {
            lock (this)
                return this._grid.getClosestLevelNoWater(this._pos);
        }

        public Vector3 getDir() {
            lock (this)
                return this._dir;
        }

        public int getId() {
            lock (this)
                return this._id;
        }

        public Vector3 getGoal() {
            lock (this)
                return this._goal;
        }

        public bool alive { get { return this._alive; } set { this._alive = value; } }
        public WorldGrid worldGrid { get { return this._grid; } set { this._grid = value; } }
    }
    //===============================================================================
    //===============================================================================
    private static Dictionary<int, GameCharacter> _npcs;
    private static Dictionary<int, GameCharacter> _players;
    private static FireWall.Circle _fireWall;

    private static bool _ready;
    //===============================================================================
    static NPCWorldView() {
        init();
    }
    public static void init() {
        _ready = false;

        _npcs = new Dictionary<int, GameCharacter>();
        _players = new Dictionary<int, GameCharacter>();
    }

    public static void clear() {
        _ready = false;

        _npcs = null;
        _players = null;
    }
    //===============================================================================
    public static Dictionary<int, GameCharacter> players { get { return _players; } }
    public static Dictionary<int, GameCharacter> npcs { get { return _npcs; } }
    public static bool ready { get { return _ready; } set { _ready = value; } }
    public static FireWall.Circle FireWall { get { return _fireWall; } set { _fireWall = value; } }
    //===============================================================================
}
