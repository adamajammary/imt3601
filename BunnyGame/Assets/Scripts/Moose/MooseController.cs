using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MooseController : NetworkBehaviour{

    private GameObject ramArea;
    private int _ramDamage = 15;
    private bool _isAttackingAnim = false;
    private PlayerController _playerController;

    public override void PreStartClient()
    {
        base.PreStartClient();
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < GetComponent<Animator>().parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);
    }

    // Use this for initialization
    void Start()
    {
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < netAnimator.animator.parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);

        ramArea = transform.GetChild(2).gameObject;

        if (!this.isLocalPlayer)
            return;

        // Set custom attributes for class:
        PlayerEffects pe = GetComponent<PlayerEffects>();
        pe.CmdSetAttributes(1.5f, 1.0f, 1.0f, 0.8f);

        // Add abilities to class:
        PlayerAbilityManager abilityManager = GetComponent<PlayerAbilityManager>();
        SpeedBomb sp = gameObject.AddComponent<SpeedBomb>();
        Stomp stomp = gameObject.AddComponent<Stomp>();
        stomp.init();
        sp.init(30, 4);
        abilityManager.abilities.Add(sp);
        abilityManager.abilities.Add(stomp);

        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(abilityManager);

        this._playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.isLocalPlayer)
            return;

        updateAnimator();

        //if (Input.GetKeyDown(KeyCode.Mouse0))
        if (Input.GetKeyDown(KeyCode.Mouse0) && !this._playerController.getCC())
            StartCoroutine(this.toggleRam());
    }


    private IEnumerator toggleRam()
    {
        _isAttackingAnim = true;
        ramArea.GetComponent<BoxCollider>().enabled = true;
        yield return new WaitForSeconds(1.0f);
        ramArea.GetComponent<BoxCollider>().enabled = false;
        _isAttackingAnim = false;
    }

    public int GetDamage()
    {
        return this._ramDamage;
    }

    // Update the animator with current state
    public void updateAnimator()
    {
        Animator animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
            animator.SetBool("isJumping", !GetComponent<CharacterController>().isGrounded && !GetComponent<PlayerController>().inWater);
            animator.SetBool("isAttacking", _isAttackingAnim);
            animator.SetFloat("height", GetComponent<PlayerController>().velocityY);
        }
    }

    public Vector3 ramImpact()
    {
        return this.ramArea.transform.position;
    }
}
