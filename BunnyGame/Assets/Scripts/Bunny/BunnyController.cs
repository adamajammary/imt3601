using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BunnyController : NetworkBehaviour {

    private float _timer;
    private float _fireRate;
    private CharacterController _controller;
    private GameObject bunnyPoop;
    private PlayerInformation playerInfo;
    private Transform _poopCameraPos;            // For aiming with camera offset
    private AudioClip _alertSound;
    private RawImage _alertOverlay;
    private GameObject[] _enemies;
    private bool[] _enemyInRange;
    private const float _alertDistance = 30.0f;
    private PlayerController _playerController;

    public override void PreStartClient() {
        base.PreStartClient();
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < GetComponent<Animator>().parameterCount; i++)
        netAnimator.SetParameterAutoSend(i, true);
    }

    void Start() { 
        if (SceneManager.GetActiveScene().name == "Lobby")
            return;

        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < netAnimator.animator.parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);

        this._poopCameraPos = transform.GetChild(2);
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
        PlayerAbilityManager abilityManager = GetComponent<PlayerAbilityManager>();
        SuperJump sj = gameObject.AddComponent<SuperJump>();
        sj.init(10);
        GrenadePoop gp = gameObject.AddComponent<GrenadePoop>();
        gp.init();
        abilityManager.abilities.Add(sj);
        abilityManager.abilities.Add(gp);


        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(abilityManager);

        this._alertOverlay = GameObject.Find("Alert").GetComponent<RawImage>();
        this._alertSound = Resources.Load<AudioClip>("Audio/BunnyAlert");
        CmdGetEnemies();

        this._playerController = GetComponent<PlayerController>();
    }

    void Update() {
        if (!this.isLocalPlayer || this.GetComponent<PlayerHealth>().IsDead())
            return;

        updateAnimator();

        //if (Input.GetAxisRaw("Fire1") > 0 && Input.GetKey(KeyCode.Mouse1))
        if (Input.GetAxisRaw("Fire1") > 0 && Input.GetKey(KeyCode.Mouse1) && !this._playerController.getCC())
            this.shoot();

        //Bunny passive "sixth sense"
        if (this._enemies != null) {
            for (int i = 0; i < this._enemies.Length; i++) {
                if (this._enemies[i] != null && !this._enemies[i].GetComponent<PlayerHealth>().IsDead()) {
                    if (Vector3.Distance(this._enemies[i].transform.position, transform.position) < _alertDistance) {
                        if (!this._enemyInRange[i])
                            alert();
                        this._enemyInRange[i] = true;
                    } else {
                        this._enemyInRange[i] = false;
                    }
                }
            }
        }
    }

    private void alert() {
        GetComponent<AudioSource>().PlayOneShot(this._alertSound);
        StartCoroutine(alertOverlay());
    }

    private IEnumerator alertOverlay() {
        for (float t = 1; t >= 0; t -= Time.deltaTime) {
            this._alertOverlay.enabled = true; //incase multiple alerts overlap
            this._alertOverlay.color = new Color(1, 1, 1, t);
            yield return 0;
        }
        this._alertOverlay.enabled = false;
    }

    private IEnumerator getEnemies(int playerCount) {
        do {
            yield return 0;
            this._enemies = GameObject.FindGameObjectsWithTag("Enemy");
        } while (this._enemies.Length + 1 != playerCount);
        this._enemyInRange = new bool[this._enemies.Length];
        for (int i = 0; i < this._enemyInRange.Length; i++)
            this._enemyInRange[i] = false;
    }

    [Command]
    private void CmdGetEnemies() {
        TargetGetEnemies(this.connectionToClient, Object.FindObjectOfType<NetworkPlayerSelect>().numPlayers);
    }

    [TargetRpc]
    private void TargetGetEnemies(NetworkConnection conn, int playerCount) {
        StartCoroutine(getEnemies(playerCount));
    }

    private void shoot() {
        if (this.GetComponent<PlayerHealth>().IsDead())
            return;

        this._timer += Time.deltaTime;

        if (this._timer > this._fireRate) {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100)) {
                Vector3 direction = hit.point - this._poopCameraPos.position;
                // Fix shooting when you shoot your own projectile
                if (hit.transform.tag == "projectile")
                    direction = ray.GetPoint(50.0f) - this._poopCameraPos.position;
                
                //Debug.Log(hit.transform);
                Vector3 dirNorm = direction.normalized;
                this.CmdShootPoop(dirNorm, this._controller.velocity, playerInfo.ConnectionID);
            } else {
                Vector3 direction = ray.GetPoint(50.0f) - this._poopCameraPos.position;
                Vector3 dirNorm = direction.normalized;
                this.CmdShootPoop(dirNorm, this._controller.velocity, playerInfo.ConnectionID);
            }

            this._timer = 0;
        }
    }

    [Command]
    public void CmdShootPoop(Vector3 direction, Vector3 startVel, int id) {

        GameObject   poop         = Instantiate(bunnyPoop);
        BunnyPoop    poopScript   = poop.GetComponent<BunnyPoop>();
        PlayerAttack attackScript = poop.GetComponent<PlayerAttack>();
        Vector3 position = (this._poopCameraPos.position + direction * 4.0f);

        //poopScript.owner   = this.gameObject;
        attackScript.owner = this.gameObject;

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
            animator.SetBool("isJumping", !GetComponent<CharacterController>().isGrounded && !GetComponent<PlayerController>().inWater);
            animator.SetFloat("height", GetComponent<PlayerController>().velocityY);
        }

    }
}
