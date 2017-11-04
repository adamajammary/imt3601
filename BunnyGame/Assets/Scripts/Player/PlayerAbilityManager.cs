using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAbilityManager : NetworkBehaviour {
    public List<SpecialAbility> abilities = new List<SpecialAbility>();

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < abilities.Count && i < 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                StartCoroutine(abilities[i].useAbility());
            }
        }
    }

    // Called when the player kills another player
    [ClientRpc]
    public void RpcStealAbility(string[] abilitynames, int id) {
        Debug.Log("recieved abilities");
        if (id != GetComponent<PlayerInformation>().playerControllerId)
            return;

        Debug.Log("*Stealing Ability*");
    }

    [Command]
    public void CmdSendAbilitiesToKiller(int killerID) {
        List<string> abilitynames = new List<string>();
        foreach (SpecialAbility ability in abilities)
            abilitynames.Add(ability.abilityName);
        RpcStealAbility(abilitynames.ToArray(), killerID);
        Debug.Log("sending abilities");
    }
}
