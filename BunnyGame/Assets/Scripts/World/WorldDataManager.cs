using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldDataManager : MonoBehaviour {

    public bool debugRender;        //Should WorldData be rendered in the editor?
    public int debugRenderLevel;
    public RectTransform progressBar;
    public GameObject progressUI;

    private IslandData _islandData;
    // Use this for initialization
    void Start() {
        StartCoroutine(init());
    }

    private IEnumerator init() {
        while (this._islandData == null) {
            this._islandData = Object.FindObjectOfType<IslandData>();
            yield return 0;
        }
        
        WorldData.init(_islandData);
        if (!WorldData.worldGrid.readFromFile())
            StartCoroutine(calcWorldData());
        else
            WorldData.ready = true;

        }

    private IEnumerator calcWorldData() { //Really wish unity let us thread stuff, but courutines will have to do.
        Time.timeScale = 0; //Freeze time
        progressUI.SetActive(true);
        float prog = 0;

        float time = Time.realtimeSinceStartup;
        Debug.Log("WorldDataManager: Setting up NPCWorldView by detecting obstacles!");
        //NPCs were having a tough time with this cell
        foreach (Transform level in this._islandData.avoidPoints) {
            foreach (Transform avoidPoint in level) {
                var difficultCell = WorldData.worldGrid.getCell(avoidPoint.transform.position); ;
                foreach (var cell in difficultCell.neighbours)
                    cell.blocked = true;
            }
        }

        StartCoroutine(findObstacles((x) => prog = x));
        while (prog < 1) {            
            progressBar.sizeDelta = new Vector2(prog * 1000, 100);
            yield return 0;
        }
        Debug.Log("WorldDataManager: Finished detecting obstacles for NPCWorldView, time elapsed: " + (Time.realtimeSinceStartup - time));

        progressUI.SetActive(false);

        Time.timeScale = 1; //resume game

        //WorldData.worldGrid.writeToFile();
        WorldData.ready = true;
    }

    //Quick way of blocking out water cells in land plane, also overextends to keep NPCs out of water
    //private IEnumerator blockWaterInLand(System.Action<float> progress) {
    //    int Iter = 0;
    //    int totalIter = _cellCount * _cellCount;
    //    int yieldRate = 2 * _cellCount;
    //    //LOOP DI LOOP
    //    for (int y = 0; y < _cellCount; y++) {
    //        for (int x = 0; x < _cellCount; x++) {
    //            for (int i = -1; i < 2; i++) { //Over extend
    //                for (int j = -1; j < 2; j++) {
    //                    if (!NPCWorldView.getCell(true, x + j, y + i).blocked)
    //                        NPCWorldView.getCell(true, x + j, y + i).blocked = !NPCWorldView.getCell(false, x, y).blocked;
    //                }
    //            }
    //            Iter++;
    //            if (Iter % yieldRate == 0) {
    //                progress((float)Iter / (float)totalIter);
    //                yield return 0;
    //            }
    //        }
    //    }
    //    progress(1);
    //}
    //Finds obstacles in every cell of NPCWorldView, and marks them as blocked
    //Areas that are closed off by blocked cells will also be blocked
    private IEnumerator findObstacles(System.Action<float> progress) {
        WorldGrid grid = WorldData.worldGrid;
        int Iter = 0;
        int totalIter = grid.yOffsets.Length * grid.cellCount * grid.cellCount;
        int yieldRate = grid.cellCount;

        for (int y = 0; y < grid.yOffsets.Length; y++) {
            for (int z = 0; z < grid.cellCount; z++) {
                for (int x = 0; x < grid.cellCount; x++) {
                    var cell = grid.getCell(x, y, z);
                    if (!cell.blocked)
                        cell.blocked = obstacleInCell(cell);
                    Iter++;
                    if (Iter % yieldRate == 0) {
                        progress((float)Iter / (float)totalIter);
                        yield return 0;
                    }
                }
            }
        }

        //WorldGrid.Cell[] targets;
        ////Generate targets depending on plane type
        //if (land) {
        //    targets = new WorldGrid.Cell[] { NPCWorldView.getCell(land, landPos.position) };
        //} else {
        //    targets = new WorldGrid.Cell[waterBodies.Length];
        //    for (int i = 0; i < waterBodies.Length; i++) {
        //        targets[i] = NPCWorldView.getCell(land, waterBodies[i].position);
        //    }
        //}

        //bool lastCellBlocked = false;
        //for (int y = 0; y < _cellCount; y++) {
        //    for (int x = 0; x < _cellCount; x++) {
        //        var cell = NPCWorldView.getCell(land, x, y);
        //        if (!cell.blocked && lastCellBlocked)
        //            this.fillAreaIfBlocked(land, cell, targets);
        //        lastCellBlocked = cell.blocked;
        //        Iter++;
        //        if (Iter % yieldRate == 0) {
        //            progress((float)Iter / (float)totalIter);
        //            yield return 0;
        //        }
        //    }
        //}
        progress(1);
    }

    //Determines if there are any colliders inside a cell
    bool obstacleInCell(WorldGrid.Cell cell) {
        float modifier = 1.0f;
        Vector3 halfExtents = new Vector3(WorldData.cellSize / 2, WorldData.cellSize / 2, WorldData.cellSize / 2) * modifier;
        int layer = (1 << 19);
        return Physics.CheckBox(cell.pos, halfExtents, Quaternion.identity, layer);
    }

    //This is basically a*, if it cant find a path from startPos to any target node, then all the nodes in
    //  the closed list are blocked nodes.
    void fillAreaIfBlocked(bool land, WorldGrid.Cell startCell, WorldGrid.Cell[] targets) {
        Dictionary<Vector3, WorldGrid.Cell> closed = null;
        foreach (var target in targets) {
            SortedList<float, WorldGrid.Cell> open =
                new SortedList<float, WorldGrid.Cell>(new WorldGrid.DuplicateKeyComparer<float>()); //For quickly finding best node to visit
            closed = new Dictionary<Vector3, WorldGrid.Cell>();                                             //For quickly looking up closed nodes

            var goal = target;
            var current = startCell;

            NPCWorldView.resetAStarData();
            current.g = 0;
            open.Add(current.f, current); //Push the start node
            while (open.Count > 0) {
                do { //Outdated cells might still be in the list
                    current = open.Values[0];
                    open.RemoveAt(0);
                } while (closed.ContainsKey(current.pos) && open.Count > 0);

                if (current.pos == goal.pos)    //Victor                                               
                    return;                     // The node is connected to the target, just return
                if (open.Count == 0 && closed.ContainsKey(current.pos))
                    break;
                //Close current tile
                closed.Add(current.pos, current);

                for (int i = 0; i < current.plusNeighbours.Count; i++) {
                    var cell = current.plusNeighbours[i];
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
        }
        // If the search made it this far, that means the node is blocked in
        for (int i = 0; i < closed.Count; i++)
            closed.ElementAt(i).Value.blocked = true;
    }

    private void OnDestroy() {
        WorldData.clear();
    }

    void OnDrawGizmos() {
        if (debugRender)
            drawGizmo();
    }

    void drawGizmo() {
        WorldGrid grid = WorldData.worldGrid;
        Gizmos.color = Color.green;
        for (int z = 0; z < grid.cellCount; z++) {
            for (int x = 0; x < grid.cellCount; x++) {
                if (!grid.getCell(x, debugRenderLevel, z).blocked)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.red;
                Vector3 cubeCenter = new Vector3(
                    x * grid.cellSize + grid.cellSize / 2, 
                    grid.yOffsets[debugRenderLevel], 
                    z * grid.cellSize + grid.cellSize / 2
                );
                cubeCenter += new Vector3(grid.xzOffsets.x, 0, grid.xzOffsets.y);
                Gizmos.DrawCube(cubeCenter, new Vector3(grid.cellSize, 0, grid.cellSize));
            }
        }
    }
}
