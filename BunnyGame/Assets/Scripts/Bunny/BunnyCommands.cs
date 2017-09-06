using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyCommands : NetworkBehaviour {
    public GameObject bunnyPoop;

    [Command]
    public void Cmdshootpoop() {
        GameObject poop = Instantiate(bunnyPoop);
        Vector3 pos = transform.position;
        Vector3 dir = transform.forward;

        dir.y += 0.2f;
        pos += transform.forward * 2.0f;
        poop.GetComponent<BunnyPoop>().shoot(dir, pos);

        NetworkServer.Spawn(poop);
    }
}
