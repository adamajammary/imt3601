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
                Vector3 cubeCenter = new Vector3(x * _cellSize + _cellSize / 2, 0, y * _cellSize - _cellSize) + _offset;
                Vector3 halfExtents = new Vector3(_cellSize / 2, _cellSize / 2, _cellSize / 2);
                cell.blocked = Physics.BoxCast(cubeCenter, halfExtents, Vector3.forward, Quaternion.identity, _cellSize);
                //if (cell.blocked)
                //    Debug.Log(string.Format("Blocked cell at pos {0}, hit by ray starting at: {1}", cubeCenter, cubeCenter - Vector3.forward * _cellSize / 2));
                NPCWorldView.setCell(x, y, cell);
            }
        }
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
