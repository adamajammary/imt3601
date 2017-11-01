using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBomb : SpecialAbility{

    private Animator         _animator;
    private GameObject       _attackArea;
    private AbilityNetwork   _networkAbility;
    private PlayerController _playerController;
    private float            _speed;
    private float            _time;
   

public void init(float speed, float time)
    {
        base.init("Textures/AbilityIcons/runfast");
        base.abilityName = "Sprint";
        this._speed = speed;
        this._time = time;
        this._animator = GetComponent<Animator>();
        this._attackArea = transform.GetChild(3).gameObject;
        this._networkAbility = GetComponent<AbilityNetwork>();
        this._playerController = GetComponent<PlayerController>();
    }

    override public IEnumerator useAbility()
    {
        if (base._cooldown > 0) 
            yield break;

        StartCoroutine(base.doCoolDown());

        float normalRun = this._playerController.runSpeed;
        float normalWalk = this._playerController.walkSpeed;
        this._playerController.runSpeed = _speed;
        this._playerController.walkSpeed = _speed;
        this._playerController.setNoInputMovement(true);


        if (this._animator != null)
        {
            this._animator.SetBool("speedAttack", true);
           // this._animator.SetFloat("moveSpeed", this._speed);
        }

        this._networkAbility.CmdSuperSpeed(true);
        float time = 0;
        while (time < _time)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (this._animator != null)
        {
            this._animator.SetBool("speedAttack", false);
        }
        
        this._networkAbility.CmdSuperSpeed(false);
        this._playerController.runSpeed = normalRun;
        this._playerController.walkSpeed = normalWalk;
        this._playerController.setNoInputMovement(false);
    }
}
