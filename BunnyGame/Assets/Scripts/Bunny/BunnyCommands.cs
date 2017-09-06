using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyCommands : NetworkBehaviour {
    public GameObject bunnyPoop;

    [Command]
    public void Cmdshootpoop(Vector3 dir) {
        GameObject poop = Instantiate(bunnyPoop);
        if (!this.isLocalPlayer) 
            poop.layer = 9;

        Debug.Log("Bullet and player mask: ");
        Debug.Log(poop.layer);
        Debug.Log(gameObject.layer);

        Vector3 pos = transform.position;
        dir.y += 0.6f;
        poop.GetComponent<BunnyPoop>().shoot(dir, pos);

        NetworkServer.Spawn(poop);
    }
}
