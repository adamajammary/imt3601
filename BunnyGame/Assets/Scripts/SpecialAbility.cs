using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/*
 * Base class for all special abilities
 * 
 * To use the cooldown, check this.cooldown at the start of subclass.useAbility(). Followed by calling doCooldown()
 */
public abstract class SpecialAbility : MonoBehaviour {
    public string imagePath;
    protected float _cooldown = 0;
    protected float _cooldownTimeInSeconds = 5;


    protected void init(string imagePath) {
        this.imagePath = imagePath;
    }

    public abstract IEnumerator useAbility();

    public float getCooldown() {
        return _cooldown;
    }

    protected IEnumerator doCoolDown() {
        while (_cooldown> 0.01f) {
            _cooldown -= Time.deltaTime;
            yield return 0;
        }
        _cooldown = 0;
    }
}
