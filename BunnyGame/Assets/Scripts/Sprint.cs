using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
        if (base._cooldown > 0 || !playerController.controller.isGrounded)
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
        //while (curSpeed > 0) {
        //    curSpeed = Mathf.SmoothDamp(curSpeed, 0, ref speedSmoothVel, 0.05f);
        //    GetComponent<CharacterController>().Move(transform.forward.normalized * curSpeed * Time.deltaTime);
        //    time += Time.deltaTime;
        //    yield return null;
        //}


    }
}
