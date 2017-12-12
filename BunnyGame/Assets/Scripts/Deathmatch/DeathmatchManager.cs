using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DeathmatchManager : NetworkBehaviour {
    private RectTransform _wallTransitionUI;  //The Timer for the firewall, repurposed. 
    private const float _DeathmatchLen = 60 * 3; //3 mins
    [SyncVar]
    private float _deathmatchTimer = 0;

    // Use this for initialization
    void Start() {
        this._wallTransitionUI = GameObject.Find("wallTransitionUI").GetComponent<RectTransform>();
    }

    void Update() {
        if (this.isServer) {
            this._deathmatchTimer += Time.deltaTime;
            if (this._deathmatchTimer > _DeathmatchLen) {
                Object.FindObjectOfType<NetworkPlayerSelect>().deathmatchOver();
                Destroy(this.gameObject);
            }
        }
        if (_wallTransitionUI != null)
            _wallTransitionUI.anchorMax = new Vector2(this._deathmatchTimer / _DeathmatchLen, 1);
    }
}
