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

    void Start() {
        this._poopGrenade = Resources.Load<GameObject>("Prefabs/PoopGrenade/PoopGrenade");
        this._explosion = Resources.Load<GameObject>("Prefabs/PoopGrenade/PoopExplosion");
        this._dustParticles = Resources.Load<GameObject>("Prefabs/BirdSpecial/DustStorm");
        this._dustTornado = Resources.Load<GameObject>("Prefabs/BirdSpecial/DustTornado");
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

    [ClientRpc]
    private void RpcSetTransparentFox(float transparancy)
    {
        Material[] materials;
        Color alfa;
      
        foreach (Transform child in this.transform.GetChild(modelChildNum))
        {
			if(child.gameObject.GetComponent<Renderer>() != null)
				materials = child.gameObject.GetComponent<Renderer>().materials;
			else if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
				materials = child.gameObject.GetComponent<SkinnedMeshRenderer>().materials;
			else
				continue;

			int count = 0;
            foreach (Material mat in materials)
            {
				alfa = mat.color;
                alfa.a = transparancy;
                materials[count++].SetColor("_Color", alfa);

                mat.renderQueue = 3100;
            }
        }
    }

    [ClientRpc]
    public void RpcSetOrginalFox()
    {
        Material[] materials;
        Color alfa;
        float orginal = 1.0f;
        
        foreach (Transform child in this.transform.GetChild(modelChildNum)) {
			if (child.gameObject.GetComponent<Renderer>() != null)
				materials = child.gameObject.GetComponent<Renderer>().materials;
			else if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
				materials = child.gameObject.GetComponent<SkinnedMeshRenderer>().materials;
			else
				continue;
			int count = 0;
            foreach (Material mat in materials) {
				alfa = mat.color;
                alfa.a = orginal;
                mat.renderQueue = 2000;
                materials[count++].SetColor("_Color", alfa);
			}
        }
    }
    /////////////////////////////////////////////////////////////////

    ///////////// Functions for GrenadePoop ability /////////////////


    [Command]
    public void CmdPoopGrenade(Vector3 direction, Vector3 startVel, int id) {
        GameObject poop = Instantiate(this._poopGrenade);
        GrenadePoopProjectile poopScript = poop.GetComponent<GrenadePoopProjectile>();
        Vector3 position = (transform.position + direction * 5.0f);

        poopScript.ConnectionID = id;   // Assign the player connection ID to the projectile.
        poopScript.shoot(direction, position, startVel);
        poopScript.owner = this.gameObject;

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
        StartCoroutine(GetComponent<BirdController>().flapLikeCrazy());
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player.GetComponent<PlayerInformation>().ConnectionID != id) {
            if (Vector3.Distance(player.transform.position, pos) < 20) {
                StartCoroutine(player.GetComponent<PlayerEffects>().blind());
            }
        }        
    }


    ///////////// Functions for DustTornado ability /////////////////
    [Command]
    public void CmdDustTornado(Vector3 pos, Vector3 dir, GameObject owner) {
        StartCoroutine(GetComponent<BirdController>().flapLikeCrazy());
        GameObject dustTornado = Instantiate(this._dustTornado);
        dustTornado.transform.position = pos;
        dustTornado.GetComponent<DustTornado>().shoot(pos, dir, owner);
        NetworkServer.SpawnWithClientAuthority(dustTornado, owner.GetComponent<PlayerInformation>().connectionToClient);
    }
    /////////////////////////////////////////////////////////////////
}
