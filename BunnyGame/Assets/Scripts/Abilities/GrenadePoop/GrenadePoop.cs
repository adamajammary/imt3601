using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePoop : SpecialAbility {
    AbilityNetwork networkAbility;
    CharacterController controller;
    BunnyController bc;

    // Use this for initialization
    public void init() {
        networkAbility = GetComponent<AbilityNetwork>();
        controller = GetComponent<CharacterController>();
        bc = GetComponent<BunnyController>(); //ID should be in PlayerController IMO
        base.init("Textures/AbilityIcons/bombIcon");
        base.abilityName = "Poop Grenade";
        base._cooldownTimeInSeconds = 0.1f;
    }

    override public IEnumerator useAbility() {
        if (this._cooldown == 0) {
            StartCoroutine(this.doCoolDown());
            networkAbility.CmdPoopGrenade(Camera.main.transform.forward, controller.velocity, bc.ConnectionID);
            StartCoroutine(spin());
        }
        yield return 0;
    }

    private IEnumerator spin() {
        bool dir = (Random.Range(0.0f, 1.0f) > 0.5);
        if (dir) {
            for (float rad = 0; rad <= 360; rad += Time.deltaTime * 360) {
                transform.rotation = Quaternion.AngleAxis(rad, Camera.main.transform.forward) * transform.rotation;
                yield return 0;
            }
        } else {
            for (float rad = 360; rad >= 0; rad -= Time.deltaTime * 360) {
                transform.rotation = Quaternion.AngleAxis(rad, Camera.main.transform.forward) * transform.rotation;
                yield return 0;
            }
        }
    }

}
