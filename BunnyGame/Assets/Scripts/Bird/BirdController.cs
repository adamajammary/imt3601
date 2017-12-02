using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BirdController : NetworkBehaviour {
    public BoxCollider pecker;

    private const float glideSpeed = -4.0f;
    private bool _pecking;
    private Animator _animator;
    private PlayerController _pc;
    private PlayerAbilityManager _am;

    public override void PreStartClient() {
        base.PreStartClient();
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < GetComponent<Animator>().parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);
    }

    // Use this for initialization
    void Start() {
        this._animator = GetComponent<Animator>();

        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < netAnimator.animator.parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);

        if (!this.isLocalPlayer)
            return;

        this._pc = GetComponent<PlayerController>();

        // Set custom attributes for class:
        PlayerEffects pe = GetComponent<PlayerEffects>();
        pe.CmdSetAttributes(0.7f, 0.8f, 1.2f, 1.0f);

        // Add abilities to class:
        this._am = GetComponent<PlayerAbilityManager>();
        var ds = gameObject.AddComponent<DustStorm>();
        var dt = gameObject.AddComponent<DustTornadoAbility>();

        ds.init();
        dt.init();

        this._am.abilities.Add(ds);
        this._am.abilities.Add(dt);
        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(this._am);

        this._pecking = false;
    }

    // Update is called once per frame
    void Update() {
        if (!this.isLocalPlayer)
            return;

        updateAnimator();

        if (Input.GetKey(KeyCode.Space))
            glide();
        //else if (Input.GetMouseButtonDown(0) && !this._pecking)
        else if (Input.GetMouseButtonDown(0) && !this._pecking && !this._pc.getCC())
            StartCoroutine(this.peck());
            //CmdPeck();
    }

    public int GetDamage() {
        return 0;
    }

    public bool getPecking() {
        return this._pecking;
    }

    public IEnumerator flapLikeCrazy() { //Animation is 1 sec long       
        this._animator.SetBool("flapLikeCrazy", true);
        yield return new WaitForSeconds(2.0f); //Peak of the peck
        this._animator.SetBool("flapLikeCrazy", false);
    }

    private void glide() {
        if (!this._pc.getGrounded()) {
            if (this._pc.velocityY < glideSpeed) {
                this._pc.velocityY = glideSpeed;
            }
            this._animator.SetBool("glide", true);
        }
    }

    // NB! Not needed with reverse attack logic (PlayerAttack.cs)
    [Command]
    private void CmdPeck() {
        RpcPeck();
    }

    [ClientRpc]
    private void RpcPeck() {
        this._animator.SetTrigger("peck");
    }

    private IEnumerator peck() { //Animation is 1 sec long
        this._pecking = true;
        pecker.enabled = true;
        CmdPeck();
        yield return new WaitForSeconds(0.8f); //Peak of the peck
        pecker.enabled = false;
        yield return new WaitForSeconds(0.2f); //Turning back
        this._pecking = false;                 //Peck done
    }

    // Update the animator with current state
    public void updateAnimator() {
        this._animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
        this._animator.SetBool("glide", false);
    }
}
