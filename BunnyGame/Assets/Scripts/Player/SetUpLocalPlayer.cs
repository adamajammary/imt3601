using UnityEngine.Networking;

public class SetUpLocalPlayer : NetworkBehaviour {

    // Use this for initialization
    void Start () {
        if (this.isLocalPlayer) {
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();

            if (camera != null)
                camera.SetTarget(this.transform);

            this.tag = "Player";
            transform.GetChild(0).gameObject.SetActive(true);
        } else
            this.tag = "Enemy";
        
    }
}
