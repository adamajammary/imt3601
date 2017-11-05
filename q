[1mdiff --git a/BunnyGame/Assets/Scripts/Abilities/SuperSpeed/SpeedBomb.cs b/BunnyGame/Assets/Scripts/Abilities/SuperSpeed/SpeedBomb.cs[m
[1mindex 1955481..7480f9a 100644[m
[1m--- a/BunnyGame/Assets/Scripts/Abilities/SuperSpeed/SpeedBomb.cs[m
[1m+++ b/BunnyGame/Assets/Scripts/Abilities/SuperSpeed/SpeedBomb.cs[m
[36m@@ -16,7 +16,7 @@[m [mpublic class SpeedBomb : SpecialAbility{[m
 [m
 public void init(float speed, float time)[m
     {[m
[31m-        base.init("Textures/AbilityIcons/runfast");[m
[32m+[m[32m        base.init("Textures/AbilityIcons/headbutt");[m
         base.abilityName = "SpeedBomb";[m
         this._speed = speed;[m
         this._time = time;[m
[1mdiff --git a/BunnyGame/Assets/Scripts/Player/PlayerAbilityManager.cs b/BunnyGame/Assets/Scripts/Player/PlayerAbilityManager.cs[m
[1mindex 42141be..f01d804 100644[m
[1m--- a/BunnyGame/Assets/Scripts/Player/PlayerAbilityManager.cs[m
[1m+++ b/BunnyGame/Assets/Scripts/Player/PlayerAbilityManager.cs[m
[36m@@ -23,19 +23,8 @@[m [mpublic class PlayerAbilityManager : NetworkBehaviour {[m
         }[m
     }[m
 [m
[31m-    public void stealAbility(int id, string[] theirAbilities)[m
[31m-    {[m
[31m-        //Debug.Log("stealAbility: " + id + ", " + GetComponent<PlayerInformation>().ConnectionID + ", " + GetComponent<PlayerInformation>().playerName);[m
[31m-        //if (id != GetComponent<PlayerInformation>().ConnectionID) {[m
[31m-        //    Debug.Log(GetComponent<PlayerInformation>().playerName+":id not matching");[m
[31m-        //    return;[m
[31m-        //}[m
[31m-        if (!isLocalPlayer) {[m
[31m-            Debug.Log(GetComponent<PlayerInformation>().playerName+":not local player");[m
[31m-            return;[m
[31m-        }[m
[31m-[m
 [m
[32m+[m[32m    public void killReward(string[] theirAbilities) {[m
         List<string> yourAbilities = new List<string>();[m
         foreach (SpecialAbility ability in abilities)[m
             yourAbilities.Add(ability.abilityName);[m
[36m@@ -110,10 +99,11 @@[m [mpublic class PlayerAbilityManager : NetworkBehaviour {[m
     [ClientRpc][m
     public void RpcSendAbilitiesToKiller(int killerID, string[] abilitynames) {[m
         GameObject player = GameObject.FindGameObjectWithTag("Player");[m
[31m-        if(player.GetComponent<PlayerInformation>().ConnectionID == killerID)[m
[31m-            player.GetComponent<PlayerAbilityManager>().stealAbility(killerID, abilitynames);[m
[32m+[m[32m        if(player.GetComponent<PlayerInformation>().ConnectionID == killerID && player.GetComponent<PlayerInformation>().isLocalPlayer)[m
[32m+[m[32m            player.GetComponent<PlayerAbilityManager>().killReward(abilitynames);[m
     }[m
 [m
[32m+[m[32m    // Called on the player when they die[m
     public void sendAbilitiesToKiller(int killerID) {[m
         List<string> abilitynames = new List<string>();[m
         foreach (SpecialAbility ability in abilities)[m
