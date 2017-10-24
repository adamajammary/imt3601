using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustTornadoAbility : SpecialAbility {

    AbilityNetwork _an;
    BirdController _bc;

    // Use this for initialization
    public void init() {
        this._an = GetComponent<AbilityNetwork>();
        this._bc = GetComponent<BirdController>();
        base.init("Textures/AbilityIcons/DustTornado");
        base.abilityName = "DustTornado";
        base._cooldownTimeInSeconds = 15f;
    }

    public override IEnumerator useAbility() {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit);
        if (this._cooldown == 0 && !this._bc.getPecking() && hit.distance < 10) {
            StartCoroutine(this.doCoolDown());
            Vector3 shootDir = Camera.main.transform.forward;
            shootDir.y = 0;
            shootDir.Normalize();
            this._an.CmdDustTornado(hit.point, shootDir);
        }
        yield return 0;
    }
}
