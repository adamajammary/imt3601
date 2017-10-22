using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BirdController : NetworkBehaviour {
    public BoxCollider pecker;
    private bool _pecking;

    private Animator _animator;

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

        // Set custom attributes for class:
        PlayerEffects pe = GetComponent<PlayerEffects>();
        pe.CmdSetAttributes(1.0f, 1.2f, 1.5f, 1.0f);

        // Add abilities to class:
        PlayerController playerController = GetComponent<PlayerController>();

        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(playerController);

        this._pecking = false;
    }

    // Update is called once per frame
    void Update() {
        if (!this.isLocalPlayer)
            return;

        updateAnimator();

        if (Input.GetMouseButtonDown(0) && !this._pecking) {
            StartCoroutine(peck());
        }
    }

    public int GetDamage() {
        return 0;
    }

    private IEnumerator peck() { //Animation is 1 sec long
        this._pecking = true;
        pecker.enabled = true;
        this._animator.SetTrigger("peck");
        yield return new WaitForSeconds(0.6f); //Peak of the peck
        pecker.enabled = false;
        yield return new WaitForSeconds(0.4f); //Turning back
        this._pecking = false;                 //Peck done
    }

    // Update the animator with current state
    public void updateAnimator() {
        this._animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
    }
}
