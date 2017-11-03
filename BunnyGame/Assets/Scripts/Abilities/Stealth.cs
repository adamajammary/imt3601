using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/********************************************
 * Allows for the player to become invisible
 *******************************************/
public class Stealth : SpecialAbility {

    private float _stealthActive = 7.0f;
    private float _transparency = 0.1f;
    private float _stealthSoundLevel = 0.1f;
    private int _modelChildNum = 1;

    AbilityNetwork networkAbility;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && base._cooldown != 0)
            cancel();
    }

    public void init(int modelChildNumb,float transparency)
    {
        base.init("Textures/AbilityIcons/ninjaman");
        base._cooldownTimeInSeconds = 15;
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

        networkAbility.useStealth(this._modelChildNum,this._stealthActive, this._transparency, this._stealthSoundLevel);
    }


    public void cancel() {
        GetComponent<PlayerAudio>().updateVolume(volumeModifier: 1);
        networkAbility.RpcSetOrginalFox();
    }
}
