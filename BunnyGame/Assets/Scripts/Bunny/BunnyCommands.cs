using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyCommands : NetworkBehaviour {
    [Command]
    public void Cmdshootpoop() {
        BunnyPoop poop = Factory.getPoop().GetComponent<BunnyPoop>();
        Vector3 pos = transform.position;
        Vector3 dir = transform.forward;

        dir.y += 0.2f;
        pos += transform.forward * 1;
        poop.shoot(dir, pos);
    }
}
