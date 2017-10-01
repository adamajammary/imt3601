using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NPCManager : NetworkBehaviour { 
    private float       _cellSize;  //The size of cells in NPCWorldView
    private int         _cellCount; //Amount of cells in NPCWorldView

    private Dictionary<int, GameObject> _players;
    private Dictionary<int, GameObject> _npcs;
    private List<int> _deadPlayers;
    private List<int> _deadNpcs;
    private NPCThread        _npcThread; //The thread running the logic for NPCs using NPCWorldView maintained by this class
    private BlockingQueue<NPCThread.instruction> _instructions; //Queue used to recieve instuctions from NPCThread
    private bool _ready;

    // Use this for initialization
    void Start() {
        if (this.isServer) {
            _cellSize = NPCWorldView.cellSize;
            _cellCount = NPCWorldView.cellCount;            

            this._players = new Dictionary<int, GameObject>();
            this._npcs = new Dictionary<int, GameObject>();
            this._deadPlayers = new List<int>();
            this._deadNpcs = new List<int>();
            this._ready = false;
            StartCoroutine(lateStart());
        }
    }

    //This is how i deal with networking until i learn more about it
    private IEnumerator lateStart() {
        yield return new WaitForSeconds(1.0f);
        while (!NPCWorldView.ready) yield return 0;
        //gather data about players for the NPCs
        GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");
        this._players.Add(0, localPlayer);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
            this._players.Add(i + 1, enemies[i]);
        var players = NPCWorldView.getPlayers();
        for (int i = 0; i < this._players.Count; i++)
            players.Add(i, new NPCWorldView.GameCharacter(i));

        //spawn npcs and gather npc data for the NPCs
        GameObject turtle = Resources.Load<GameObject>("Prefabs/TurtleNPC");
        for (int i = 0; i < 100; i++)  // Spawn turtles            
            this.CmdSpawnNPC(turtle, i);
        
        this._instructions = new BlockingQueue<NPCThread.instruction>();
        this._npcThread = new NPCThread(this._instructions);
        StartCoroutine(ASyncUpdate());
        this._ready = true;
    }

    private IEnumerator ASyncUpdate() {
        //Update data about gamecharacters in NPCWorldView
        int updateCount = 0; //How many objects have been updated this far
        int updatesPerFrame = 30; //How many objects to update per frame
        while (NPCWorldView.getRunNPCThread()) {
            //Update Players
            var players = NPCWorldView.getPlayers();
            for (int i = 0; i < this._players.Count; i++) {
                if (this._players[i] != null) {
                    players[i].update(this._players[i].transform.position, this._players[i].transform.forward);
                    updateCount++;
                } else
                    this._deadPlayers.Add(players[i].getId());
                
                if (updateCount > updatesPerFrame) {
                    updateCount = 0;
                    yield return 0;
                }
            }
            //Update NPCS
            var npcs = NPCWorldView.getNpcs();
            for (int i = 0; i < this._npcs.Count; i++) {
                if (this._npcs[i] != null) {
                    npcs[i].update(this._npcs[i].transform.position, this._npcs[i].transform.forward);
                    updateCount++;
                } else
                    this._deadNpcs.Add(npcs[i].getId());
                if (updateCount > updatesPerFrame) {
                    updateCount = 0;
                    yield return 0;
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (this._ready) {
            if (this._deadNpcs.Count > 0 || this._deadNpcs.Count > 0) {
                while (this._npcThread.isUpdating) { /*Wait for npc thread to catch up */}
                this.removeDeadStuff();
                this.handleInstructions(true);
            } else
                this.handleInstructions(false);
        }
    }

    void removeDeadStuff() {
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
    }

    void handleInstructions(bool filter) {
        while (!this._instructions.isEmpty()) {
            var instruction = this._instructions.Dequeue();
            if (filter) {
                if (this._npcs.ContainsKey(instruction.id)) // Filter param so that this test won't be done when not necessary
                    this._npcs[instruction.id].GetComponent<NPC>().setMoveDir(instruction.moveDir);
            } else
                this._npcs[instruction.id].GetComponent<NPC>().setMoveDir(instruction.moveDir);
        }
    }

    //Spawns a NPC with a random direction
    [Command]
    private void CmdSpawnNPC(GameObject npc, int id) {
        var turtle = Instantiate(npc);
        NPCWorldView.worldCellData landCell;
        NPCWorldView.worldCellData waterCell;
        do { //Find a random position for the NPC
            int x = Random.Range(0, this._cellCount);
            int y = Random.Range(0, this._cellCount);
            landCell = NPCWorldView.getCell(NPCWorldView.WorldPlane.LAND, x, y);
            waterCell = NPCWorldView.getCell(NPCWorldView.WorldPlane.WATER, x, y);            
        } while (landCell.blocked || !waterCell.blocked);
        turtle.GetComponent<NPC>().setSpawnPos(landCell.pos);
        //Angle is used to generate a direction
        float angle = Random.Range(0, Mathf.PI * 2);
        turtle.GetComponent<NPC>().setMoveDir(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
        this._npcs.Add(id, turtle);
        //Add a datastructure for the NPC in the NPCWorldView class
        NPCWorldView.getNpcs().Add(id, new NPCWorldView.GameCharacter(id));
        NetworkServer.Spawn(turtle);
    }

    void OnApplicationQuit() {
        NPCWorldView.setRunNPCThread(false);
    }

    void OnDestroy() {
        NPCWorldView.setRunNPCThread(false);
    }
}
