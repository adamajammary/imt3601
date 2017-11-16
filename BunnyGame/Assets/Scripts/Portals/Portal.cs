using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    public GameObject magicTrail;
    private Portal _targetPortal;
    private Vector3[] _spline = new Vector3[3]; 

	// Use this for initialization
	void Awake () {
        magicTrail = Instantiate(magicTrail);	
	}

    private void Update() {
        Debug.DrawLine(this._spline[0], this._spline[1]);
        Debug.DrawLine(this._spline[1], this._spline[2]);
    }

    public void setTarget(Portal targetPortal) {
        this._targetPortal = targetPortal;

        Vector3 halfWay;
        this._spline[0] = transform.position;
        this._spline[2] = targetPortal.transform.position;
        halfWay = Vector3.Lerp(this._spline[2], this._spline[0], 0.5f);
        halfWay.y = (this._spline[2].y > this._spline[0].y) ? this._spline[2].y + 100 : this._spline[0].y + 100;
        this._spline[1] = halfWay;
        StartCoroutine(moveMagicTrail());
    }

    private IEnumerator moveMagicTrail() {
        const float speed = 0.5f;
        float t = 0;
        bool forward = true;
        while (true) {
            Vector3 t1 = Vector3.Lerp(this._spline[0], this._spline[1], t);
            Vector3 t2 = Vector3.Lerp(this._spline[1], this._spline[2], t);
            Vector3 pos = Vector3.Lerp(t1, t2, t);
            magicTrail.transform.position = pos;
            
            t += Time.deltaTime * speed * ((forward) ? 1 : -1);
            if (t >= 1) forward = false;
            if (t <= 0) forward = true;
            yield return 0;
        }
    }
}
