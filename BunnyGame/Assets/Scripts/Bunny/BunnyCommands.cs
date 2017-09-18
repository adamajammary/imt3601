﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyCommands : NetworkBehaviour {
    public GameObject bunnyPoop;

    void Start() {
        bunnyPoop = Resources.Load<GameObject>("Prefabs/poop");
    }

    [Command]
    public void Cmdshootpoop(Vector3 dir, Vector3 startVel) {
        GameObject poop = Instantiate(bunnyPoop);

        Vector3 pos = transform.position;
        //dir.y += 0.6f;                      
        pos += dir * 4.0f; 
        poop.GetComponent<BunnyPoop>().shoot(dir, pos, startVel);

        NetworkServer.Spawn(poop);
    }
}
