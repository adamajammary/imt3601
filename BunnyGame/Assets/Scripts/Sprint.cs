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


        float time = 0;
        while (time < _time) {
            GetComponent<CharacterController>().Move(transform.forward.normalized);
            time += Time.deltaTime;
            yield return null;
        }


    }
}
