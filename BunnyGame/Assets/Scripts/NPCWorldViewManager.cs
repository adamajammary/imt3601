using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWorldViewManager : MonoBehaviour {
    public bool debugRender;

    float _cellSize;
    int _cellCount;
    Vector3 _offset;
    // Use this for initialization
    void Start() {
        _cellSize = NPCWorldView.cellWorldSize;
        _cellCount = NPCWorldView.cellCount;
        _offset = new Vector3(-(_cellCount * _cellSize / 2.0f + _cellSize / 2.0f),
                              -15,
                              -(_cellCount * _cellSize / 2.0f + _cellSize / 2.0f));
        findObstacles();
    }

	// Update is called once per frame
	void Update () {
       
	}

    void findObstacles() {
        float time = Time.realtimeSinceStartup;
        Debug.Log("NPCWorldViewManager: Setting up NPCWorldView by detecting obstacles!");
        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                NPCWorldView.worldCellData cell = new NPCWorldView.worldCellData();
                cell.blocked = obstacleInCell(x, y);
                NPCWorldView.setCell(x, y, cell);
            }
        }
        Debug.Log("NPCWorldViewManager: Finished detecting obstacles for NPCWorldView, time elapsed: " + (Time.realtimeSinceStartup - time));
    }

    bool obstacleInCell(int x, int y) {
        bool obstacle = false;
        float modifier = 1.0f;
        NPCWorldView.worldCellData cell = new NPCWorldView.worldCellData();
        Vector3 cubeCenter = new Vector3(x * _cellSize + _cellSize / 2, 0, y * _cellSize + _cellSize / 2) + _offset;
        Vector3 halfExtents = new Vector3(_cellSize / 2, _cellSize / 2, 0) * modifier;

        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

        foreach (Vector3 dir in directions) {
            Vector3 rayStart = cubeCenter - dir * _cellSize / 2;
            obstacle = Physics.BoxCast(rayStart, halfExtents, dir, Quaternion.identity, _cellSize * modifier);
            if (obstacle) return obstacle;
        }       
        return obstacle;
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
