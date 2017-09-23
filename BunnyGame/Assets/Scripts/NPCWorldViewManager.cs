using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWorldViewManager : MonoBehaviour {
    float _cellSize;
    int _cellCount;
    Vector3 _offset;
    // Use this for initialization
    void Start() {
        _cellSize = NPCWorldView.cellWorldSize;
        _cellCount = NPCWorldView.cellCount;
        _offset = new Vector3(-(_cellCount * _cellSize / 2.0f + _cellSize / 2.0f),
                              200,
                              -(_cellCount * _cellSize / 2.0f + _cellSize / 2.0f));
    }

	// Update is called once per frame
	void Update () {
        findObstacles();
	}

    void findObstacles() {
        for (int y = 0; y < _cellCount; y++) {
            for (int x = 0; x < _cellCount; x++) {
                NPCWorldView.worldCellData cell = new NPCWorldView.worldCellData();
                cell.blocked = obstacleInCell(x, y);
                NPCWorldView.setCell(x, y, cell);
            }
        }
    }

    bool obstacleInCell(int x, int y) {
        bool obstacle = false;
        NPCWorldView.worldCellData cell = new NPCWorldView.worldCellData();
        Vector3 cubeCenter = new Vector3(x * _cellSize + _cellSize / 2, 0, y * _cellSize + _cellSize / 2) + _offset;
        Vector3 rayStart = cubeCenter - Vector3.forward * _cellSize / 2;
        Vector3 halfExtents = new Vector3(_cellSize / 2, _cellSize / 2, 0);
        obstacle = Physics.BoxCast(rayStart, halfExtents, Vector3.forward, Quaternion.identity, _cellSize);
        if (obstacle) return obstacle;
        //Gotta cast a ray from both sides, because checking collisions at the start of the ray behaves annoyingly
        rayStart = cubeCenter - Vector3.back * _cellSize / 2;
        obstacle = Physics.BoxCast(rayStart, halfExtents, Vector3.back, Quaternion.identity, _cellSize);
        return obstacle;
    }

    void OnDrawGizmos() {
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
