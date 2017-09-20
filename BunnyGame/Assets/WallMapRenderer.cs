using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMapRenderer : MonoBehaviour {
    private LineRenderer _lr;
    const int _pointsCount = 50;
    

    void Start() {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = _pointsCount;
    }

    public void draw(Transform wall) {
        Vector3[] points = new Vector3[_pointsCount];
        int index = 0;
        for (float rads = 0; rads < Mathf.PI * 2; rads += Mathf.PI * 2 / _pointsCount) {
            Vector3 dir = new Vector3(Mathf.Cos(rads), 0, Mathf.Sin(rads));
            float weightedRadius = Vector3.Scale(dir, wall.localScale).magnitude / 2;
            points[index++] = wall.position + dir * weightedRadius + new Vector3(0, 150, 0);
        }
        this._lr.SetPositions(points);
    }
}
