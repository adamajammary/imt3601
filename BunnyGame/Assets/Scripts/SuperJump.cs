using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

class SuperJump : SpecialAbility {


    override public void useAbility() {
        if (cooldown > 0)
            return;
        doCoolDown();

        // Do super jump things here

    }

}
