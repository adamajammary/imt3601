using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyCommands : NetworkBehaviour {
    public GameObject bunnyPoop;

    [Command]
    public void Cmdshootpoop(Vector3 dir) {
        GameObject poop = Instantiate(bunnyPoop);

        Vector3 pos = transform.position;
        dir.y += 0.6f;
        pos += dir * 2.0f;
        poop.GetComponent<BunnyPoop>().shoot(dir, pos);

        NetworkServer.Spawn(poop);
    }
}
