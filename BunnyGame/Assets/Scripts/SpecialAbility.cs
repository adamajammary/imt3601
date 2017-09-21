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
    protected float cooldown;
    public float cooldownTimeInSeconds;

    public abstract void useAbility();

    public float getCooldown() {
        return cooldown;
    }

    protected IEnumerator doCoolDown() {
        while (cooldown> 0.01f) {
            cooldown -= Time.deltaTime;
            yield return 0;
        }
        cooldown = 0;
    }
}
