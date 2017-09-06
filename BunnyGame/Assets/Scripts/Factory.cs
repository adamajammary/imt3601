using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Factory {
    private class ObjectPool { // A class that manages instances of a prefab
        private int                 _maxObjects;
        private GameObject          _objectPrefab;
        private List<GameObject>    _activeObjects;
        private List<GameObject>    _inactiveObjects;

        public ObjectPool(int maxObjects, GameObject obj) {
            this._maxObjects =      maxObjects;
            this._objectPrefab =    obj;
            this._activeObjects =   new List<GameObject>();
            this._inactiveObjects = new List<GameObject>();
        }

        public GameObject getObject() {
            int totalObjects = this._inactiveObjects.Count + this._inactiveObjects.Count;

            if (this._inactiveObjects.Count > 0) { // If there are inactive objects, just return one of them
                GameObject obj = this._inactiveObjects[0];
                this._inactiveObjects.RemoveAt(0);
                this._activeObjects.Add(obj);
                obj.SetActive(true);
                return obj;
            }else if (totalObjects < this._maxObjects) { // If there is room for more objects, instantiate a new object
                GameObject obj = MonoBehaviour.Instantiate(this._objectPrefab);
                this._activeObjects.Add(obj);
                return obj;
            } else { // If there is no more room, return the oldest active object
                GameObject obj = this._activeObjects[0];
                this._activeObjects.RemoveAt(0);
                this._activeObjects.Add(obj);
                return obj;
            }
        }

        public void collectTrash() { // Moves inactive objects from active list to inactive list
            for (int i = 0; i < this._activeObjects.Count; i++) {
                GameObject obj = this._activeObjects[i];
                if (!obj.activeSelf) {
                    this._activeObjects.RemoveAt(i);
                    this._inactiveObjects.Add(obj);
                }
            }
        }
    }

    private static GameObject poop;
    private static ObjectPool _poopPool;
    
    public static void init(List<GameObject> factoryPrefabs) {
        Dictionary<string, GameObject> fp = new Dictionary<string, GameObject>();
        foreach (GameObject prefab in factoryPrefabs)
            fp.Add(prefab.name, prefab);

        _poopPool = new ObjectPool(20, fp["poop"]);       
    }

    public static void update() {
        _poopPool.collectTrash();
    }

    public static GameObject getPoop() {
        return _poopPool.getObject();
    }
}