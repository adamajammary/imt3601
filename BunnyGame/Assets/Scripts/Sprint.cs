using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* ****************************************************************************
 * TODO: 
 * - Disable running while sprinting
 * - Make ability use on demand? (Perhaps do this to the regular running for
 *   all players(Stamina)? and keep this ability like this)
 *      - Hold down a key to use it
 *      - Use the cooldown as a "max boost time" meter so it can be used as
 *        long as the cooldown isn't maxed out
 *        
 *****************************************************************************/


public class Sprint : SpecialAbility {
    private float _speed;
    private float _time;

    public void init(float speed, float time){
        base.init("Textures/AbilityIcons/test");
        base.abilityName = "Sprint";
        _speed = speed;
        _time = time;
    }

    override public IEnumerator useAbility() {
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0 || !playerController.controller.isGrounded) // Can't sprint if in the air
            yield break;
        
        StartCoroutine(base.doCoolDown());

        float curSpeed = _speed;
        float speedSmoothVel = 0;
        float time = 0;
        while (time < _time) {
            curSpeed = Mathf.SmoothDamp(curSpeed, _speed, ref speedSmoothVel, 0.05f);
            GetComponent<CharacterController>().Move(transform.forward * curSpeed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }


    }
}
