using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBrain { 
    //==============Member variables==================================================
    private Stack<State>                            _state;
    private NPCWorldView.GameCharacter              _npc;
    private BlockingQueue<NPCThread.instruction>    _instructions;
    private float                                   _speed;

    //==============Constructor==================================================
    public NPCBrain(NPCWorldView.GameCharacter npc, BlockingQueue<NPCThread.instruction> i){
        this._npc               = npc;
        this._state             = new Stack<State>();
        this._instructions      = i;
        this._speed             = 1.0f;

        this._state.Push(new Roam(this));
    }

    //==============Update==================================================
    public void update() {
        this._state.Peek().update();
    }

    //==============Functions==================================================
    public bool npcAlive() {
        return this._npc != null;
    }

    private void sendInstuction(Vector3 dir) {
        if (dir == Vector3.zero || dir == this._npc.getDir()) return;
        NPCThread.instruction i = new NPCThread.instruction(this._npc.getId(), dir.normalized * this._speed, Vector3.negativeInfinity);
        this._instructions.Enqueue(i);
    }

    private void sendInstuction(Vector3 dir, Vector3 goal) {
        if (dir == Vector3.zero || dir == this._npc.getDir()) return;
        NPCThread.instruction i = new NPCThread.instruction(this._npc.getId(), dir.normalized * this._speed, goal);
        this._instructions.Enqueue(i);
    }

    //==============State super class==================================================
    private class State {
        protected NPCBrain _brain;
        public State(NPCBrain x) { this._brain = x; }
        public virtual void update() { }

        //Usefull functions for any state
        protected bool detectObstacle() {
            var npc = this._brain._npc;
            float viewDist = 5;
            float fov = 45;
            Vector3 dir = npc.getDir();
            Vector3[] eyes = new Vector3[3];
            eyes[0] = dir;
            eyes[1] = Quaternion.AngleAxis(fov, Vector3.up) * dir;
            eyes[2] = Quaternion.AngleAxis(-fov, Vector3.up) * dir;
            foreach (var eye in eyes) {
                if (NPCWorldView.rayCast(true, npc.getPos(), npc.getPos() + eye * viewDist) != float.MaxValue)
                    return true;
            }
            return false;
        }

        //protected Vector3 turnTowards(Vector3 current, Vector3 dir) {
        //    float turnAngle = 10;

        //    Vector3 left = Quaternion.AngleAxis(turnAngle, Vector3.up) * current;
        //    Vector3 right = Quaternion.AngleAxis(-turnAngle, Vector3.up) * current;
        //    float leftAngle = angle(dir, left);
        //    float rightAngle = angle(dir, right);
        //    Vector3 retVec = (leftAngle <= rightAngle) ? left : right;
        //    return (leftAngle >= turnAngle || rightAngle >= turnAngle) ? retVec : dir;
        //}

        //float angle(Vector3 a3, Vector3 b3) {
        //    Vector2 a2 = new Vector2(a3.x, a3.z);
        //    Vector2 b2 = new Vector2(b3.x, b3.z);
        //    return Vector2.Angle(a2, b2);
        //}

        protected bool inDanger() {
            var npc = this._brain._npc;
            var players = NPCWorldView.getPlayers();
            foreach (var player in players.Values)
                if (Vector3.Distance(npc.getPos(), player.getPos()) < 20)
                    return true;
           
            float dist = Vector3.Distance(npc.getPos(), NPCWorldView.FireWall.pos);
            if ((NPCWorldView.FireWall.radius - dist) < 20)
                return true;

            return false;
        }
    }

    //==============State classes==================================================

    //==============Roam state==================================================
    private class Roam : State {
        Vector3 lastCell;
        public Roam(NPCBrain x) : base(x) {
            lastCell = new Vector3(-1000, -1000, -1000);
        }

        override public void update() {
            roam();
            if (inDanger()) {
                this._brain._state.Push(new FleeDanger(this._brain));
            } else if (detectObstacle()) {
                this._brain._state.Push(new AvoidObstacle(this._brain));
            }
        }

        private void roam() {
            Vector3 roamDir = Vector3.zero;
            Vector3 mapPos = this._brain._npc.getMapPos();
            if (lastCell == mapPos) return;
            if (hash(mapPos) < 0.5)
                roamDir = Quaternion.AngleAxis(5, Vector3.up) * this._brain._npc.getDir();
            else
                roamDir = Quaternion.AngleAxis(5, Vector3.up) * this._brain._npc.getDir();
            this._brain.sendInstuction(roamDir);
            lastCell = mapPos;
        }

        private float hash(Vector3 i) {
            float temp = Mathf.Abs((Mathf.Sin(i.x) + Mathf.Sin(i.y) + Mathf.Sin(i.z)) * 1464.468f);
            return temp - (int)temp;
        }
    }

    //==============Avoid obstacle state==================================================
    private class AvoidObstacle : State {
        protected Stack<NPCWorldView.worldCellData> _path;
        protected Vector3 _goal;
        public AvoidObstacle(NPCBrain x) : base(x) {
            this._goal = Vector3.negativeInfinity;
            this._path = new Stack<NPCWorldView.worldCellData>();
            AStar(this._brain._npc.getCell(), findTargetCell(x._npc.getDir()));
        }

        override public void update() {
            walkPath();           

            if (inDanger()) {
                this._brain._state.Pop();
                this._brain._state.Push(new FleeDanger(this._brain));
            } else if (this._path.Count == 0) {
                this._brain._state.Pop(); // Return to previous state
            }            
        }

        protected void walkPath() {
            recalcPath();
            var npc = this._brain._npc;
            Vector3 pathDir;

            pathDir = this._path.Peek().pos - npc.getPos();
            pathDir.y = 0;
            pathDir.Normalize();
            this._brain.sendInstuction(pathDir, this._goal);
            if (npc.getCell() == this._path.Peek()) this._path.Pop();
        }

        protected void recalcPath() { 
            var npc = this._brain._npc;
            if (npc.getGoal() != this._goal) { //This happends when a client is out of sync with master, and they calculate different paths
                AStar(npc.getCell(), NPCWorldView.getCell(true, this._goal));
            }
        }

        protected NPCWorldView.worldCellData findTargetCell(Vector3 prefDir) {
            NPCWorldView.worldCellData target = null;
            Vector3 testDir;
            float degInc = 180 / 8;
            while (target == null) {
                float start = 5;
                target = probeDir(prefDir, start);
                if (target != null) break;
                for (float deg = degInc; deg < 180; deg += degInc) {
                    testDir = Quaternion.AngleAxis(deg, Vector3.up) * prefDir;
                    target = probeDir(testDir, start);
                    if (target != null) break;
                    testDir = Quaternion.AngleAxis(-deg, Vector3.up) * prefDir;
                    target = probeDir(testDir, start);
                    if (target != null) break;
                }
                start += 10;
            }
            return target; //This shouldn't happen
        }

        private NPCWorldView.worldCellData probeDir(Vector3 dir, float start) {
            Vector3 pos = this._brain._npc.getPos();
            float probeLen = start + 10;
            float mult;
            float cellSize = NPCWorldView.cellSize;
            for (mult = start; mult < probeLen; mult++) {
                Vector3 rayEnd = pos + dir * cellSize * mult;
                var cell = NPCWorldView.getCell(true, rayEnd);
                var waterCell = NPCWorldView.getCell(false, rayEnd);
                if (!cell.blocked && waterCell.blocked)
                    return cell;
            }
            return null;            
        }

        protected void AStar(NPCWorldView.worldCellData startCell, NPCWorldView.worldCellData goal) {
            Dictionary<Vector3, NPCWorldView.worldCellData> closed =
                new Dictionary<Vector3, NPCWorldView.worldCellData>();                                             //For quickly looking up closed nodes
            SortedList<float, NPCWorldView.worldCellData> open =
                new SortedList<float, NPCWorldView.worldCellData>(new NPCWorldView.DuplicateKeyComparer<float>()); //For quickly finding best node to visit                                

            this._path.Clear(); //Clear any old paths
            this._goal = goal.pos;

            var current = startCell;

            NPCWorldView.resetAStarData();
            current.g = 0;
            open.Add(current.f, current); //Push the start node
            while (open.Count > 0) {
                do { //Outdated cells might still be in the list
                    current = open.Values[0];
                    open.RemoveAt(0);
                } while (closed.ContainsKey(current.pos) && open.Count > 0);

                if (current.pos == goal.pos) {    //Victor  
                    NPCWorldView.worldCellData tmp = goal;
                    while (tmp.parent != null) {
                        this._path.Push(tmp);
                        tmp = tmp.parent;
                    }
                    this._path.Push(tmp);
                    return;
                }
                if (open.Count == 0 && closed.ContainsKey(current.pos))
                    break;

                //Close current tile
                closed.Add(current.pos, current);
                foreach (var cell in current.neighbours) {
                    var waterCell = NPCWorldView.getCell(false, cell.x, cell.y);
                    if (!closed.ContainsKey(cell.pos) && !cell.blocked && waterCell.blocked) {
                        float g = current.g + Vector3.Distance(cell.pos, current.pos);
                        if (g < cell.g) { //New and better G value?
                            cell.h = Vector3.Distance(cell.pos, goal.pos);
                            cell.g = g;
                            cell.parent = current;
                            open.Add(cell.f, cell);
                        }
                    }
                }
            }
        }
    }

    //==============Flee Danger state==================================================

    private class FleeDanger : AvoidObstacle {
        public FleeDanger(NPCBrain x) : base(x) {
            x._speed = 4;
        }

        override public void update() {
            var npc = this._brain._npc;
            Vector3 dir = fleeDir();
            if (detectObstacle()) {                
                AStar(npc.getCell(), findTargetCell(dir));
            }
            if (this._path.Count > 1) {               
                this._path.Pop();
                dir = (this._path.Peek().pos - npc.getPos()).normalized;
                dir.y = 0;
                dir.Normalize();
            } 
            this._brain.sendInstuction(dir);
            if (dir == Vector3.zero) {
                this._brain._speed = 1;
                this._brain._state.Pop();
            }
        }

        private Vector3 fleeDir() {
            Vector3 fleeDir = Vector3.zero;
            fleeDir += playersFleeDir();
            fleeDir += FireWallFleeDir();
            return fleeDir.normalized;
        }

        private Vector3 playersFleeDir() {
            var npc = this._brain._npc;
            var players = NPCWorldView.getPlayers();
            Vector3 fleeDir = Vector3.zero;
            foreach (var player in players.Values) {
                if (canSeePlayer(player)) {
                    fleeDir += (npc.getPos() - player.getPos()).normalized;
                }
            }
            fleeDir.y = 0;
            return fleeDir.normalized;
        }

        private Vector3 FireWallFleeDir() {
            var npc = this._brain._npc;
            float dist = Vector3.Distance(npc.getPos(), NPCWorldView.FireWall.pos);
            float viewDist = 20;
            if ((NPCWorldView.FireWall.radius - dist) < viewDist) {
                Vector3 fleeDir = NPCWorldView.FireWall.pos - npc.getPos();
                fleeDir.y = 0;
                return fleeDir.normalized;
            } else
                return Vector3.zero;
        }       

        private bool canSeePlayer(NPCWorldView.GameCharacter player) {
            var npc = this._brain._npc;
            if (Vector3.Distance(npc.getPos(), player.getPos()) < 20) 
                return true;
            return false;
        }
    }

}
