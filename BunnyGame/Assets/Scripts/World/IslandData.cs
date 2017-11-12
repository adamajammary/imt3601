using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandData : MonoBehaviour {

    public Transform[] connectPoints;
    public Transform[] avoidPoints;
    public string name;

    private int _cellCount;
    private int _worldSize;
    private float[] _yOffsets;

	// Use this for initialization
	void Awake () {
        turnOffPointsRendering();
        switch (name) {
            case "Island":
                IslandInit();
                break;
            case "Island42":
                Island42Init();
                break;
        }
        this._yOffsets = new float[connectPoints.Length];
        for (int i = 0; i < connectPoints.Length; i++)
            this._yOffsets[i] = connectPoints[i].position.y;
    }

    public int cellCount { get { return _cellCount; } }
    public int worldSize { get { return _worldSize; } }
    public float[] yOffsets { get { return _yOffsets; } }

    // Can switch this to loading from file in the future
    private void Island42Init() {
        this._cellCount = 150;
        this._worldSize = 400;        
    }

    private void IslandInit() {
        this._cellCount = 150;
        this._worldSize = 400;
    }

    private void turnOffPointsRendering() {
        foreach (Transform t in connectPoints) {
            foreach (Transform tt in t) {
                tt.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        foreach (Transform t in avoidPoints) {
            foreach (Transform tt in t) {
                tt.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}
