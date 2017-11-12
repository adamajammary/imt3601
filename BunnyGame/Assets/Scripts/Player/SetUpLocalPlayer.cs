using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

public class SetUpLocalPlayer : NetworkBehaviour {

    // Use this for initialization
    void Start () {
        if (this.isLocalPlayer) {
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();

            // Setting bunny offset target if BunnyController exists
            BunnyController bunnyContr = GetComponent<BunnyController>();

            if (camera != null) {
                if (!bunnyContr)
                    camera.SetTarget(this.transform);
                else
                    camera.SetTarget(this.transform.GetChild(2));
            }
            this.tag = "Player";
            transform.GetChild(0).gameObject.SetActive(true);
            StartCoroutine(SpawnPlayer());
        } else
            this.tag = "Enemy";        
    }

    private IEnumerator SpawnPlayer() {
        while (!WorldData.ready) yield return 0;        

        WorldGrid.Cell cell;
        do { //Find a random position for the player
            int x = Random.Range(0, WorldData.cellCount);
            int z = Random.Range(0, WorldData.cellCount);
            cell = WorldData.worldGrid.getCell(x, 1, z);
        } while (cell.blocked);
        transform.position = cell.pos;
    }
}
