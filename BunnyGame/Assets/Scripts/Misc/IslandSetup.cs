using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandSetup : MonoBehaviour {
    List<Material> _materials;
    Shader IslandShader;
    Transform _fireWall;

	// Use this for initialization
	void Awake () {
        this._fireWall = null;
        this._materials = new List<Material>();
        IslandShader = Resources.Load<Shader>("Shaders/Island");
        traverseAndFix(this.transform);
    }

    void Update() {
        if (this._fireWall == null)
            tryGetFireWall();
        else {
            foreach (var mat in this._materials) {
                mat.SetVector("_FireWallPos", getFireWallPos());
                mat.SetFloat("_FireWallRadius", this._fireWall.GetComponent<FireWall>().getRadius());
            }
        }
    }

    private void traverseAndFix(Transform root) {
        foreach (Transform t in transform) {
            setupObject(t);
            foreach (Transform tt in t)
                setupObject(tt);
        }
    }

    private void setupObject(Transform t) {
        if (t.GetComponent<MeshFilter>() != null) {
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

    private Vector4 getFireWallPos() {
        Vector3 p = this._fireWall.transform.position;
        return new Vector4(p.x, p.y, p.z, 0);
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
