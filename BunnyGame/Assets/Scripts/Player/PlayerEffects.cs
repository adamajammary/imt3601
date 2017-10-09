using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEffects : NetworkBehaviour {

    public bool insideWall;

    private GameObject _blood;
    private PlayerController _pc;
    private CharacterController _cc;
    private PlayerHealth _health;
    private const float _damageRate = 0.25f;    //How often to damage player outside wall
    private float _damageTimer;                 //Timer used to find out when to damage player   

    private float _maxFallSpeed = 20; // How fast you can fall before starting to take fall damage
    private int _fallDamage = 40;
    private bool _dealFallDamageOnCollision = false;
    private bool _fallDamageImmune = false;
    private bool _hitByPoopGrenade = false;
    // Use this for initialization
    void Start () {
        this.insideWall = true;
        this._pc = GetComponent<PlayerController>();
        this._cc = GetComponent<CharacterController>();
        this._blood = Resources.Load<GameObject>("Prefabs/Blood");

        this._damageTimer = 0;

        this._health = this.GetComponent<PlayerHealth>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!this.isLocalPlayer) return;
        if (!this.insideWall) // Feels hacky, but when TakeDamage only works on the server its got to be this way
            wallDamage();

        handleFallDamage();
    }
    
    //=========Poop Grenade==================================================================================================================
    public void OnPoopGrenade(int damage, int id, Vector3 impact) {
        this._health.TakeDamage(damage, id);
        StartCoroutine(knockBack(impact));
    }

    public IEnumerator knockBack(Vector3 impact) {
        Vector3 dir = transform.position - impact;
        float force = 10.0f;
        dir.Normalize();
        if (dir.y <= 0.2f) dir.y = 0.2f;
        dir.Normalize();
        Vector3 flatDir = dir;
        flatDir.y = 0;
        flatDir.Normalize();
        Vector3 curDir = dir;
        Vector3 pos = transform.position;
        pos.y += 2;
        transform.position = pos;       
        for (float i = 0; i < 1.0f; i += Time.deltaTime * 2) {
            this._cc.Move(curDir* force * Time.deltaTime);
            curDir = Vector3.Lerp(dir, flatDir, i);
            yield return 0;
        }
    }
    //========================================================================================================================================

    private void OnTriggerEnter(Collider other) {
        if (!this.isLocalPlayer)
            return;

        if ((other.gameObject.tag == "foxbite") && (other.gameObject.transform.parent.gameObject.tag == "Enemy")) {
            FoxController foxScript = other.GetComponentInParent<FoxController>();
            PlayerInformation otherInfo = other.GetComponentInParent<PlayerInformation>();

            if ((this._health != null) && (foxScript != null) && !this._health.IsDead()) {
                this.CmdBloodParticle(foxScript.biteImpact());
                this._health.TakeDamage(foxScript.GetDamage(), otherInfo.ConnectionID);
            }
        } else if (other.gameObject.tag == "projectile") {
            BunnyPoop poopScript = other.gameObject.GetComponent<BunnyPoop>();
            PlayerInformation otherInfo = poopScript.owner.GetComponent<PlayerInformation>();
            if ((this._health != null) && (poopScript != null) && !this._health.IsDead()) {
                this.CmdBloodParticle(other.gameObject.transform.position);
                this._health.TakeDamage(poopScript.GetDamage(), otherInfo.ConnectionID);
            }

            Destroy(other.gameObject);
        } else if (other.gameObject.name == "Water") {
            this._fallDamageImmune = true; // Immune from falldamage when in water
        }
    }

    private void wallDamage() {
        if (this._damageTimer > _damageRate) {
            this.GetComponent<PlayerHealth>().TakeDamage(1, -1);
            this._damageTimer = 0;
        }
        this._damageTimer += Time.deltaTime;
    }

    public void onWaterStay(float waterForce) {
        this._pc.velocityY += waterForce * Time.deltaTime;
    }

    private void handleFallDamage() {
        if (_fallDamageImmune) { // Cannot take damage while immune
            _dealFallDamageOnCollision = false;
        } else if (-this._pc.velocityY > _maxFallSpeed && !_dealFallDamageOnCollision)
            _dealFallDamageOnCollision = true;
        else if (-this._pc.velocityY < 1 && _dealFallDamageOnCollision) {
            this.GetComponent<PlayerHealth>().TakeDamage(_fallDamage, -1);
            _dealFallDamageOnCollision = false;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!this.isLocalPlayer)
            return;

        if (other.gameObject.name == "Water")
            this._fallDamageImmune = false;
    }

    [Command]
    private void CmdBloodParticle(Vector3 hitPosition) {
        GameObject blood = Instantiate(this._blood);

        blood.transform.position = hitPosition;

        NetworkServer.Spawn(blood);
        Destroy(blood, 5.0f);
    }


}
