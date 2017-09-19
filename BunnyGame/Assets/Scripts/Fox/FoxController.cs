using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FoxController : NetworkBehaviour {
    GameObject biteArea;

    // Use this for initialization
    void Start()
    {
        if (!this.isLocalPlayer)
            return; 

        PlayerController playerController = GetComponent<PlayerController>();
        playerController.runSpeed = 15;

        biteArea = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
        biteArea.transform.SetParent(transform);
        biteArea.transform.localPosition = new Vector3(0, 0.03f, 0.34f);
        biteArea.name = "FoxBiteHitbox";
        biteArea.tag = "projectile"; // Temporarily, should preferrably have its own tag so that it can deal a different amout of damage than bunnypoop. (see PlayerController.OnCollisionEnter()
        biteArea.GetComponent<BoxCollider>().enabled = false;
        biteArea.transform.localScale = Vector3.zero;
        NetworkServer.Spawn(biteArea);
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            StartCoroutine(this.bite());
        }
    }

    // Biting is enabled for 1 tick after called
    private IEnumerator bite()
    {
        biteArea.GetComponent<BoxCollider>().enabled = true;
        biteArea.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        yield return 0;
        biteArea.GetComponent<BoxCollider>().enabled = false;
        biteArea.transform.localScale = Vector3.zero;
    }
}
