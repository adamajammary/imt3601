using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pecker : MonoBehaviour {
    public GameObject owner;
    public Transform body;

    private int damage = 10;

    void Update() {
        transform.position = body.position;
    }

    private void OnTriggerEnter(Collider other) {
        if ((other.gameObject.tag == "npc")) {
            NPC npc = other.GetComponent<NPC>();
            npc.kill(owner, owner.GetComponent<PlayerInformation>().ConnectionID);
        }
    }

    public int GetDamage() {
        return damage;
    }

}
