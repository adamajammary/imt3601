using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour {
    public bool debugRender;

    float _cellSize;
    int _cellCount;
    Vector3 _offset;
    // Use this for initialization
    void Start() {
        _cellSize = NPCWorldView.cellSize;
        _cellCount = NPCWorldView.cellCount;
        _offset = NPCWorldView.offset;
        //findObstacles();
        findObstacles();
    }

	// Update is called once per frame
	void Update () {
       
	}

    void findObstacles() {
        float time = Time.realtimeSinceStartup;
        Debug.Log("NPCManager: Setting up NPCWorldView by detecting obstacles!");
        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                var cell = NPCWorldView.getCell(x, y);
                cell.blocked = obstacleInCell(cell);
            }
        }
        bool lastCellBlocked = false;
        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                var cell = NPCWorldView.getCell(x, y);
                if (!cell.blocked && lastCellBlocked) 
                    fillAreaIfBlocked(cell);
                lastCellBlocked = cell.blocked;
            }
        }
        Debug.Log("NPCManager: Finished detecting obstacles for NPCWorldView, time elapsed: " + (Time.realtimeSinceStartup - time));
    }

    bool obstacleInCell(NPCWorldView.worldCellData cell) {
        bool obstacle = false;
        float modifier = 1.0f;
        Vector3 halfExtents = new Vector3(_cellSize / 2, _cellSize / 2, 0) * modifier;
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        foreach (Vector3 dir in directions) {
            Vector3 rayStart = cell.pos - dir * _cellSize / 2;
            obstacle = Physics.BoxCast(rayStart, halfExtents, dir, Quaternion.identity, _cellSize * modifier);
            if (obstacle) return obstacle;
        }       
        return obstacle;
    }

    //This is basically a*, if it cant find a path from startPos to the corner node, then all the nodes in
    //  the closed list are blocked nodes.
    void fillAreaIfBlocked(NPCWorldView.worldCellData startCell) {
        SortedList<float, NPCWorldView.worldCellData> open = 
            new SortedList<float, NPCWorldView.worldCellData>(new NPCWorldView.DuplicateKeyComparer<float>()); //For quickly finding best node to visit
        Dictionary<Vector3, NPCWorldView.worldCellData> closed = 
            new Dictionary<Vector3, NPCWorldView.worldCellData>();                                             //For quickly looking up closed nodes

        var goal = NPCWorldView.getCell(0, 0);
        var current = startCell;

        NPCWorldView.resetAStarData();
        current.g = 0;
        open.Add(current.f, current); //Push the start node
        while (open.Count > 0) {
            do { //Outdated cells might still be in the list
                current = open.Values[0];
                open.RemoveAt(0);
            } while (closed.ContainsKey(current.pos) && open.Count > 0);

            if (current.pos == goal.pos) {  //Victor
                // The node is connected to the rest of the map, just return
                return;
            }
            //Close current tile
            closed.Add(current.pos, current);

            for (int i = 0; i < current.neighbours.Count; i++) {
                var cell = current.neighbours[i];
                if (!closed.ContainsKey(cell.pos) && !cell.blocked) {
                    float g = current.g + 1;
                    if (g < cell.g) { //New and better G value?
                        cell.h = Mathf.Abs(goal.x - cell.x) + Mathf.Abs(goal.y - cell.y);
                        cell.g = g;
                        open.Add(cell.f, cell);
                    }
                }
            }
        }
        // If the search made it this far, that means the node is blocked in
        for (int i = 0; i < closed.Count; i++) {
            NPCWorldView.worldCellData cell = closed.ElementAt(i).Value;
            cell.blocked = true;
        }
    }

    void OnDrawGizmos() {
        if (debugRender) {
            for (int y = 0; y < _cellCount; y++) {
                for (int x = 0; x < _cellCount; x++) {

                    if (!NPCWorldView.getCell(x, y).blocked)
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.red;
                    Vector3 cubeCenter = new Vector3(x * _cellSize + _cellSize / 2, 0, y * _cellSize + _cellSize / 2) + _offset;
                    Gizmos.DrawCube(cubeCenter, new Vector3(_cellSize, 0, _cellSize));
                }
            }
        }
    }
}
