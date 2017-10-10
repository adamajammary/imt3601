using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FoxController : NetworkBehaviour {

    private GameObject biteArea;
    private int _biteDamage = 15;
    

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

        biteArea = transform.GetChild(2).gameObject;

        if (!this.isLocalPlayer)
            return;

        // Set custom attributes for class:
        PlayerEffects pe = GetComponent<PlayerEffects>();
        pe.setAttributes(1.2f, 1.2f, 1.2f, 0.8f);

        // Add abilities to class:
        PlayerController playerController = GetComponent<PlayerController>();
        Sprint sp = gameObject.AddComponent<Sprint>();
        sp.init(50, 1);
        playerController.abilities.Add(sp);

        Stealth st = gameObject.AddComponent<Stealth>();
        st.init(1, 0.1f);
        playerController.abilities.Add(st);

        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(playerController);
    }

    // Update is called once per frame
    void Update() {
        if (!this.isLocalPlayer)
            return;

        updateAnimator();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            this.bite();
    }

    private void bite() {
        if (this.GetComponent<PlayerHealth>().IsDead())
            return;

        if (this.isServer)
            this.RpcBite();
        else if (this.isClient)
            this.CmdBite();
    }

    [Command]
    private void CmdBite() {
        this.RpcBite();
    }

    [ClientRpc]
    private void RpcBite() {
        StartCoroutine(this.toggleBite());
    }

    // Biting is enabled for 1 tick after called
    private IEnumerator toggleBite() {
        biteArea.GetComponent<BoxCollider>().enabled = true; 
        yield return 0;
        biteArea.GetComponent<BoxCollider>().enabled = false;
    }

    public int GetDamage() {
        return this._biteDamage;
    }

    // Update the animator with current state
    public void updateAnimator() {
        Animator animator = GetComponent<Animator>();

        if (animator != null)
            animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
    }

    public Vector3 biteImpact() {
        return this.biteArea.transform.position;
    }
}
