using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NPCManager : NetworkBehaviour {
    [SyncVar]
    private int _playerCount = -1;

    private Dictionary<int, GameObject> _players;       //Used to update NPCWorldView
    private Dictionary<int, GameObject> _npcs;          //Used to update NPCWorldView
    private List<int> _deadPlayers;   //Keeps track of dead players, so that they can be removed from datastructures at a convenient time
    private List<int> _deadNpcs;      //Keeps track of dead npcs, so that they can be removed from datastructures at a convenient time
    private NPCThread _npcThread;     //The thread running the logic for NPCs using NPCWorldView maintained by this class
    private BlockingQueue<NPCThread.instruction> _instructions;  //Queue used to recieve instuctions from NPCThread
    private bool _ready;         //Flag set to true when initialization is finished

    // Use this for initialization
    void Start() {
        this._players = new Dictionary<int, GameObject>();
        this._npcs = new Dictionary<int, GameObject>();
        this._deadPlayers = new List<int>();
        this._deadNpcs = new List<int>();
        this._ready = false;
        if (this.isServer)
            StartCoroutine(waitForClients());
        StartCoroutine(init());
    }

    //Waits for clients, then syncs playercount, and spawns npcs
    private IEnumerator waitForClients() {
        while (!WorldData.ready) yield return 0;

        string[] npcPrefabNames = { "CatNPC", "DogNPC", "EagleNPC", "WhaleNPC", "ChikenNPC" };
        List<GameObject> npcs = new List<GameObject>();
        
        foreach (string name in npcPrefabNames) npcs.Add(Resources.Load<GameObject>("Prefabs/NPCs/" + name));
        for (int i = 0; i < 100; i++) this.CmdSpawnNPC(npcs[Random.Range(0, npcs.Count)]);

        int playerCount = Object.FindObjectOfType<NetworkPlayerSelect>().numPlayers;
        while (playerCount != (GameObject.FindGameObjectsWithTag("Enemy").Length + 1)) //When this is true, all clients are connected and in the game scene
            yield return 0;

        this._playerCount = playerCount; //sync playerCount to clients, now that all are here
    }

    //Spawn NPCs, then register players/npcs in datastructures in this class, and NPCWorldView
    //Populates the 4 key datastructures for this class and the NPCThread.
    //This class uses a list of players and a list of NPCs.
    //These lists are used to update a list of players and NPCs in NPCWorldView.
    //The need for keeping two list comes from the fact that the Unity API is not thread safe.
    //The NPCThread uses a thread safe representation of the World provided by NPCWorldView.
    private IEnumerator init() {

        while (!WorldData.ready) yield return 0;

        //Wait for all NPCs to spawn
        while (GameObject.FindGameObjectsWithTag("npc").Length != 100)
            yield return 0;

        //Wait for all players to spawn, +1 for localplayer 
        while (this._playerCount != (GameObject.FindGameObjectsWithTag("Enemy").Length + 1))
            yield return 0;

        //gather data about players for the NPCs
        GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        //Add players to the list of players
        this._players.Add(0, localPlayer);
        for (int i = 0; i < enemies.Length; i++)
            this._players.Add(i + 1, enemies[i]);

        //Add players to NPCWorldView so that the NPCThread can use data about them
        var players = NPCWorldView.players;
        for (int i = 0; i < this._players.Count; i++)
            players.Add(i, new NPCWorldView.GameCharacter(i));

        GameObject[] npcs = GameObject.FindGameObjectsWithTag("npc");
        for (int i = 0; i < npcs.Length; i++) {
            this._npcs.Add(i, npcs[i]);
            NPCWorldView.npcs.Add(i, new NPCWorldView.GameCharacter(i));
        }

        this._ready = true;
        NPCWorldView.ready = true;
        NPCWorldView.runNpcThread = true;
        this._instructions = new BlockingQueue<NPCThread.instruction>();
        this._npcThread = new NPCThread(this._instructions);
    }

    // Update is called once per frame
    void Update() {
        if (this._ready) {
            this.updateNPCWorldView();
            this.handleInstructions();
            this.removeDeadStuff();
        }
    }

    //Updates data about players and npcs for NPCWorldView so that the NPCThread can use data about them
    private void updateNPCWorldView() {
        if (this._npcThread.wait) return;
        //Update NPCS
        var npcs = NPCWorldView.npcs;
        foreach (var npc in this._npcs) {
            if (npc.Value != null) {
                Vector3 goal = npc.Value.GetComponent<NPC>().getGoal();
                npcs[npc.Key].update(npc.Value.transform.position, npc.Value.transform.forward, goal);
            } else
                this._deadNpcs.Add(npc.Key);
        }
        //Update Players
        var players = NPCWorldView.players;
        foreach (var player in this._players) {
            if (player.Value != null) {
                players[player.Key].update(player.Value.transform.position, player.Value.transform.forward, Vector3.negativeInfinity);
            } else
                this._deadPlayers.Add(player.Key);
        }
    }

    //Removes dead players/npcs from data structure, its important that the NPCThread is done looping through the npcs, 
    //  because removing dead stuff will break a foreach loop
    void removeDeadStuff() {
        if (this._deadNpcs.Count > 0 || this._deadNpcs.Count > 0) {
            if (this._npcs.Count <= 1) {
                NPCWorldView.runNpcThread = false;
                this._deadNpcs.Clear();
                this._deadPlayers.Clear();
                this._ready = false;
                NPCWorldView.clear();
                return;
            } else
                if (this._npcThread.isUpdating) { this._npcThread.wait = true; return; /*Wait for npc thread to catch up */}

            var players = NPCWorldView.npcs;
            foreach (int dead in this._deadPlayers) {
                this._players.Remove(dead);
                players.Remove(dead);
            }
            var npcs = NPCWorldView.npcs;
            foreach (int dead in this._deadNpcs) {
                this._npcs.Remove(dead);
                npcs.Remove(dead);
            }
            this._deadNpcs.Clear();
            this._deadPlayers.Clear();
            this._npcThread.wait = false;
        }
    }

    //Recieves instructions from the NPCThread, and passes them along to the NPC GameObjects in the scene
    void handleInstructions() {
        while (!this._instructions.isEmpty()) {
            var instruction = this._instructions.Dequeue();
            if (this._npcs.ContainsKey(instruction.id) && this._npcs[instruction.id] != null)
                this._npcs[instruction.id].GetComponent<NPC>().update(instruction.moveDir, instruction.goal);
        }
    }

    //Spawns a NPC with a random direction
    [Command]
    private void CmdSpawnNPC(GameObject npc) {
        var npcInstance = Instantiate(npc);
        WorldGrid.Cell cell;
        do { //Find a random position for the NPC
            int x = Random.Range(0, WorldData.cellCount);
            int z = Random.Range(0, WorldData.cellCount);
            cell = WorldData.worldGrid.getCell(x, 1, z);
        } while (cell.blocked);
        //Angle is used to generate a direction
        float angle = Random.Range(0, Mathf.PI * 2);
        Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        npcInstance.GetComponent<NPC>().spawn(cell.pos, dir);

        NetworkServer.Spawn(npcInstance);

    }

    //It's important to stop the NPCThread when quitting
    void OnApplicationQuit() {
        NPCWorldView.runNpcThread = true;
        NPCWorldView.clear();
    }

    void OnDestroy() {
        NPCWorldView.runNpcThread = false;
        NPCWorldView.clear();
    }
}