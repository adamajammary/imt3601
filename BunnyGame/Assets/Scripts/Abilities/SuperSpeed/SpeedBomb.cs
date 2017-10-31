using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBomb : SpecialAbility{

    private Animator         _animator;
    private GameObject       _attackArea;
    private AbilityNetwork   _networkAbility;
    // private PlayerController _controller;
   // private CharacterController _controller;
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
        //this._controller = GetComponent<PlayerController>();
      //  this._controller = GetComponent<CharacterController>();

    }

    override public IEnumerator useAbility()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0) 
            yield break;

        StartCoroutine(base.doCoolDown());

        float normalRun = playerController.runSpeed;
        float normalWalk = playerController.walkSpeed;
        playerController.runSpeed = _speed;
        playerController.walkSpeed = _speed;

        if (this._animator != null)
        {
            this._animator.SetBool("speedAttack", true);
           // this._animator.SetFloat("moveSpeed", this._speed);
        }

        this._networkAbility.CmdSuperSpeed(true);
        float time = 0;
        while (time < _time)
        {
            //Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            //Vector2 inputDir = input.normalized;
            //Vector3 moveDir = transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y));
            //moveDir.y = 0;
            //Vector3 velocity = moveDir.normalized * this._speed;
            //this._controller.Move(velocity * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }

        if (this._animator != null)
        {
            this._animator.SetBool("speedAttack", false);
        }
      
        this._networkAbility.CmdSuperSpeed(false);
        playerController.runSpeed = normalRun;
        playerController.walkSpeed = normalWalk;
    }
}
