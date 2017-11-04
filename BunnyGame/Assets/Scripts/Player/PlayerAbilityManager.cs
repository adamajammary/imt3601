using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAbilityManager : NetworkBehaviour {
    public List<SpecialAbility> abilities = new List<SpecialAbility>();
    private AbilityPanel display;

    // Use this for initialization
    void Start () {
        display = GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.O))
            stealNewAbility("DustStorm");

        for (int i = 0; i < abilities.Count && i < 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                StartCoroutine(abilities[i].useAbility());
            }
        }
    }

    // Called when the player kills another player
    [ClientRpc]
    public void RpcStealAbility(string[] theirAbilities, int id) {
        if (id != GetComponent<PlayerInformation>().playerControllerId || !isLocalPlayer)
            return;


        List<string> yourAbilities = new List<string>();
        foreach (SpecialAbility ability in abilities)
            yourAbilities.Add(ability.abilityName);

        string[] newAbilities = theirAbilities.Except(yourAbilities).ToArray<string>();


        // If the player you killed had abilities you didn't have
        if(newAbilities.Length > 0) {
            int index = Random.Range(0, newAbilities.Length);
            stealNewAbility(newAbilities[index]);
        } else { // If you have all the abilities the other player had
            int index = Random.Range(0, theirAbilities.Length);
            upgradeExistingAbility(newAbilities[index]);
        }
    }


    private void stealNewAbility(string abilityName) {
        SpecialAbility sa;
        switch (abilityName) {
            case "Sprint":
                sa = gameObject.AddComponent<Sprint>();
                ((Sprint)sa).init(35, 1);
                break;
            case "Stealth":
                sa = gameObject.AddComponent<Stealth>();
                ((Stealth)sa).init(1, 0);
                break;
            case "DustStorm":
                sa = gameObject.AddComponent<DustStorm>();
                ((DustStorm)sa).init();
                break;
            case "SuperJump":
                sa = gameObject.AddComponent<SuperJump>();
                ((SuperJump)sa).init(5);
                break;
            case "DustTornado":
                sa = gameObject.AddComponent<DustTornadoAbility>();
                ((DustTornadoAbility)sa).init();
                break;
            case "GrenadePoop":
                sa = gameObject.AddComponent<GrenadePoop>();
                ((GrenadePoop)sa).init();
                break;
            case "SpeedBomb":
                sa = gameObject.AddComponent<SpeedBomb>();
                ((SpeedBomb)sa).init(30, 2);
                break;
            default:
                Debug.Log("Ability does not exist: \"" + abilityName + "\" (PlayerAbilityManager.cs:stealNewAbility())");
                return;
        }
        abilities.Add(sa);
        display.setupPanel(this);
    }

    private void upgradeExistingAbility(string abilityName) {
        switch (abilityName) {
            case "Sprint": break;
            case "Stealth": break;
            case "DustStorm": break;
            case "SuperJump": break;
            case "DustTornado": break;
            case "GrenadePoop": break;
            case "SpeedBomb": break;
            default: return;

        }
    }



    [Command]
    public void CmdSendAbilitiesToKiller(int killerID) {
        List<string> abilitynames = new List<string>();
        foreach (SpecialAbility ability in abilities)
            abilitynames.Add(ability.abilityName);

        RpcStealAbility(abilitynames.ToArray(), killerID);
    }
}
