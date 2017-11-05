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
        RaycastHit hit;

        bool isAttacking = false;
        if (GetComponent<BirdController>())
            isAttacking = GetComponent<BirdController>().getPecking();

        Physics.Raycast(transform.position, Vector3.down, out hit);
        if (this._cooldown == 0 && !isAttacking && hit.distance < 10) {
            StartCoroutine(this.doCoolDown());
            StartCoroutine(GetComponent<BirdController>().flapLikeCrazy());
            this._an.CmdDustStorm(hit.point, GetComponent<PlayerInformation>().ConnectionID);
        }
        yield return 0;
    }
}
