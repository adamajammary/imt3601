using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour {
    public GameObject portal;

    private int[] portalCounts = new int[]{ 4, 1, 1 };
    HashSet<WorldGrid.Cell> takenCells = new HashSet<WorldGrid.Cell>();

    // Use this for initialization
    void Start () {
        StartCoroutine(init());        
	}

    private Vector3 getPortalPos(int y) {
        WorldGrid.Cell cell;
        WorldGrid.Cell cellPlus;
        do {
            int x = Random.Range(0, WorldData.cellCount);
            int z = Random.Range(0, WorldData.cellCount);
            cell = WorldData.worldGrid.getCell(x, y, z);

            if (y + 1 < WorldData.worldGrid.yOffsets.Length)
                cellPlus = WorldData.worldGrid.getCell(x, y + 1, z);
            else {
                cellPlus = new WorldGrid.Cell();
                cellPlus.blocked = true;
            }

        } while (takenCells.Contains(cell) || cell.blocked || !cellPlus.blocked);

        int layermask = (1 << 19);
        Ray ray = new Ray(cell.pos + Vector3.up * 5, Vector3.down);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 10, layermask);
        return hit.point;
    }

    private IEnumerator init() {
        while (!WorldData.ready) yield return 0;

        //Level 1 to 2 portals
        for (int level = 0; level < portalCounts.Length; level++) {
            for (int i = 0; i < portalCounts[level]; i++) {
                Portal p1 = Instantiate(portal, getPortalPos(level + 1), Quaternion.identity).GetComponent<Portal>();
                Portal p2 = Instantiate(portal, getPortalPos(level + 2), Quaternion.identity).GetComponent<Portal>();
                p1.setTarget(p2);
                p2.setTarget(p1);
            }
        }
    }
}
