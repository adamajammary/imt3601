using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class FireWall : NetworkBehaviour {
    class Circle {
        public Vector3 _pos;
        private float _radius;
        public GameObject wall;

        public Circle(float radius, Vector3 pos) {
            this._radius = radius;
            this._pos = pos;

            this.wall = Resources.Load<GameObject>("Prefabs/WallShell");
            this.wall = MonoBehaviour.Instantiate(this.wall);
            this.wall.transform.position = pos;
            this.wall.transform.localScale = new Vector3(this.radius * 2, 300, this.radius * 2);
        }

        public Vector3 pos {
            get {
                return _pos;
            }
            set {
                _pos = value;
                wall.transform.position = value;
            }
        }

        public float radius {
            get {
                return _radius;
            }
            set {
                _radius = value;
                wall.transform.localScale = new Vector3(value * 2, 300, value * 2);
            }
        }
    }

    private const float _noiseSpeed     = -40.25f;   //The rate at which the seed changes for perlin   
    private const float _wallShrinkTime = 45.0f;//Time in seconds between _wall shrinking
    private const float _wallShrinkRate = 0.04f; //The rate at which the wall shrinks

    private WallMapRenderer _actualWallRenderer;//Renders the actual fire wall
    private WallMapRenderer _targetWallRenderer;//Renders the target fire wall
    private RectTransform   _wallTransitionUI;  //The little onscreen bar indicating when the wall will shrink
    private Image           _outsideWallEffect; //A red transparent UI panel indicating that the player is outside the wall
    private Material        _fs;                //fs = fireshader
    private Circle          _current;           //The current circle
    private Circle          _target;            //The target circle
    private System.Random   _RNG;               //Number generator, will be seeded the same across all clients
    [SyncVar]
    private int             _rngSeed;
    private float           _noiseSeed;         //seed for perlin
    private float           _wallShrinkTimer;   //Timer for when to shrink _wall   
    private bool            _wallIsShrinking;   //Keeps track of wheter or not the wall is shrinking

    // Use this for initialization
    void Start () {
        this._wallTransitionUI = GameObject.Find("wallTransitionUI").GetComponent<RectTransform>();
        this._outsideWallEffect = GameObject.Find("OutsideWallEffect").GetComponent<Image>();
        this._targetWallRenderer = GameObject.Find("TargetWallMapRenderer").GetComponent<WallMapRenderer>();
        this._actualWallRenderer = GameObject.Find("FireWallMapRenderer").GetComponent<WallMapRenderer>();

        this._fs = GetComponent<Renderer>().material;
        this._current = new Circle(250, Vector3.zero);
        this._target = new Circle(250, Vector3.zero);

        this._noiseSeed = 0;
        this._wallShrinkTimer = 0;
        this._wallIsShrinking = false;

        if (this.isServer)
            this._rngSeed = UnityEngine.Random.Range(0, 9999999);
        StartCoroutine(lateStart());
    }

    private IEnumerator lateStart() {
        yield return new WaitForSeconds(1.0f); //Wait one second for _rngSeed to sync (kinda hacky)
        this._RNG = new System.Random(this._rngSeed);
        this.recalculateWalls();
        this._targetWallRenderer.draw(this._target.wall.transform);
    }

    // Update is called once per frame
    void Update() {
        this.generateWallTexture();

        if (this._wallShrinkTimer > _wallShrinkTime) {
            StartCoroutine(interpolateWall());
            this._wallShrinkTimer = 0;
        }
        if (!this._wallIsShrinking) {
            this._wallShrinkTimer += Time.deltaTime;
            this.UpdateWallUI();
        }
        this._actualWallRenderer.draw(this.transform);
    }   

    private void UpdateWallUI() {
        _wallTransitionUI.sizeDelta = new Vector2(150 * this._wallShrinkTimer / _wallShrinkTime, 10);
    }

    // Calculates a new target wall, sets current wall to last target
    private void recalculateWalls() {
        Circle temp = this._current;
        this._current = this._target;
        this._target = temp;

        this._target.radius = this._current.radius / 2.0f;
        float angle = (float)_RNG.NextDouble() * Mathf.PI * 2;
        float currentWallOffset = (float)_RNG.NextDouble() * (this._current.radius - this._target.radius);
        this._target.pos = this._current.pos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * currentWallOffset;
    }

    private void generateWallTexture() {
        this._fs.SetFloat("_NoiseSeed", this._noiseSeed);
        // This will change where noise is sampled from the noise plane
        this._noiseSeed += _noiseSpeed * Time.deltaTime;
    }

    // Transitions the wall from current state to target state
    private IEnumerator interpolateWall() {
        float t = 0;
        this._wallIsShrinking = true;

        while (t <= 1) {
            transform.position = Vector3.Lerp(_current.wall.transform.position, _target.wall.transform.position, t);
            transform.localScale = Vector3.Lerp(_current.wall.transform.localScale, _target.wall.transform.localScale, t);

            t += _wallShrinkRate * Time.deltaTime;
            yield return 0;
        }
        this._wallIsShrinking = false;
        this.recalculateWalls();
        this._targetWallRenderer.draw(this._target.wall.transform);
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            _outsideWallEffect.enabled = true;
            other.GetComponent<PlayerEffects>().insideWall = false;
        }else if (other.tag == "Enemy") {
            other.GetComponent<PlayerEffects>().insideWall = false;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            _outsideWallEffect.enabled = false;
            other.GetComponent<PlayerEffects>().insideWall = true;
        } else if (other.tag == "Enemy") {
            other.GetComponent<PlayerEffects>().insideWall = true;
        }
    }
}