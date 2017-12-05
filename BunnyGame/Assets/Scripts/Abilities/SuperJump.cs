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
        base.init("Textures/AbilityIcons/jumphigh");
        _jumpHeight = jumpHeight;
        base.abilityName = "SuperJump";
    }

    override public IEnumerator useAbility() {
        PlayerController playerController = GetComponent<PlayerController>();
        if (base._cooldown > 0 || !playerController.controller.isGrounded)
            yield break;

        float oldHeight = playerController.jumpHeight;
        playerController.jumpHeight = _jumpHeight;
        while (!playerController.controller.isGrounded) {
            yield return null;
        }
        if (playerController.jump()) StartCoroutine(base.doCoolDown());
        playerController.jumpHeight = oldHeight;
    }
}
