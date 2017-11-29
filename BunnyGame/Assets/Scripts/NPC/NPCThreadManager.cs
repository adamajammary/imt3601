using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCThreadManager {

    private NPCThread[] _threads;
    private BlockingQueue<NPCThread.instruction>[] _instructions;

    public NPCThreadManager(int count) {
        this._instructions = new BlockingQueue<NPCThread.instruction>[count];
        this._threads = new NPCThread[count];
        int perThreadNpcs = NPCWorldView.npcs.Count / count;
        for (int i = 0; i < count; i++) {
            this._instructions[i] = new BlockingQueue<NPCThread.instruction>();
            var npcs = new List<NPCWorldView.GameCharacter>();
            for (int j = perThreadNpcs * (i + 0); j < perThreadNpcs * (i + 1); j++) {
                NPCWorldView.GameCharacter npc;
                NPCWorldView.npcs.TryGetValue(j, out npc);
                npcs.Add(npc);
            }
            this._threads[i] = new NPCThread(this._instructions[i], npcs);
        }
    }

    public bool isUpdating {
        get {
            foreach (var thread in this._threads)
                if (thread.isUpdating)
                    return true;
            return false;
        }
    }

    public bool wait {
        get {
            foreach (var thread in this._threads)
                if (thread.wait)
                    return true;
            return false;
        }
        set {
            foreach (var thread in this._threads)
                thread.wait = value;
        }
    }

    public BlockingQueue<NPCThread.instruction>[] instructions { get { return this._instructions; } }
}
