using UnityEngine;

public class FireWall : MonoBehaviour {
    private Texture2D _ft;
    private Material _fs;
    private float _noiseSeed = 0;
    private float _noiseSpeed = 1.25f;

	// Use this for initialization
	void Start () {
        this._fs = GetComponent<Renderer>().material;
        this._ft = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        this._fs.mainTexture = this._ft;
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
