using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEffects : NetworkBehaviour {
    public bool                 insideWall;

    [SyncVar] private float     _toughness;
    [SyncVar] private float     _damage;
    [SyncVar] private float _speed;
    [SyncVar] private float _jump;

    private GameObject          _blood;
    private PlayerController    _pc;
    private CharacterController _cc;
    private PlayerHealth        _health;  

    private float               _maxFallSpeed = 20; // How fast you can fall before starting to take fall damage
    private int                 _fallDamage = 40;
    private bool                _dealFallDamageOnCollision = false;
    private bool                _fallDamageImmune = false;

    // Use this for initialization
    void Start () {
        this.insideWall = true;
        this._pc        = GetComponent<PlayerController>();
        this._cc        = GetComponent<CharacterController>();
        this._blood     = Resources.Load<GameObject>("Prefabs/Blood");
        this._health    = this.GetComponent<PlayerHealth>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!this.isLocalPlayer) return;
        if (!this.insideWall) wallDamage();
        handleFallDamage();
    }

    //=========Attrbutes=====================================================================================================================
    [Command]
    public void CmdSetAttributes(float t, float d, float s, float j) { // Used when spawning players
        this._toughness = t;
        this._damage    = d;
        this._speed     = s;
        this._jump      = j;
    }

    [Command] public void CmdAddToughness(float amount)  { this._toughness += amount; }
    [Command] public void CmdAddDamage(float amount)     { this._damage += amount; }
    [Command] public void CmdAddSpeed(float amount)      { this._speed += amount; }
    [Command] public void CmdAddJump(float amount)       { this._jump += amount; }    

    public float getToughness() { return this._toughness; } //Used when getting dealt damage (multiplier)
    public float getDamage()    { return this._damage; }    //Used when dealing damage (multiplier)
    public float getSpeed()     { return this._speed; }     //Used when moving (multiplier)
    public float getJump()      { return this._jump; }      //Used when jumping (multiplier)

    private float calcDamage(GameObject attacker, float damage) { // Use this to get attribute adjusted damage
        float damageMult = attacker.GetComponent<PlayerEffects>().getDamage();
        //Debug.Log("Damage: " + damage + "DamageMult: " + damageMult + "Toughness: " + this._toughness);
        //Debug.Log("Final damage: " + damage * damageMult / this._toughness);
        return damage * damageMult / this._toughness;
    }
    //=========Poop Grenade==================================================================================================================
    public void OnPoopGrenade(GameObject attacker, int damage, int id, Vector3 impact) {
        this._health.TakeDamage(calcDamage(attacker, damage), id);
        StartCoroutine(knockBack(impact));
    }

    public IEnumerator knockBack(Vector3 impact) {
        Vector3 dir = transform.position - impact;
        float force = 20.0f;
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

    //=========Other==========================================================================================================================
    private void OnTriggerEnter(Collider other) {
        if (!this.isLocalPlayer)
            return;

        if ((other.gameObject.tag == "foxbite") && (other.gameObject.transform.parent.gameObject.tag == "Enemy")) {
            FoxController foxScript = other.GetComponentInParent<FoxController>();
            PlayerInformation otherInfo = other.GetComponentInParent<PlayerInformation>();

            if ((this._health != null) && (foxScript != null) && !this._health.IsDead()) {
                this.CmdBloodParticle(foxScript.biteImpact());
                this._health.TakeDamage(calcDamage(other.transform.parent.gameObject, foxScript.GetDamage()), otherInfo.ConnectionID);
            }
        } else if (other.gameObject.tag == "projectile") {
            BunnyPoop poopScript = other.gameObject.GetComponent<BunnyPoop>();
            PlayerInformation otherInfo = poopScript.owner.GetComponent<PlayerInformation>();
            if ((this._health != null) && (poopScript != null) && !this._health.IsDead()) {
                this.CmdBloodParticle(other.gameObject.transform.position);
                this._health.TakeDamage(calcDamage(poopScript.owner, poopScript.GetDamage()), otherInfo.ConnectionID);
            }

            Destroy(other.gameObject);
        } else if (other.gameObject.name == "Water") {
            this._fallDamageImmune = true; // Immune from falldamage when in water
        }
    }

    

    private void wallDamage() {
        this.GetComponent<PlayerHealth>().TakeDamage(10 * Time.deltaTime, -1);
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
