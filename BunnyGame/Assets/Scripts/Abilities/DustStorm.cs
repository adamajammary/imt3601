using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustStorm : SpecialAbility {
    AbilityNetwork _an;

    // Use this for initialization
    public void init() {
        this._an = GetComponent<AbilityNetwork>();
        base.init("Textures/AbilityIcons/DustStorm");
        base.abilityName = "DustStorm";
        base._cooldownTimeInSeconds = 15f;
    }

    public override IEnumerator useAbility() {
        if (this._cooldown == 0) {
            StartCoroutine(this.doCoolDown());
            this._an.CmdDustStorm(transform.position, GetComponent<PlayerInformation>().ConnectionID);
        }
        yield return 0;
    }
}
