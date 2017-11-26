using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVAnimation : MonoBehaviour {

    public int uvTileY = 4;
    public int uvTileX = 4;

    public int fps = 30;

    private int index;

    private Vector2 size;
    private Vector2 offset;

    private Renderer renderer;

	// Use this for initialization
	void Start () {
        renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {

        // calculate the index
        index = (int)(Time.deltaTime * fps);

        // repeat when exhausted all frames
        index = index % (uvTileY * uvTileX);

        // size of each tile
        size = new Vector2(1.0f / uvTileY, 1.0f / uvTileX);

        // split into horizontal and vertical indexes
        var uIndex = index % uvTileX;
        var vIndex = index / uvTileX;

        //v cordinate is at the bottom of the image in openGL, so we invertt it
        offset = new Vector2(uIndex * size.x, 1.0f - size.y - vIndex * size.y);

        renderer.material.SetTextureOffset("_MainTex", offset);
        renderer.material.SetTextureScale("_MainTex", size);
	}
}
