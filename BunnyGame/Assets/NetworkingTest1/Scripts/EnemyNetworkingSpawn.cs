using UnityEngine;
using UnityEngine.Networking;

public class EnemyNetworkingSpawn : NetworkBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int        _nrOfEnemies = 1;

    // Use this for initialization
    private void Start() {
    }

    // Spawns enemies with random positions and rotations
    public override void OnStartServer() {
        GameObject enemy;
        Vector3    spawnPosition;
        Quaternion spawnRotation;

        for (int i = 0; i < this._nrOfEnemies; i++) {
            spawnPosition = new Vector3(Random.Range(-8.0f, 8.0f), 0.0f, Random.Range(-8.0f, 8.0f));
            spawnRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 180.0f), 0.0f);
            enemy         = Instantiate(this._enemyPrefab, spawnPosition, spawnRotation);

            NetworkServer.Spawn(enemy);
        }
    }

    // Update is called once per frame
    private void Update() {
    }
}
