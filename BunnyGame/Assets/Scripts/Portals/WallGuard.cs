using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WallGuard : NetworkBehaviour {

    public GameObject trail;
    public BoxCollider[] safeZones; 

    private GameObject _player;
    private float _wallTimer;
    private bool _movingPlayer;

	// Use this for initialization
	void Start () {
        this._wallTimer = 0;
        this._movingPlayer = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (this._player == null) {
            tryGetPlayer();
        } else {
            if (onWall() && !this._movingPlayer) {
                _wallTimer += Time.deltaTime;
                if (_wallTimer > 1.0f && !this._movingPlayer)
                    StartCoroutine(movePlayerOffWall());
            } else {
                _wallTimer = 0;
            }                
        }
	}

    private IEnumerator movePlayerOffWall() {
        this._movingPlayer = true;
        this._player.GetComponent<PlayerEffects>().CmdAddTrail(1.0f);
       
        const float speed = 1.0f;
        float t = 0;

        Vector3[] spline = new Vector3[3];
        spline[0] = this._player.transform.position;
        spline[2] = getTarget();
        spline[1] = Vector3.Lerp(spline[0], spline[2], 0.5f);
        spline[1].y += 20;

        while (t < 1) {
            this._player.GetComponent<PlayerController>().velocityY = 0;
            this._player.transform.position = getSplinePos(spline, t);
            t += Time.deltaTime * speed;
            yield return 0;
        }
        this._movingPlayer = false;
    }

    private Vector3 getTarget() {
        WorldGrid.Cell target;

        for (float dist = 1; dist < 500; dist++) {
            for (float rad = 0; rad < Mathf.PI * 2; rad += Mathf.PI / 8) {
                target = WorldData.worldGrid.getCell(this._player.transform.position + new Vector3(Mathf.Cos(rad), 1, Mathf.Sin(rad)) * dist);
                if (!target.blocked)
                    return target.pos;
            }
        }
        return Vector3.zero;
    }

    private Vector3 getSplinePos(Vector3[] spline, float t) {
        Vector3 t1 = Vector3.Lerp(spline[0], spline[1], t);
        Vector3 t2 = Vector3.Lerp(spline[1], spline[2], t);
        return Vector3.Lerp(t1, t2, t);
    }

    void tryGetPlayer() { //Due to networking, this is needed because this gameobject will spawn before the player
        this._player = GameObject.FindGameObjectWithTag("Player");
    }


    private bool onWall() {
        if (this._player.transform.position.y > 15 || this._player.transform.position.y < 6)
            return false;

        if (this._player.GetComponent<PlayerController>().getCC())
            return false;

        foreach(var zone in safeZones) {
           if (zone.bounds.Contains(this._player.transform.position))
                return false;
        }

        return true;
    }
}
