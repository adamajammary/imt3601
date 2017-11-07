using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldDataManager : MonoBehaviour {

    public bool debugRender;        //Should WorldData be rendered in the editor?
    public int debugRenderLevel;

    private Transform pointsOfInterest;
    public Transform[] difficultSpots;
    public Transform landPos;
    public RectTransform progressBar;
    public GameObject progressUI;
    // Use this for initialization
    void Start() {
        WorldData.init();

        if (!WorldData.worldGrid.readFromFile())
            StartCoroutine(calcNPCWorld());
        else
            WorldData.ready = true;
    }

    private IEnumerator calcNPCWorld() { //Really wish unity let us thread stuff, but courutines will have to do.
        Time.timeScale = 0; //Freeze time

        progressUI = GameObject.Find("NPCWorldViewProgress");
        foreach (Transform child in progressUI.transform)
            child.gameObject.SetActive(true);
        progressBar = GameObject.Find("ProgressBar").GetComponent<RectTransform>();

        float waterProg = 0;
        float blockWaterProg = 0;
        float landProg = 0;


        float time = Time.realtimeSinceStartup;
        Debug.Log("NPCManager: Setting up NPCWorldView by detecting obstacles!");
        //NPCs were having a tough time with this cell
        foreach (var toughSpot in difficultSpots) {
            var difficultCell = NPCWorldView.getCell(true, toughSpot.transform.position); ;
            foreach (var cell in difficultCell.neighbours)
                cell.blocked = true;
        }

        StartCoroutine(findObstacles(false, (x) => waterProg = x));
        while (landProg < 1 || waterProg < 1 || blockWaterProg < 1) {
            if (waterProg == 1 && blockWaterProg == 0) StartCoroutine(blockWaterInLand((x) => blockWaterProg = x));
            if (blockWaterProg == 1 && landProg == 0) StartCoroutine(findObstacles(true, (x) => landProg = x));
            progressBar.sizeDelta = new Vector2((landProg + waterProg + blockWaterProg) * 333, 100);
            yield return 0;
        }
        Debug.Log("NPCManager: Finished detecting obstacles for NPCWorldView, time elapsed: " + (Time.realtimeSinceStartup - time));

        progressUI.SetActive(false);

        Time.timeScale = 1; //resume game

        NPCWorldView.writeToFile();
        NPCWorldView.ready = true;
    }

    //Quick way of blocking out water cells in land plane, also overextends to keep NPCs out of water
    private IEnumerator blockWaterInLand(System.Action<float> progress) {
        int Iter = 0;
        int totalIter = _cellCount * _cellCount;
        int yieldRate = 2 * _cellCount;
        //LOOP DI LOOP
        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                for (int i = -1; i < 2; i++) { //Over extend
                    for (int j = -1; j < 2; j++) {
                        if (!NPCWorldView.getCell(true, x + j, y + i).blocked)
                            NPCWorldView.getCell(true, x + j, y + i).blocked = !NPCWorldView.getCell(false, x, y).blocked;
                    }
                }
                Iter++;
                if (Iter % yieldRate == 0) {
                    progress((float)Iter / (float)totalIter);
                    yield return 0;
                }
            }
        }
        progress(1);
    }
    //Finds obstacles in every cell of NPCWorldView, and marks them as blocked
    //Areas that are closed off by blocked cells will also be blocked
    private IEnumerator findObstacles(bool land, System.Action<float> progress) {
        int Iter = 0;
        int totalIter = 2 * _cellCount * _cellCount;
        int yieldRate = _cellCount;

        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                var cell = NPCWorldView.getCell(land, x, y);
                if (!cell.blocked)
                    cell.blocked = obstacleInCell(cell);
                Iter++;
                if (Iter % yieldRate == 0) {
                    progress((float)Iter / (float)totalIter);
                    yield return 0;
                }
            }
        }

        NPCWorldView.worldCellData[] targets;
        //Generate targets depending on plane type
        if (land) {
            targets = new NPCWorldView.worldCellData[] { NPCWorldView.getCell(land, landPos.position) };
        } else {
            targets = new NPCWorldView.worldCellData[waterBodies.Length];
            for (int i = 0; i < waterBodies.Length; i++) {
                targets[i] = NPCWorldView.getCell(land, waterBodies[i].position);
            }
        }

        bool lastCellBlocked = false;
        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                var cell = NPCWorldView.getCell(land, x, y);
                if (!cell.blocked && lastCellBlocked)
                    this.fillAreaIfBlocked(land, cell, targets);
                lastCellBlocked = cell.blocked;
                Iter++;
                if (Iter % yieldRate == 0) {
                    progress((float)Iter / (float)totalIter);
                    yield return 0;
                }
            }
        }
        progress(1);
    }

    //Determines if there are any colliders inside a cell
    bool obstacleInCell(NPCWorldView.worldCellData cell) {
        float modifier = 1.0f;
        Vector3 halfExtents = new Vector3(_cellSize / 2, _cellSize / 2, _cellSize / 2) * modifier;
        return Physics.CheckBox(cell.pos, halfExtents, Quaternion.identity, 1);
    }

    //This is basically a*, if it cant find a path from startPos to any target node, then all the nodes in
    //  the closed list are blocked nodes.
    void fillAreaIfBlocked(bool land, NPCWorldView.worldCellData startCell, NPCWorldView.worldCellData[] targets) {
        Dictionary<Vector3, NPCWorldView.worldCellData> closed = null;
        foreach (var target in targets) {
            SortedList<float, NPCWorldView.worldCellData> open =
                new SortedList<float, NPCWorldView.worldCellData>(new NPCWorldView.DuplicateKeyComparer<float>()); //For quickly finding best node to visit
            closed = new Dictionary<Vector3, NPCWorldView.worldCellData>();                                             //For quickly looking up closed nodes

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
        NPCWorldView.clear();
    }

    void OnDrawGizmos() {
        if (debugRenderLand) {
            drawGizmo(true);
        }
        if (debugRenderWater) {
            drawGizmo(false);
            Vector3 offset = NPCWorldView.waterOffset;
            foreach (var waterbody in waterBodies) {
                int[] fuck = NPCWorldView.convertWorld2Cell(waterbody.position);
                Gizmos.color = Color.cyan;
                Vector3 cubeCenter = new Vector3(fuck[0] * _cellSize + _cellSize / 2, 0, fuck[1] * _cellSize + _cellSize / 2) + offset;
                Gizmos.DrawCube(cubeCenter, new Vector3(_cellSize, 0, _cellSize));
            }
        }
    }

    void drawGizmo() {
        WorldGrid grid = WorldData.worldGrid;
        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                if (!NPCWorldView.getCell(land, x, y).blocked)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.red;
                Vector3 cubeCenter = new Vector3(x * _cellSize + _cellSize / 2, 0, y * _cellSize + _cellSize / 2) + offset;
                Gizmos.DrawCube(cubeCenter, new Vector3(_cellSize, 0, _cellSize));
            }
        }
    }
}
