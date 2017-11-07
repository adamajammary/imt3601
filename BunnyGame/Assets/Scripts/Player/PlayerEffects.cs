using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerEffects : NetworkBehaviour {
    public bool insideWall;

    [SyncVar] private float _toughness;
    [SyncVar] private float _damage;
    [SyncVar] private float _speed;
    [SyncVar] private float _jump;

    private GameObject          _blood;
    private PlayerController    _pc;
    private CharacterController _cc;
    private PlayerHealth        _health;
    private PlayerAudio         _playerAudio;
    private Image               _blindEffect;
    private BreathMeter         _breathMeter;
    
    private int   _fallDamage = 40;
    private bool  _fallDamageImmune = false;
    private float _damageImpactVelocity = -20;
    private float _currentImpactVelocity = 0;

    private int _waterDamage = 5;
    private float _timeInWater = 0;
    public float _maxTimeInWater = 10;

    // Use this for initialization
    void Start () {
        this.insideWall     = true;
        this._pc            = GetComponent<PlayerController>();
        this._cc            = GetComponent<CharacterController>();
        this._blood         = Resources.Load<GameObject>("Prefabs/Blood");
        this._health        = this.GetComponent<PlayerHealth>();
        this._playerAudio   = GetComponent<PlayerAudio>();
        this._blindEffect   = GameObject.Find("BlindOverlay").GetComponent<Image>();
        this._breathMeter   = GameObject.Find("BreathMeter").GetComponent<BreathMeter>();
    }
	
	// Update is called once per frame
	void Update () {
        handleGroundHit();
        if (!this.isLocalPlayer) return;
        if (!this.insideWall) wallDamage();

        handleBreath();
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

    public float calcDamage(GameObject attacker, float damage) { // Use this to get attribute adjusted damage
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
        this._pc.setCC(true);
        for (float i = 0; i < 1.0f; i += Time.deltaTime * 2) {
            this._cc.Move(curDir* force * Time.deltaTime);
            curDir = Vector3.Lerp(dir, flatDir, i);
            yield return 0;
        }
        this._pc.setCC(false);
    }

    //=========Dust Storm=====================================================================================================================
    public IEnumerator blind() {
        float timer = 0;
        while (timer < 10) { // Incase multiple blinds overlap.
            timer += Time.deltaTime;
            this._blindEffect.enabled = true;
            yield return 0;
        };
        this._blindEffect.enabled = false;
    }

    //=========Other==========================================================================================================================
    // Triggers when something collides with our player character.
    // We are the victim, while the enemy is the attacker.
    private void OnTriggerEnter(Collider other) {
        if (!this.isLocalPlayer)
            return;

        // NB! Handled in PlayerAttack.cs
        /*if ((other.gameObject.tag == "foxbite") && (other.gameObject.transform.parent.gameObject.tag == "Enemy")) {
            FoxController foxScript = other.GetComponentInParent<FoxController>();
            PlayerInformation otherInfo = other.GetComponentInParent<PlayerInformation>();

            if ((this._health != null) && (foxScript != null) && !this._health.IsDead()) {
                this.CmdBloodParticle(foxScript.biteImpact());
                this._health.TakeDamage(calcDamage(other.transform.parent.gameObject, foxScript.GetDamage()), otherInfo.ConnectionID);
            }
        } else if (other.gameObject.tag == "projectile") {
            BunnyPoop poopScript = other.gameObject.GetComponent<BunnyPoop>();
            PlayerInformation otherInfo = poopScript.owner.GetComponent<PlayerInformation>();
            if ((this._health != null) && (poopScript != null) && !this._health.IsDead() && poopScript.owner.gameObject != this.gameObject) {
                this.CmdBloodParticle(other.gameObject.transform.position);
                this._health.TakeDamage(calcDamage(poopScript.owner, poopScript.GetDamage()), otherInfo.ConnectionID);
            }

            Destroy(other.gameObject);
        } else if (other.gameObject.tag == "pecker" && other.transform.parent.tag == "Enemy") {
            pecker p = other.gameObject.GetComponent<pecker>();
            PlayerInformation otherInfo = p.owner.GetComponent<PlayerInformation>();
            if ((this._health != null) && !this._health.IsDead()) {
                this.CmdBloodParticle(other.gameObject.transform.position);
                this._health.TakeDamage(calcDamage(p.owner, p.GetDamage()), otherInfo.ConnectionID);
            }
        } else if ((other.gameObject.tag == "mooseAttack") && (other.gameObject.transform.parent.gameObject.tag == "Enemy"))
        {
            MooseController MooseScript = other.GetComponentInParent<MooseController>();
            PlayerInformation otherInfo = other.GetComponentInParent<PlayerInformation>();

            if ((this._health != null) && (MooseScript != null) && !this._health.IsDead())
            {
                this.CmdBloodParticle(MooseScript.ramImpact());
                this._health.TakeDamage(calcDamage(other.transform.parent.gameObject, MooseScript.GetDamage()), otherInfo.ConnectionID);
            }
        } else */
        if (other.gameObject.name == "Water") {
            this._fallDamageImmune = true; // Immune from falldamage when in water
        }
    }

    private void wallDamage() {
        //this.GetComponent<PlayerHealth>().TakeDamage(10 * Time.deltaTime, -1);
        this.GetComponent<PlayerHealth>().TakeDamage(10 * Time.deltaTime, (int)KillerID.KILLER_ID_WALL);
    }

    public void onWaterStay(float waterForce) {
        if (!this._pc.getCC())
            this._pc.velocityY += waterForce * Time.deltaTime;
    }

    private void handleGroundHit() {
        if (!_fallDamageImmune && _cc.isGrounded && _currentImpactVelocity < _damageImpactVelocity) {
            _playerAudio.playGroundHit(_currentImpactVelocity);
            if (isLocalPlayer) {
                //this.GetComponent<PlayerHealth>().TakeDamage(_fallDamage * (_currentImpactVelocity / _damageImpactVelocity), -1);
                this.GetComponent<PlayerHealth>().TakeDamage(_fallDamage * (_currentImpactVelocity / _damageImpactVelocity), (int)KillerID.KILLER_ID_FALL);
                _currentImpactVelocity = 0;
            }
        }
        else if(isLocalPlayer) _currentImpactVelocity = _cc.velocity.y;
    }

    private void handleBreath() {
        if (_pc.inWater && _timeInWater < _maxTimeInWater)
            _timeInWater += Time.deltaTime;
        else if (!_pc.inWater && _timeInWater > 0)
            _timeInWater -= Time.deltaTime;

        this._breathMeter.breath = 1 - _timeInWater / _maxTimeInWater;
        if (_timeInWater > _maxTimeInWater)
            this.GetComponent<PlayerHealth>().TakeDamage(Time.deltaTime * _waterDamage, (int)KillerID.KILLER_ID_WATER);
    }

    private void OnTriggerExit(Collider other) {
        if (!this.isLocalPlayer)
            return;

        if (other.gameObject.name == "Water")
            this._fallDamageImmune = false;
    }

    [Command]
    public void CmdBloodParticle(Vector3 hitPosition) {
        GameObject blood = Instantiate(this._blood);

        blood.transform.position = hitPosition;

        NetworkServer.Spawn(blood);
        Destroy(blood, 5.0f);
    }
}
