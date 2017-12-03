using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// TODO - Move deathmatch managment into another class
public class ServerSetup : NetworkBehaviour {
    private RectTransform _wallTransitionUI;  //The little onscreen bar indicating when the wall will shrink
    private const float _DeathmatchLen = 60 * 5; //5 mins
    [SyncVar]
    private float _deathmatchTimer = 0;

    // Use this for initialization
    void Start () {
		if (this.isServer) {
            StartCoroutine(spawnGI());         
        }
        StartCoroutine(init());
    }

    private IEnumerator spawnGI() {
        GameObject player = null;
        do {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return 0;
        } while (player == null);
        GameObject gimanager = Resources.Load<GameObject>("Prefabs/GameInfoManager");
        gimanager = Instantiate(gimanager);
        //NetworkServer.SpawnWithClientAuthority(gimanager, player.GetComponent<PlayerInformation>().connectionToServer);
        NetworkServer.Spawn(gimanager);
    }

    private IEnumerator init() {
        while (!GameInfo.ready) yield return 0;
        if (this.isServer) {
            Debug.Log("CUUCK");
            if (GameInfo.gamemode == "Battleroyale") {
                GameObject fireWall = Resources.Load<GameObject>("Prefabs/FireWall");
                fireWall = Instantiate(fireWall);
                NetworkServer.Spawn(fireWall);
            }

            GameObject npcManager = Resources.Load<GameObject>("Prefabs/NPCManager");
            npcManager = Instantiate(npcManager);
            NetworkServer.Spawn(npcManager);

            GameObject island = Resources.Load<GameObject>("Prefabs/Islands/" + GameInfo.map);
            island = Instantiate(island);
            NetworkServer.Spawn(island);
        }

        if (GameInfo.gamemode == "Deathmatch") {
            this._wallTransitionUI = GameObject.Find("wallTransitionUI").GetComponent<RectTransform>();
        }
    }


    void Update() {
        if (GameInfo.gamemode == "Deathmatch") {
            if (this.isServer) {
                this._deathmatchTimer += Time.deltaTime;
                if (this._deathmatchTimer > _DeathmatchLen) {
                    Object.FindObjectOfType<NetworkPlayerSelect>().deathmatchOver();
                }
            }
            if (_wallTransitionUI != null)
                _wallTransitionUI.anchorMax = new Vector2(this._deathmatchTimer / _DeathmatchLen, 1);
        }
    }
}
