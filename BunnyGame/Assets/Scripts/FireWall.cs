using System.Collections.Generic;
using UnityEngine;

public class FireWall : MonoBehaviour {
    private Texture2D _ft;
    private Material _fs;
    private float _noiseSeed = 0;
    private float _noiseSpeed = 1.25f;
    private float _wallShrinkTime = 10.0f; //Time in seconds between _wall shrinking
    private float _wallShrinkTimer = 0.0f; //Timer for when to shrink _wall

    private Mesh _wall;
    private Circle _current;
    private Circle _target;
    private float _targetRadius = 250;

	// Use this for initialization
	void Start () {
        this._fs = GetComponent<Renderer>().material;
        this._ft = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        this._fs.mainTexture = this._ft;

        this._current = new Circle(250, Vector3.zero);
        this._target = new Circle(250, Vector3.zero);
   
        this._wall = GetComponent<Mesh>();

        
    }
	
	// Update is called once per frame
	void Update () {
        this.generateWallTexture();

        if (this._wallShrinkTimer > this._wallShrinkTime) {
            this.recalculateWalls();
            StartCoroutine(interpolateWall());
            this._wallShrinkTimer = 0;
        }
        this._wallShrinkTimer += Time.deltaTime;

        Debug.Log(string.Format("Wall will shrink in : {0}", this._wallShrinkTime - this._wallShrinkTimer));
    }

    private void recalculateWalls() {
        this._current = this._target;
        this._targetRadius /= 2.0f;

        float angle = Random.Range(0, 1) * Mathf.PI * 2;
        float currentWallOffset = Random.Range(0, this._current._radius - this._targetRadius);
        Vector3 targetPos = this._current._pos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * currentWallOffset;
        this._target = new Circle(this._targetRadius, targetPos);
    }

    private void generateWallTexture() {
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
    }

    private IEnumerator<bool> interpolateWall() {
        float t = 0;

        while (t <= 1) {
            transform.position = Vector3.Lerp(_current._wall.transform.position, _target._wall.transform.position, t);
            transform.localScale = Vector3.Lerp(_current._wall.transform.localScale, _target._wall.transform.localScale, t);

            t += 0.3f * Time.deltaTime;
            yield return false;
        }
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
    public float _radius;
    public Vector3 _pos;
    public GameObject _wall;

    public Circle(float radius, Vector3 pos) {
        this._radius = radius;
        this._pos = pos;

        this._wall = Resources.Load<GameObject>("Prefabs/WallShell");
        this._wall = MonoBehaviour.Instantiate(this._wall);
        this._wall.transform.position = pos;
        this._wall.transform.localScale = new Vector3(this._radius * 2, 500, this._radius * 2);
    }
}

