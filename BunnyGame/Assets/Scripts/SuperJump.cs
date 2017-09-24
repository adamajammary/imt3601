using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



/********************************************
 * Allows for the player to jump extra high
 *******************************************/
public class SuperJump : SpecialAbility {
    private float _jumpHeight;

    public void init(float jumpHeight) {
        base.init("Textures/AbilityIcons/test");
        _jumpHeight = jumpHeight;
        base.abilityName = "Super Jump";
    }

    override public IEnumerator useAbility() {
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0 || !playerController.controller.isGrounded)
            yield break;
        
        StartCoroutine(base.doCoolDown());

        float oldHeight = playerController.jumpHeight;
        playerController.jumpHeight = _jumpHeight;
        while (!playerController.controller.isGrounded) {
            yield return null;
        }
        playerController.jump();
        playerController.jumpHeight = oldHeight;
    }
}
