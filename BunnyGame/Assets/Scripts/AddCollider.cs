using UnityEngine;

public class AddCollider : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
        foreach (Transform childTransform in this.transform)
        {
            childTransform.gameObject.AddComponent<MeshCollider>();
            childTransform.GetComponent<MeshCollider>().isTrigger = true;
            childTransform.gameObject.tag                         = this.tag;
        }
    }
}
