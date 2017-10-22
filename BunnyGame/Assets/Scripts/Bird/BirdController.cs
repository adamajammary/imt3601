using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BirdController : NetworkBehaviour {


    public override void PreStartClient() {
        base.PreStartClient();
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < GetComponent<Animator>().parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);
    }

    // Use this for initialization
    void Start() {
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
    }

    // Update is called once per frame
    void Update() {
        if (!this.isLocalPlayer)
            return;

        updateAnimator();
    }

    public int GetDamage() {
        return 0;
    }

    // Update the animator with current state
    public void updateAnimator() {
        Animator animator = GetComponent<Animator>();

        if (animator != null) {
            animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
            if (Input.GetMouseButtonDown(0)) {
                animator.SetTrigger("peck");
            }            
        }
    }
}
