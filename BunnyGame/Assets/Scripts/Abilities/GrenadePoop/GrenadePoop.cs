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
        }
        yield return 0;
    }

}
