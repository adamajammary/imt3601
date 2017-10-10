using UnityEngine;
using UnityEngine.Networking;

public class GrenadePoopProjectile : NetworkBehaviour {
    [SyncVar] public GameObject owner; //The gameobject which owns this 

    private const int _timeToLive = 3;

    private const float _speed = 20.0f;
    private const float _antiGravity = 2.5f;
    private const int _damage = 30;
    private const float AOE = 10;

    private Rigidbody _rb;
    private float _timeAlive = 0;

    private const float _noiseSpeed = 2.25f;
    private float _noiseSeed;
    private Material _shader;
    private bool _dead;

    [SyncVar] public int ConnectionID = -1;

    private void Awake() {
        this._dead = false;
        this._noiseSeed = Random.Range(0, 9999);
        this._shader = GetComponent<Renderer>().material;
        this._rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        this._noiseSeed += _noiseSpeed * Time.deltaTime;
        this._shader.SetFloat("_NoiseSeed", this._noiseSeed);
        // to reduce bullet drop
        this._rb.AddForce(Vector3.up * _antiGravity);

        this._timeAlive += Time.deltaTime;

        if (this._timeAlive > _timeToLive)
            Destroy(this.gameObject);
    }

    public void shoot(Vector3 dir, Vector3 pos, Vector3 startVel) {
        this.transform.position = pos;
        this._rb.velocity = dir * _speed + startVel;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject == this.owner || this._dead) return;

        // Find npcs/players in blast area
        int playerlayer = 1 << 8;
        int npcLayer = 1 << 14;
        int layermask = playerlayer | npcLayer;
        Collider[] hit =  Physics.OverlapSphere(this.transform.position, AOE, layermask);
        for (int i = 0; i < hit.Length; i++) {
            if (hit[i].tag == "npc") {
                hit[i].GetComponent<NPC>().kill(this.owner, this.ConnectionID);
            } else if (hit[i].tag == "Player" && hit[i].isTrigger && hit[i].gameObject != owner) {                   
                hit[i].gameObject.GetComponent<PlayerEffects>().OnPoopGrenade(owner, _damage, ConnectionID, transform.position);
            }
        }
        this.owner.GetComponent<AbilityNetwork>().CmdPoopExplosion(this.transform.position);
        Destroy(this.gameObject, 1.0f); //Give the object a chance to collide on other clients
        this.GetComponent<Collider>().enabled = false;
        this.GetComponent<MeshRenderer>().enabled = false;
        this._dead = true;
    }

    public int GetDamage() {
        return _damage;
    }
}
