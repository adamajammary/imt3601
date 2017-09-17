using System.Collections.Generic;
using UnityEngine;

public class FireWall : MonoBehaviour {
    private Texture2D _ft;
    private Material _fs;
    private float _noiseSeed = 0;
    private float _noiseSpeed = 1.25f;

    Mesh wall;
    float t = 0;

    Circle start;
    Circle target;

	// Use this for initialization
	void Start () {
        this._fs = GetComponent<Renderer>().material;
        this._ft = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        this._fs.mainTexture = this._ft;

        start = new Circle(250, Vector3.zero);
        target = new Circle(100, new Vector3(50, 0, 50));
   
        wall = GetComponent<Mesh>();
    }
	
	// Update is called once per frame
	void Update () {
        //Sets every pixel in the firewall texture
        for (int y = 0; y < _ft.height; y++) {
            for (int x = 0; x < _ft.width; x++) {
                float n = this.noise(x, y);
                this._ft.SetPixel(x, y, new Color(1, 0.12f, 0, n));
            }
        }
        // Apply all pixel changes to the texture
        this._ft.Apply();        
        // This will change where noise is sampled from the noise plane
        this._noiseSeed += this._noiseSpeed * Time.deltaTime;

        interpolateWall();  
    }

    void interpolateWall() {
        if (t >= 1)
            return;

        transform.position = Vector3.Lerp(start.getWall().transform.position, target.getWall().transform.position, t);
        transform.localScale = Vector3.Lerp(start.getWall().transform.localScale, target.getWall().transform.localScale, t);

        t += 0.1f * Time.deltaTime;
    }

    // Generates values from 0.4-1.0 based on perlin noise
    private float noise(float x, float y) {
        float xSeed = 1337.0f + this._noiseSeed;
        float ySeed = 1337.0f + this._noiseSeed;
        float xDivider = 0.7f;
        float yDivider = 9.7f;
        float n = Mathf.PerlinNoise(x / xDivider + xSeed, y / yDivider + ySeed);
        if (n < 0.4f)
            n = 0.4f;
        return n;
    }
}

class Circle {
    private float _radius;
    private Vector3 _pos;
    private GameObject _wall;

    public Circle(float radius, Vector3 pos) {
        this._radius = radius;
        this._pos = pos;

        this._wall = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        this._wall.transform.position = pos;
        this._wall.transform.localScale = new Vector3(this._radius * 2, 500, this._radius * 2);
    }

    public GameObject getWall() {
        return this._wall;
    }
}

