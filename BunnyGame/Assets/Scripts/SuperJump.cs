using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SuperJump : SpecialAbility {
    private float _jumpHeight;

    public void init(string imagePath, float jumpHeight){
        base.init("");
        _jumpHeight = jumpHeight;
    }

    override public IEnumerator useAbility() {
        Debug.Log("Supa Jump~~!");
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0 || !playerController.controller.isGrounded)
            yield break;
        
        base.doCoolDown();

        float oldHeight = playerController.jumpHeight;
        playerController.jumpHeight = _jumpHeight;
        while (!playerController.controller.isGrounded) {
            yield return null;
        }
        playerController.jump();
        playerController.jumpHeight = oldHeight;

    }
}
