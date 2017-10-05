using UnityEngine;
using UnityEngine.Networking;

public class GrenadePoopProjectile : NetworkBehaviour {
    [SyncVar] public GameObject owner; //The gameobject which owns this 

    private const int _timeToLive = 3;

    private const float _speed = 10.0f;
    private const float _antiGravity = 2.5f;
    private const int _damage = 30;
    private const float AOE = 10;

    private Rigidbody _rb;
    private float _timeAlive = 0;


    [SyncVar] public int ConnectionID = -1;

    private void Awake() {
        this._rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate() {
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
        if (other.gameObject.tag != "Player") {
            // Find npcs/players in blast area
            int playerlayer = 1 << 8;
            int npcLayer = 1 << 14;
            int layermask = playerlayer | npcLayer;
            Collider[] hit =  Physics.OverlapSphere(this.transform.position, AOE, layermask);
            for (int i = 0; i < hit.Length; i++) {
                if (hit[i].tag == "npc") {
                    hit[i].GetComponent<NPC>().kill(this.owner, this.ConnectionID);
                } else if (hit[i].tag == "Player" && hit[i].isTrigger) { // HealthScript only works on localplayers, so calling TakeDamage on an enemy does nothing
                                                     // This means that the grenade can hurt the one who uses it, which might be fine.                    
                    hit[i].gameObject.GetComponent<PlayerEffects>().OnPoopGrenade(_damage, ConnectionID);
                }
            }
            this.owner.GetComponent<AbilityNetwork>().CmdPoopExplosion(this.transform.position);
            Destroy(this.gameObject);
        }
    }

    public int GetDamage() {
        return _damage;
    }
}
