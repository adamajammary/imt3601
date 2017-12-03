using UnityEngine;
using UnityEngine.Networking;

public class PlayerAttack : NetworkBehaviour {
    [SyncVar]
    public GameObject owner;

    // Triggers when the weapon/attack-collider collides with an enemy player character.
    // We are the attacker, while the enemy is the victim.
    public void OnTriggerEnter(Collider other) {
        PlayerInformation attackerInfo = owner.GetComponent<PlayerInformation>();
        PlayerInformation victimInfo   = other.GetComponent<PlayerInformation>();
        int               attackerID   = (attackerInfo != null ? attackerInfo.ConnectionID : -1);
        int               victimID     = (victimInfo   != null ? victimInfo.ConnectionID   : -1);
        PlayerHealth      victimHealth = other.GetComponent<PlayerHealth>();

        // PLAYER VS. ENEMY
        if (other.gameObject.tag == "Enemy") {
            if ((attackerID < 0) || (victimID < 0) || (victimHealth == null) || !other.transform.GetChild(1).gameObject.activeInHierarchy)
                return;

            // BIRD
            if (this.gameObject.tag == "pecker") {
                pecker birdPecker = this.GetComponent<pecker>();

                if (birdPecker != null)
                    victimHealth.Attack(birdPecker.GetDamage(), owner, attackerID, victimID, other.gameObject.transform.position);
            // BUNNY
            } else if (this.gameObject.tag == "projectile") {
                BunnyPoop bunnyPoop = this.GetComponent<BunnyPoop>();

                if (bunnyPoop != null)
                    victimHealth.Attack(bunnyPoop.GetDamage(), owner, attackerID, victimID, other.gameObject.transform.position);

                NetworkServer.Destroy(this.gameObject);
            // FOX
            } else if ((this.gameObject.tag == "foxbite") && (other.gameObject.tag == "Enemy")) {
                FoxController foxCtrl = owner.GetComponent<FoxController>();

                if (foxCtrl != null)
                    victimHealth.Attack(foxCtrl.GetDamage(), owner, attackerID, victimID, foxCtrl.biteImpact());
            // MOOSE
            } else if ((this.gameObject.tag == "mooseAttack") && (other.gameObject.tag == "Enemy")) {
                MooseController mooseCtrl = owner.GetComponent<MooseController>();

                if (mooseCtrl != null)
                    victimHealth.Attack(mooseCtrl.GetDamage(), owner, attackerID, victimID, mooseCtrl.ramImpact());
            }
        // PLAYER VS. NPC
        } else if (other.gameObject.tag == "npc") {
            PlayerHealth  playerHealth  = owner.GetComponent<PlayerHealth>();
            PlayerEffects playerEffects = owner.GetComponent<PlayerEffects>();
            NPC           npc           = other.GetComponent<NPC>();

            if ((npc == null) || npc.IsDead)
                return;

            npc.IsDead = true;
            playerEffects.CmdBloodParticle(other.transform.position);

            switch (npc.type) {
                case "whale":   playerEffects.CmdAddToughness(0.05f);  break;
                case "cat":     playerEffects.CmdAddDamage(0.05f);     break;
                case "dog":     playerEffects.CmdAddSpeed(0.05f);      break;
                case "eagle":   playerEffects.CmdAddJump(0.05f);       break;
                case "chicken": playerHealth.Heal(10.0f);              break;
                default:        Debug.Log("ERROR! Unknown NPC type."); break;
            }

            other.gameObject.GetComponent<NPC>().die();
        }
    }
}
