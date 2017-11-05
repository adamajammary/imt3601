using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



/***************************************************************************
 * Used to create functions for Special Abilities that requires networking
 * 
 ***************************************************************************/


public class AbilityNetwork : NetworkBehaviour {

    private int modelChildNum = 1;

    private GameObject _poopGrenade;
    private GameObject _explosion;
    private GameObject _dustParticles;
    private GameObject _dustTornado;
    private GameObject _fireFart;

    void Start() {
        this._poopGrenade = Resources.Load<GameObject>("Prefabs/PoopGrenade/PoopGrenade");
        this._explosion = Resources.Load<GameObject>("Prefabs/PoopGrenade/PoopExplosion");
        this._dustParticles = Resources.Load<GameObject>("Prefabs/BirdSpecial/DustStorm");
        this._dustTornado = Resources.Load<GameObject>("Prefabs/BirdSpecial/DustTornado");

        if (this.transform.childCount > 6)
            this._fireFart = this.transform.GetChild(6).gameObject;
    }
  

    ///////////// Functions for Stealth ability /////////////////

    public void useStealth(int modelChildNum, float activeTime, float transparancy, float volumeMod)
    {
        this.modelChildNum = modelChildNum;
        CmdStealth(activeTime, transparancy, volumeMod);
    }
    
    [Command]
    public void CmdStealth(float activeTime, float transparancy, float volumeMod)
    {
        StartCoroutine(stealth(activeTime,transparancy, volumeMod));
    }

    private IEnumerator stealth(float activeTime,float transparancy, float volumeMod)
    {
        GetComponent<PlayerAudio>().updateVolume(volumeModifier: volumeMod);
        RpcSetTransparentFox(transparancy);
        yield return new WaitForSeconds(activeTime);
        GetComponent<PlayerAudio>().updateVolume(volumeModifier: 1);
        RpcSetOrginalFox();
    }

    [Command]
    public void CmdCancelStealth()
    {
        RpcSetOrginalFox();
    }

    [ClientRpc]
    private void RpcSetTransparentFox(float transparancy){
        if (isLocalPlayer && transparancy < 0.1f)
            transparancy = 0.1f;
        
        Color alpha;
      
        foreach(SkinnedMeshRenderer smr in transform.GetComponentsInChildren<SkinnedMeshRenderer>()) {
            foreach(Material mat in smr.materials) {
                alpha = mat.color;
                alpha.a = transparancy;
                mat.SetColor("_Color", alpha);
                mat.renderQueue = 3100;
            }
        }
    }

    [ClientRpc]
    public void RpcSetOrginalFox()
    {
        Color alpha;
        float original = 1.0f;

        foreach (SkinnedMeshRenderer smr in transform.GetComponentsInChildren<SkinnedMeshRenderer>()) {
            foreach (Material mat in smr.materials) {
                alpha = mat.color;
                alpha.a = original;
                mat.SetColor("_Color", alpha);
                mat.renderQueue = 200;
            }
        }
    }
    /////////////////////////////////////////////////////////////////

    ///////////// Functions for GrenadePoop ability /////////////////


    [Command]
    public void CmdPoopGrenade(Vector3 direction, Vector3 startVel, int id) {
        GameObject poop = Instantiate(this._poopGrenade);
        GrenadePoopProjectile poopScript = poop.GetComponent<GrenadePoopProjectile>();
        PlayerAttack attackScript = poop.GetComponent<PlayerAttack>();
        Vector3 position = (transform.position + direction * 5.0f);

        poopScript.ConnectionID = id;   // Assign the player connection ID to the projectile.
        poopScript.shoot(direction, position, startVel);
        poopScript.owner = this.gameObject;
        attackScript.owner = this.gameObject;

        NetworkServer.Spawn(poop);
    }

    [Command]
    public void CmdPoopExplosion(Vector3 pos) {
        GameObject explosion = Instantiate(this._explosion);
        explosion.transform.position = pos;
        NetworkServer.Spawn(explosion);
        Destroy(explosion, 1.1f);
    }
    /////////////////////////////////////////////////////////////////

    ///////////// Functions for DustStorm ability ///////////////////
    [Command]
    public void CmdDustStorm(Vector3 pos, int id) {
        GameObject dustStorm = Instantiate(this._dustParticles);
        dustStorm.transform.position = pos;
        NetworkServer.Spawn(dustStorm);
        Destroy(dustStorm, 10.0f);
        RpcBlind(pos, id);
    }

    [ClientRpc]
    private void RpcBlind(Vector3 pos, int id) {
        if(GetComponent<BirdController>())      // TODO something else here for other classes? (could just spin their model around to emulate the flapping or something)
            StartCoroutine(GetComponent<BirdController>().flapLikeCrazy());

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player.GetComponent<PlayerInformation>().ConnectionID != id) {
            if (Vector3.Distance(player.transform.position, pos) < 20 && !player.GetComponent<PlayerHealth>().IsDead()) {
                StartCoroutine(player.GetComponent<PlayerEffects>().blind());
            }
        }
    }


    ///////////// Functions for DustTornado ability /////////////////
    [Command]
    public void CmdDustTornado(Vector3 pos, Vector3 dir, GameObject owner) {
        if(GetComponent<BirdController>())      // TODO something else here for other classes? (could just spin their model around to emulate the flapping or something)
            StartCoroutine(GetComponent<BirdController>().flapLikeCrazy());
        GameObject dustTornado = Instantiate(this._dustTornado);
        dustTornado.transform.position = pos;
        dustTornado.GetComponent<DustTornado>().shoot(pos, dir, owner);
        NetworkServer.SpawnWithClientAuthority(dustTornado, owner.GetComponent<PlayerInformation>().connectionToClient);
    }
    /////////////////////////////////////////////////////////////////

    /////////////////////// Functiuons for SuperSpeed ///////////////
    public void SuperSpeed(bool active) {
        Transform damageArea = this.transform.GetChild(3);

        if (gameObject != null)
            damageArea.GetComponent<CapsuleCollider>().enabled = active;

        if (this.isServer)
            this.RpcSuperSpeed(active);
        else if (this.isClient)
            this.CmdSuperSpeed(active);
    }

    [ClientRpc]
    private void RpcSuperSpeed(bool active) {
        this._fireFart.SetActive(active);
    }

    [Command]
    public void CmdSuperSpeed(bool active) {
        this.RpcSuperSpeed(active);
    }
    //////////////////////////////////////////////////////////////////
}
