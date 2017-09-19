using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireWall : MonoBehaviour {
    public RectTransform wallTransitionUI;       //The little onscreen bar indicating when the wall will shrink
    public Image         outsideWallEffect;      //A red transparent UI panel indicating that the player is outside the wall

    private const float _noiseSpeed = 1.25f;     //The rate at which the seed changes for perlin   
    private const float _wallShrinkTime = 15.0f; //Time in seconds between _wall shrinking
    private const float _wallShrinkRate = 0.15f; //The rate at which the wall shrinks

    private Texture2D   _ft;                //ft = fire texture
    private Material    _fs;                //fs = fireshader
    private Circle      _current;           //The current circle
    private Circle      _target;            //The target circle
    private float       _noiseSeed;         //seed for perlin
    private float       _wallShrinkTimer;   //Timer for when to shrink _wall   
    private bool        _wallIsShrinking;   //Keeps track of wheter or not the wall is shrinking

    // Use this for initialization
    void Start () {
        this._fs = GetComponent<Renderer>().material;
        this._ft = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        this._fs.mainTexture = this._ft;

        this._current = new Circle(250, Vector3.zero);
        this._target = new Circle(250, Vector3.zero);
        this._wallIsShrinking = false;
        this._noiseSeed = 0;
        this._wallShrinkTimer = 0;   
    }

    // Update is called once per frame
    void Update() {
        this.generateWallTexture();

        if (this._wallShrinkTimer > _wallShrinkTime) {
            this.recalculateWalls();
            StartCoroutine(interpolateWall());
            this._wallShrinkTimer = 0;
        }
        if (!this._wallIsShrinking) {
            this._wallShrinkTimer += Time.deltaTime;
            this.UpdateWallUI();
        }
    }

    private void UpdateWallUI() {
        wallTransitionUI.sizeDelta = new Vector2(150 * this._wallShrinkTimer / _wallShrinkTime, 10);
    }

    // Calculates a new target wall, sets current wall to last target
    private void recalculateWalls() {
        this._current = this._target;

        float targetRadius = this._current._radius / 2.0f;
        float angle = Random.Range(0, 1) * Mathf.PI * 2;
        float currentWallOffset = Random.Range(0, this._current._radius - targetRadius);
        Vector3 targetPos = this._current._pos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * currentWallOffset;

        this._target = new Circle(targetRadius, targetPos);
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
        this._noiseSeed += _noiseSpeed * Time.deltaTime;
    }

    // Transitions the wall from current state to target state
    private IEnumerator<bool> interpolateWall() {
        float t = 0;
        this._wallIsShrinking = true;

        while (t <= 1) {
            transform.position = Vector3.Lerp(_current._wall.transform.position, _target._wall.transform.position, t);
            transform.localScale = Vector3.Lerp(_current._wall.transform.localScale, _target._wall.transform.localScale, t);

            t += _wallShrinkRate * Time.deltaTime;
            yield return false;
        }
        this._wallIsShrinking = false;
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

    void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            outsideWallEffect.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            outsideWallEffect.enabled = false;
        }
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

