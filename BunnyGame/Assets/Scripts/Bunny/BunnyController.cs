using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class BunnyController : NetworkBehaviour {

    private float _timer;
    private float _fireRate;
    private CharacterController _controller;
    private GameObject bunnyPoop;
    private PlayerInformation playerInfo;

   public override void PreStartClient()
    {
        base.PreStartClient();
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < GetComponent<Animator>().parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);
    }


 void Start() { 

        if (SceneManager.GetActiveScene().name != "Island")
            return;
          NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < netAnimator.animator.parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);

        bunnyPoop = Resources.Load<GameObject>("Prefabs/poop");
        playerInfo = GetComponent<PlayerInformation>();
        if (!this.isLocalPlayer)
            return;

        _controller = GetComponent<CharacterController>();
        _timer = 0;
        _fireRate = 0.2f;

        // Set custom attributes for class:
        PlayerEffects pe = GetComponent<PlayerEffects>();
        pe.CmdSetAttributes(1.0f, 1.0f, 1.0f, 1.5f);

        // Add abilities to class:
        PlayerController playerController = GetComponent<PlayerController>();
        SuperJump sj = gameObject.AddComponent<SuperJump>();
        sj.init(10);
        GrenadePoop gp = gameObject.AddComponent<GrenadePoop>();
        gp.init();
        playerController.abilities.Add(sj);
        playerController.abilities.Add(gp);


        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(playerController);
    }

 
    void Update() {
        if (!this.isLocalPlayer)
            return;

        updateAnimator();

        if (Input.GetAxisRaw("Fire1") > 0 && Input.GetKey(KeyCode.Mouse1))
            this.shoot();
    }

    private void shoot() {
        if (this.GetComponent<PlayerHealth>().IsDead())
            return;

        this._timer += Time.deltaTime;

        if (this._timer > this._fireRate) {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100)) {
                Vector3 direction = hit.point - this.transform.position;
                Vector3 dirNorm = direction.normalized;
                this.CmdShootPoop(dirNorm, this._controller.velocity, playerInfo.ConnectionID);
            } else {
                Vector3 direction = ray.GetPoint(50.0f) - this.transform.position;
                Vector3 dirNorm = direction.normalized;
                this.CmdShootPoop(dirNorm, this._controller.velocity, playerInfo.ConnectionID);
            }

            this._timer = 0;
        }
    }

    [Command]
    public void CmdShootPoop(Vector3 direction, Vector3 startVel, int id) {
        GameObject poop       = Instantiate(bunnyPoop);
        BunnyPoop  poopScript = poop.GetComponent<BunnyPoop>();
        Vector3    position   = (transform.position + direction * 4.0f);

        poopScript.owner = this.gameObject;
        poopScript.shoot(direction, position, startVel);

        NetworkServer.Spawn(poop);
    }

    // Update the animator with current state
    public void updateAnimator()
    {
        Animator animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
            animator.SetBool("isJumping", !GetComponent<CharacterController>().isGrounded);
            animator.SetFloat("height", GetComponent<PlayerController>().velocityY);
        }

    }
}
