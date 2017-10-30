using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBomb : SpecialAbility{

    private float _speed;
    private float _time;
    private Animator _animator;


public void init(float speed, float time)
    {
        base.init("Textures/AbilityIcons/runfast");
        base.abilityName = "Sprint";
        this._speed = speed;
        this._time = time;
        this._animator = GetComponent<Animator>();

}

    override public IEnumerator useAbility()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0 || !playerController.controller.isGrounded || // Can't start sprinting if in the air
            playerController.currentSpeed < 0.01f) // Can't start sprinting from a stand-still
            yield break;

        StartCoroutine(base.doCoolDown());

        float normalRun = playerController.runSpeed;
        float normalWalk = playerController.walkSpeed;
        playerController.runSpeed = _speed;
        playerController.walkSpeed = _speed;

        if (this._animator != null)
        {
            this._animator.SetBool("speedAttack", true);
            this._animator.SetFloat("moveSpeed", this._speed);
        }

        float time = 0;
        while (time < _time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (this._animator != null)
        {
            this._animator.SetBool("speedAttack", false);
            //this._animator.SetFloat("moveSpeed" , normalRun);
        }

        playerController.runSpeed = normalRun;
        playerController.walkSpeed = normalWalk;
    }
}
