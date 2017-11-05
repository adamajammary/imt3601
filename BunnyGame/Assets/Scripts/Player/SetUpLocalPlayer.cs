using UnityEngine.Networking;

public class SetUpLocalPlayer : NetworkBehaviour {

    // Use this for initialization
    void Start () {
        if (this.isLocalPlayer) {
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();

            // Setting bunny offset target if BunnyController exists
            BunnyController bunnyContr = GetComponent<BunnyController>();

            if (camera != null) {
                if (!bunnyContr)
                    camera.SetTarget(this.transform);
                else
                    camera.SetTarget(this.transform.GetChild(2));
            }
            this.tag = "Player";
            transform.GetChild(0).gameObject.SetActive(true);
        } else
            this.tag = "Enemy";        
    }
}
