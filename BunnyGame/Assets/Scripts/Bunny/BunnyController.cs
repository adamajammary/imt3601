using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyController : NetworkBehaviour {

    private float _timer;
    private float _fireRate;
    private CharacterController _controller;
    private GameObject bunnyPoop;
    private int _connectionID = -1;

    void Start () {
        bunnyPoop = Resources.Load<GameObject>("Prefabs/poop");

        if (!this.isLocalPlayer)
            return;

        _controller = GetComponent<CharacterController>();
        _timer = 0;
        _fireRate = 0.2f;

        // Set custom attributes for class:
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.jumpHeight = 3;

        // Add abilities to class:
        SuperJump sj = gameObject.AddComponent<SuperJump>();
        sj.init(10);
        playerController.abilities.Add(sj);
        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(playerController);

        NetworkClient client = NetworkClient.allClients[0];

        if (client != null)
            this._connectionID = client.connection.connectionId;
    }

    void Update () {
        if (!this.isLocalPlayer)
            return;

        if (Input.GetAxisRaw("Fire1") > 0 && Input.GetKey(KeyCode.Mouse1))
            this.shoot();
    }

    private void shoot() {
        if (this.GetComponent<PlayerHealth>().IsDead())
            return;

        this._timer += Time.deltaTime;
        if (this._timer > this._fireRate)
        {

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Vector3 direction = hit.point - this.transform.position;
                Vector3 dirNorm = direction.normalized;
                this.CmdShootPoop(dirNorm, this._controller.velocity, this._connectionID);
            }
            else
            {
                Vector3 direction = ray.GetPoint(50.0f) - this.transform.position;
                Vector3 dirNorm = direction.normalized;
                this.CmdShootPoop(dirNorm, this._controller.velocity, this._connectionID);
            }
            this._timer = 0;
        }
    }

    [Command]
    public void CmdShootPoop(Vector3 dir, Vector3 startVel, int id)
    {
        GameObject poop       = Instantiate(bunnyPoop);
        BunnyPoop  poopScript = poop.GetComponent<BunnyPoop>();

        // Assign the player connection ID to the projectile.
        poopScript.SetConnectionID(id);

        Vector3 pos = transform.position;
        pos += dir * 4.0f;
        poopScript.shoot(dir, pos, startVel);

        NetworkServer.Spawn(poop);
    }
}
