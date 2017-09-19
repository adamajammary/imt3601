using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireWall : MonoBehaviour {
    public RectTransform wallTransitionUI;       //The little onscreen bar indicating when the wall will shrink
    public Image         outsideWallEffect;      //A red transparent UI panel indicating that the player is outside the wall

    private const float _noiseSpeed = -1.25f;     //The rate at which the seed changes for perlin   
    private const float _wallShrinkTime = 15.0f; //Time in seconds between _wall shrinking
    private const float _wallShrinkRate = 0.1f; //The rate at which the wall shrinks

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

        this._noiseSeed = 0;
        this._wallShrinkTimer = 0;
        this._wallIsShrinking = false;
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

        float targetRadius = this._current.radius / 2.0f;
        float angle = Random.Range(0, 1) * Mathf.PI * 2;
        float currentWallOffset = Random.Range(0, this._current.radius - targetRadius);
        Vector3 targetPos = this._current.pos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * currentWallOffset;

        this._target = new Circle(targetRadius, targetPos);
    }

    private void generateWallTexture() {
        //Sets every pixel in the firewall texture
        for (int y = 0; y < _ft.height; y++) {
            for (int x = 0; x < _ft.width; x++) {
                float n = this.noise(x, y);
                if (n < 0.3f)
                    this._ft.SetPixel(x, y, new Color(0, 0, 0, 0.6f));
                else if (n < 0.5f)
                    this._ft.SetPixel(x, y, new Color(1, 0.55f, 0, 0.6f));
                else
                    this._ft.SetPixel(x, y, new Color(1, 0, 0, 0.6f));
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
            transform.position = Vector3.Lerp(_current.wall.transform.position, _target.wall.transform.position, t);
            transform.localScale = Vector3.Lerp(_current.wall.transform.localScale, _target.wall.transform.localScale, t);

            t += _wallShrinkRate * Time.deltaTime;
            yield return false;
        }
        this._wallIsShrinking = false;
    }

    // Generates values from 0.4-1.0 based on perlin noise
    private float noise(float x, float y) {
        float xRadius = transform.localScale.x / 2;
        float zRadius = transform.localScale.z / 2;
        float avgRadius = (xRadius + zRadius) / 2;

        float wallCircumference = Mathf.PI * 2 * avgRadius;

        float xSeed = 1337.0f + this._noiseSeed;
        float ySeed = 1337.0f + this._noiseSeed;
        float xDivider = 1144.07f / avgRadius;
        float yDivider = 31.7f;
        float n = Mathf.PerlinNoise(x / xDivider + xSeed, y / yDivider + ySeed);
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
    public float radius;
    public Vector3 pos;
    public GameObject wall;

    public Circle(float radius, Vector3 pos) {
        this.radius = radius;
        this.pos = pos;

        this.wall = Resources.Load<GameObject>("Prefabs/WallShell");
        this.wall = MonoBehaviour.Instantiate(this.wall);
        this.wall.transform.position = pos;
        this.wall.transform.localScale = new Vector3(this.radius * 2, 500, this.radius * 2);
    }

    ~Circle() {
        MonoBehaviour.Destroy(wall);
    }
}

