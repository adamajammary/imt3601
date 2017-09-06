using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public List<GameObject> factoryPrefabs;

	private void Awake () {
        Factory.init(factoryPrefabs);
	}

    private void Update() {
        Factory.update();
    }
}
