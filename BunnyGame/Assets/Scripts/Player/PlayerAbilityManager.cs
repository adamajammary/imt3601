using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAbilityManager : NetworkBehaviour {

    public List<SpecialAbility> abilities = new List<SpecialAbility>();

    private AbilityPanel display;
    private PlayerController _playerController;

    // Use this for initialization
    void Start () {
        display = GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>();
        this._playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update () {

        for (int i = 0; i < abilities.Count && i < 9; i++) {
            //if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && !this._playerController.getCC()) {
                StartCoroutine(abilities[i].useAbility());
            }
        }
    }


    public void killReward(string[] theirAbilities) {
        List<string> yourAbilities = new List<string>();
        foreach (SpecialAbility ability in abilities)
            yourAbilities.Add(ability.abilityName);

        string[] newAbilities = theirAbilities.Except(yourAbilities).ToArray<string>();

        // If the player you killed had abilities you didn't have and you haven't maxed out on the number of abilities
        if (newAbilities.Length > 0 && abilities.Count < 10) {
            int index = Random.Range(0, newAbilities.Length);
            Debug.Log("Trying to steal ability " + newAbilities[index]);
            stealNewAbility(newAbilities[index]);
        }
        else {
            /*
             * Not sure what to do if a player has all the abilities of who they kill.
             * Possibilities:
             *     Nothing
             *     Upgradeds to the abilities
             *     Attributes
             */
            Debug.Log("Nothing to steal");
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
                ((SuperJump)sa).init(10);
                break;
            case "DustTornado":
                sa = gameObject.AddComponent<DustTornadoAbility>();
                ((DustTornadoAbility)sa).init();
                break;
            case "GrenadePoop":
                sa = gameObject.AddComponent<GrenadePoop>();
                ((GrenadePoop)sa).init();
                break;
            case "SpeedBomb":  // Disabled because the ability doesn't currently work for all classes
                sa = gameObject.AddComponent<SpeedBomb>();
                ((SpeedBomb)sa).init(30, 2);
                break;
            case "Stomp":
                sa = gameObject.AddComponent<Stomp>();
                ((Stomp)sa).init();
                break;
            default:
                Debug.Log("Ability does not exist: \"" + abilityName + "\" (PlayerAbilityManager.cs:stealNewAbility())");
                return;
        }
        Debug.Log("Added:" + abilityName);
        abilities.Add(sa);
        display.setupPanel(this);
    }



    [Command]
    public void CmdSendAbilitiesToKiller(int killerID, string[] abilitynames) {
        RpcSendAbilitiesToKiller(killerID, abilitynames);
    }

    [ClientRpc]
    public void RpcSendAbilitiesToKiller(int killerID, string[] abilitynames) {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player.GetComponent<PlayerInformation>())
            return;
        if(player.GetComponent<PlayerInformation>().ConnectionID == killerID && player.GetComponent<PlayerInformation>().isLocalPlayer)
            player.GetComponent<PlayerAbilityManager>().killReward(abilitynames);
    }

    // Called on the player when they die
    public void sendAbilitiesToKiller(int killerID) {
        List<string> abilitynames = new List<string>();
        foreach (SpecialAbility ability in abilities)
            abilitynames.Add(ability.abilityName);

        if (isServer)
            RpcSendAbilitiesToKiller(killerID, abilitynames.ToArray());
        else if(isClient)
            CmdSendAbilitiesToKiller(killerID, abilitynames.ToArray());
    }
}
