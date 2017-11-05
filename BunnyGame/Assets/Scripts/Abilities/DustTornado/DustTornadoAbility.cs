using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustTornadoAbility : SpecialAbility {

    private AbilityNetwork _abilityNetwork;

    // Use this for initialization
    public void init() {
        this._abilityNetwork = GetComponent<AbilityNetwork>();
        base.init("Textures/AbilityIcons/DustTornado");
        base.abilityName = "DustTornado";
        base._cooldownTimeInSeconds = 20f;
    }

    public override IEnumerator useAbility() {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit);

        bool isAttacking = false;
        if (GetComponent<BirdController>())
            isAttacking = GetComponent<BirdController>().getPecking();

        if (this._cooldown == 0 && !isAttacking && hit.distance < 10) {
            StartCoroutine(this.doCoolDown());
            if(GetComponent<BirdController>())
                StartCoroutine(GetComponent<BirdController>().flapLikeCrazy());
            Vector3 shootDir = Camera.main.transform.forward;
            shootDir.y = 0;
            shootDir.Normalize();
            this._abilityNetwork.CmdDustTornado(hit.point, shootDir, this.gameObject);
        }
        yield return 0;
    }
}
