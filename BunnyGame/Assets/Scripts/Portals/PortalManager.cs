using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PortalManager : NetworkBehaviour {
    public GameObject portal;

    private int[] portalCounts = new int[]{ 4, 1, 1 };
    HashSet<WorldGrid.Cell> takenCells = new HashSet<WorldGrid.Cell>();

    // Use this for initialization
    void Start () {
        if (this.isServer)
            StartCoroutine(init());        
	}

    private Vector3 getPortalPos(int y) {
        WorldGrid.Cell cell;
        WorldGrid.Cell cellPlus;
        do {
            cell = WorldData.worldGrid.getRandomCell(false, y);

            if (y + 1 < WorldData.worldGrid.yOffsets.Length)
                cellPlus = WorldData.worldGrid.getCell(cell.x, y + 1, cell.z);
            else {
                cellPlus = new WorldGrid.Cell();
                cellPlus.blocked = true;
            }
        } while (takenCells.Contains(cell));

        int layermask = (1 << 19);
        Ray ray = new Ray(cell.pos + Vector3.up * 5, Vector3.down);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 10, layermask);
        return hit.point;
    }

    [ClientRpc]
    private void RpcSpawnPortals(Vector3 one, Vector3 two) {
        Portal p1 = Instantiate(portal, one, Quaternion.identity).GetComponent<Portal>();
        Portal p2 = Instantiate(portal, two, Quaternion.identity).GetComponent<Portal>();
        p1.setTarget(p2);
        p2.setTarget(p1);
    }

    private IEnumerator init() {
        while (!WorldData.ready) yield return 0;

        while (!GameInfo.playersReady) //When this is true, all clients are connected and in the game scene
            yield return 0;

        //Level 1 to 2 portals
        for (int level = 0; level < portalCounts.Length; level++) {
            for (int i = 0; i < portalCounts[level]; i++) {
                RpcSpawnPortals(getPortalPos(level + 1), getPortalPos(level + 2));
            }
        }
    }
}
