using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* ****************************************************************************
 * TODO for potential V2?:
 * - Make ability use on demand? (Perhaps do this to the regular running for
 *   all players(Stamina)? and keep this ability like this)
 *      - Hold down a key to use it
 *      - Use the cooldown as a "max boost time" meter so it can be used as
 *        long as the cooldown isn't maxed out
 *        
 *****************************************************************************/

/***************************************
 * Allows the player to run extra fast
 **************************************/
public class Sprint : SpecialAbility {
    private float _speed;
    private float _time;

    public void init(float speed, float time) {
        base.init("Textures/AbilityIcons/runfast");
        base.abilityName = "Sprint";
        _speed = speed;
        _time = time;
    }

    override public IEnumerator useAbility() {
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0 || !playerController.controller.isGrounded || // Can't start sprinting if in the air
            playerController.currentSpeed < 0.01f ) // Can't start sprinting from a stand-still
            yield break;
        
        StartCoroutine(base.doCoolDown());

        float normalRun = playerController.runSpeed;
        float normalWalk = playerController.walkSpeed;
        playerController.runSpeed = _speed;
        playerController.walkSpeed = _speed;

        float time = 0;
        while (time < _time) {
            time += Time.deltaTime;
            yield return null;
        }
        playerController.runSpeed = normalRun;
        playerController.walkSpeed = normalWalk;
    }
}
