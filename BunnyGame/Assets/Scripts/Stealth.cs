using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/********************************************
 * Allows for the player to become invisible
 *******************************************/
public class Stealth : SpecialAbility {

    private float _stealthActive = 10.0f;
    private float _transparency = 0.1f;
    private int _modelChildNum = 1;

    AbilityNetwork networkAbility;

    public void init(int modelChildNumb,float transparency)
    {
        base.init("Textures/AbilityIcons/test");
        base.abilityName = "Stealth";
        this._transparency = transparency;
        this._modelChildNum = modelChildNumb;
        networkAbility = GetComponent<AbilityNetwork>();
    }

    override public IEnumerator useAbility()
    {
        if (base._cooldown > 0)
            yield break;

        StartCoroutine(base.doCoolDown());

        networkAbility.useStealth(this._modelChildNum,this._stealthActive, this._transparency);
    }
}
