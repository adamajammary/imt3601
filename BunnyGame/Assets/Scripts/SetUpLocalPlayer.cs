﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetUpLocalPlayer : NetworkBehaviour {
    NetworkTransformChild modelNetworkTransform;


    // Use this for initialization
    void Start () {
        if (this.isLocalPlayer) {
            this.gameObject.AddComponent<PlayerController>();
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();
            camera.SetTarget(this.transform);
            this.tag = "Player";
        } else
            this.tag = "Enemy";

        setupClass();
    }

    void setupClass() {

        // Temporarily choosing class at random
        // NB! This random choice isn't synced across clients

        int animal = Random.Range(0,2);

        switch (animal) {
            case 0:
                setupFox();
                break;
            case 1:
                setupBunny();
                break;
        }
    }

    // Load a model for the character from a prefab
    // Prefabs for this has to be stored in "Assets/Resources/Prefabs/"
    private void loadAnimalModel(string name) {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/"+name);
        GameObject model = Instantiate(prefab);
        model.transform.SetParent(transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.rotation = transform.rotation;
    }

    // Setup the player as a Fox
    private void setupFox() {
        loadAnimalModel("FoxModel");

        if (!this.isLocalPlayer)
            return;

        // Setup fox-specific attributes, abilities, etc...
        PlayerController pc = GetComponent<PlayerController>();
        pc.runSpeed = 15;


    }

    // Setup the player as a bunny
    private void setupBunny() {
        loadAnimalModel("BunnyModel");

        if (!this.isLocalPlayer)
            return;

        // Set up bunny-specific attributes, abilities, etc...
        BunnyCommands commands = gameObject.AddComponent<BunnyCommands>();
        commands.bunnyPoop = Resources.Load<GameObject>("Prefabs/poop");
        PlayerController pc = GetComponent<PlayerController>();
        pc.jumpHeight = 3;
    }

}
