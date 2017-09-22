using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/*
 * Base class for all special abilities
 * 
 * The execution of an ability should override from the useAbility() coroutine. 
 * The sub-class' useAbility() should always check base._cooldown and call base.doCoolDown() at the start (see SuperJump). (This is of course only when you want to use the cooldown feature).
 */
public abstract class SpecialAbility : MonoBehaviour {
    public string abilityName;
    protected float _cooldown = 0;
    protected float _cooldownTimeInSeconds = 10;
    private string imagePath = "";

    public abstract IEnumerator useAbility();

    public float getCooldownPercent() {
        return _cooldown / _cooldownTimeInSeconds;
    }

    public string getImagePath() {
        return imagePath;
    }

    protected void init(string imagePath) {
        this.imagePath = imagePath;
    }


    protected IEnumerator doCoolDown() {
        _cooldown = _cooldownTimeInSeconds;
        while (_cooldown> 0.01f) {
            _cooldown -= Time.deltaTime;
            yield return 0;
        }
        _cooldown = 0;
    }
}
