using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stomp : SpecialAbility
{
    private GameObject _owner;
    private AbilityNetwork _networkAbility;
    private float _AOE = 20;

    public void init()
    {
        base.init("Textures/AbilityIcons/Stomp");
        base.abilityName = "Stomp";
        this._owner = this.transform.gameObject;
        this._networkAbility = GetComponent<AbilityNetwork>();     
    }

    override public IEnumerator useAbility()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0 || !playerController.controller.isGrounded)
            yield break;

        StartCoroutine(base.doCoolDown());

        this._networkAbility.Stomp(this._owner, this._AOE, this.transform.position);
    }
}
