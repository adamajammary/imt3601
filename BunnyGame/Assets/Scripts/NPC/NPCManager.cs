using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NPCManager : NetworkBehaviour { 
    private int         _cellCount; //Amount of cells in NPCWorldView

    private Dictionary<int, GameObject> _players;
    private Dictionary<int, GameObject> _npcs;
    private List<int> _deadPlayers;
    private List<int> _deadNpcs;
    private NPCThread _npcThread; //The thread running the logic for NPCs using NPCWorldView maintained by this class
    private BlockingQueue<NPCThread.instruction> _instructions; //Queue used to recieve instuctions from NPCThread
    private bool _ready;

    // Use this for initialization
    void Start() {
        _cellCount = NPCWorldView.cellCount;            

        this._players = new Dictionary<int, GameObject>();
        this._npcs = new Dictionary<int, GameObject>();
        this._deadPlayers = new List<int>();
        this._deadNpcs = new List<int>();
        this._ready = false;
        StartCoroutine(lateStart());
    }

    //This is how i deal with networking until i learn more about it
    private IEnumerator lateStart() {
        yield return new WaitForSeconds(1.0f);
        while (!NPCWorldView.ready) yield return 0;

        if (this.isServer) {
            string[] npcPrefabNames = { "CatNPC", "DogNPC", "EagleNPC", "WhaleNPC", "ChikenNPC" };
            List<GameObject> npcs = new List<GameObject>();
            foreach (string name in npcPrefabNames) npcs.Add(Resources.Load<GameObject>("Prefabs/NPCs/" + name));
            for (int i = 0; i < 100; i++) this.CmdSpawnNPC(npcs[Random.Range(0, npcs.Count)]);
        }

        //gather data about players for the NPCs
        GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");
        this._players.Add(0, localPlayer);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
            this._players.Add(i + 1, enemies[i]);
        var players = NPCWorldView.getPlayers();
        for (int i = 0; i < this._players.Count; i++)
            players.Add(i, new NPCWorldView.GameCharacter(i));
      
        GameObject[] turtles = GameObject.FindGameObjectsWithTag("npc");
        for (int i = 0; i < turtles.Length; i++) {
            this._npcs.Add(i, turtles[i]);
            NPCWorldView.getNpcs().Add(i, new NPCWorldView.GameCharacter(i));
        }
        
        this._instructions = new BlockingQueue<NPCThread.instruction>();
        this._npcThread = new NPCThread(this._instructions);
        this._ready = true;
    }

    private void updateNPCWorldView() {        
        //Update NPCS
        var npcs = NPCWorldView.getNpcs();
        foreach (var npc in this._npcs) {
            if (npc.Value != null) {
                Vector3 goal = npc.Value.GetComponent<NPC>().getGoal();
                npcs[npc.Key].update(npc.Value.transform.position, npc.Value.transform.forward, goal);
            } else
                this._deadNpcs.Add(npc.Key);
        }
        //Update Players
        var players = NPCWorldView.getPlayers();
        foreach (var player in this._players) {
            if (player.Value != null) {
                players[player.Key].update(player.Value.transform.position, player.Value.transform.forward, Vector3.negativeInfinity);
            } else
                this._deadPlayers.Add(player.Key);
        }
        this.removeDeadStuff();        
    }

    // Update is called once per frame
    void Update () {
        if (this._ready) {
            this.updateNPCWorldView();
            this.handleInstructions();
        }
    }

    void removeDeadStuff() {
        if (this._deadNpcs.Count > 0 || this._deadNpcs.Count > 0) {
            if (this._npcs.Count <= 1) {
                NPCWorldView.setRunNPCThread(false);
                this._deadNpcs.Clear();
                this._deadPlayers.Clear();
                this._ready = false;              
                return;
            } else
                while (this._npcThread.isUpdating) { /*Wait for npc thread to catch up */}

            var players = NPCWorldView.getPlayers();
            foreach (int dead in this._deadPlayers) {
                this._players.Remove(dead);
                players.Remove(dead);
            }
            var npcs = NPCWorldView.getNpcs();
            foreach (int dead in this._deadNpcs) {
                this._npcs.Remove(dead);
                npcs.Remove(dead);
            }
            this._deadNpcs.Clear();
            this._deadPlayers.Clear();
        }
    }

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
        NPCWorldView.worldCellData landCell;
        NPCWorldView.worldCellData waterCell;
        do { //Find a random position for the NPC
            int x = Random.Range(0, this._cellCount);
            int y = Random.Range(0, this._cellCount);
            landCell = NPCWorldView.getCell(true, x, y);
            waterCell = NPCWorldView.getCell(false, x, y);            
        } while (landCell.blocked || !waterCell.blocked);

        //Angle is used to generate a direction
        float angle = Random.Range(0, Mathf.PI * 2);
        Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        npcInstance.GetComponent<NPC>().spawn(landCell.pos, dir);

        NetworkServer.Spawn(npcInstance);
        
    }

    void OnApplicationQuit() {
        NPCWorldView.setRunNPCThread(false);
    }

    void OnDestroy() {
        NPCWorldView.setRunNPCThread(false);
    }
}
