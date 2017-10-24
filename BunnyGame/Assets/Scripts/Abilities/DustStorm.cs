using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustStorm : SpecialAbility {

    // Use this for initialization
    public void init() {
        
        base.init("Textures/AbilityIcons/bombIcon");
        base.abilityName = "Poop Grenade";
        base._cooldownTimeInSeconds = 15f;
    }

    public override IEnumerator useAbility() {
        if (this._cooldown == 0) {
            StartCoroutine(this.doCoolDown());
            
        }
        yield return 0;
    }
}
