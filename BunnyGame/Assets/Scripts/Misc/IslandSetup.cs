using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandSetup : MonoBehaviour {
    List<Material> _materials;
    Transform _fireWall;

	// Use this for initialization
	void Awake () {
        this._fireWall = null;
        this._materials = new List<Material>();
        Shader IslandShader = Resources.Load<Shader>("Shaders/Island");
        foreach (Transform t in transform) {
            t.gameObject.AddComponent<MeshCollider>();
            var renderer = t.gameObject.GetComponent<MeshRenderer>();
            var mats = renderer.materials;
            foreach (var mat in mats) {
                Color clr = mat.color;
                mat.shader = IslandShader;
                mat.color = clr;
                if (newMaterial(mat))
                    this._materials.Add(mat);
            }
        }
    }

    void Update() {
        if (this._fireWall == null)
            tryGetFireWall();
        else {
            foreach (var mat in this._materials) {
                mat.SetVector("_FireWallPos", getFireWallPos());
                mat.SetFloat("_FireWallRadius", getFireWallRadius());
            }
        }
    }

    private Vector4 getFireWallPos() {
        Vector3 p = this._fireWall.transform.position;
        return new Vector4(p.x, p.y, p.z, 0);
    }

    private float getFireWallRadius() {
        return this._fireWall.transform.localScale.x / 2;
    }

    private bool newMaterial(Material material) {
        foreach (var mat in this._materials) {
            if (mat == material)
                return false;
        }
        return true;
    }

    private void tryGetFireWall() {
        GameObject obj = GameObject.FindGameObjectWithTag("FireWall");
        if (obj != null) this._fireWall = obj.transform;
    }
}
