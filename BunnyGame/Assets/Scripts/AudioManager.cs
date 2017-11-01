using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : MonoBehaviour {
    public Transform cameraTransform;
    public int mapRadius;
    public float waterLevel;
    
    private float masterVolume;
    private float effectVolume;

    private bool _isUnderWater = false;

    // Ambient
    private AudioSource _ocean;
    private AudioSource _ambientNature;
    private AudioSource _pond;


    // Firewall
    private FireWall _firewall;
    private AudioSource _fire;


    // Music
    private AudioSource _music;
    private AudioClip[] _musicClips;


    private void Awake() {
        StartCoroutine(musicPlayer());
    }

    void Start () {
        if(GameObject.Find("AbilityPanel")) // Only start effectPlayer if we are in the game (not lobby). Checking for abilitypanel because it only exists in the game scene
            StartCoroutine(effectPlayer());

        updateVolume();
	}

    // Plays music tracks in a loop
    private IEnumerator musicPlayer() {
        _musicClips = Resources.LoadAll<AudioClip>("Audio/Music/");
        GameObject musicObj = new GameObject() {name = "MusicPlayer"};
        _music = musicObj.AddComponent<AudioSource>();
        _music.volume = 0;

        // TODO: Make the music clips fade in/out with overlap to make the transition between the tracks smoother

        int clipIndex = Random.Range(0, _musicClips.Length);
        while (true) {
            _music.PlayOneShot(_musicClips[clipIndex]);
            Debug.Log("Playing " + _musicClips[clipIndex].name);
            yield return new WaitForSeconds(_musicClips[clipIndex].length);
            clipIndex = (clipIndex + 1) % _musicClips.Length;
        }
    }


    // Plays sound effects
    private IEnumerator effectPlayer() {
        // Set up ocean sound:
        GameObject oceanObj = new GameObject { name = "OceanSoundPlayer" };
        _ocean = oceanObj.AddComponent<AudioSource>();
        _ocean.clip = Resources.Load<AudioClip>("Audio/water_lapping_sea_waves_sandy_beach_5_meters_01");
        _ocean.loop = true;
        _ocean.volume = 0;
        _ocean.Play();

        // Set up fire sound:
        GameObject fireObj = new GameObject { name = "FireSoundPlayer" };
        _fire = fireObj.AddComponent<AudioSource>();
        _fire.clip = Resources.Load<AudioClip>("Audio/nature_fire_big");
        _fire.loop = true;
        _fire.volume = 0;
        _fire.Play();

        // Update sound effects:
        while (true) {
            _isUnderWater = cameraTransform.position.y < waterLevel;
            updateFireSound();
            updateOceanSound();
            yield return null;
        }
    }


    // Sets the volume of the ocean sound based on distance from the ocean
    // also distorts sound when underwater
    private void updateOceanSound() {
        float distFromOcean = findDistanceToWater();

        if (distFromOcean != -1) {
            _ocean.volume = (Mathf.Max((distFromOcean > 0 ? 3f / distFromOcean : 1f), .05f) / 2f) * effectVolume;
            _ocean.pitch = _isUnderWater ? 0.2f : 1f;
        }
    }


    // Updates the volume of the fire sound based on distance from the firewall
    // also distorts sound when underwater
    private void updateFireSound() {
        // No updating if the firewall hasn't been created yet
        if (_firewall == null) {
            GameObject wall = GameObject.FindGameObjectWithTag("FireWall");
            if (wall == null) return;
            else _firewall = wall.GetComponent<FireWall>();
        }

        float distFromCenter = Vector2.Distance(new Vector2(cameraTransform.position.x, cameraTransform.position.z), new Vector2(_firewall.transform.position.x, _firewall.transform.position.z));
        float distFromWall = Mathf.Abs(_firewall.transform.localScale.x / 2 - distFromCenter);

        _fire.volume = (distFromWall > 0 ? 0.05f + 0.95f * Mathf.Pow(1 - distFromWall / 250, 4) : 1) / 3 * effectVolume;
    }



    // Sets the volume values based on player settings
    public void updateVolume() {
        masterVolume = PlayerPrefs.GetFloat("Master Volume", 100) / 100f;
        effectVolume = PlayerPrefs.GetFloat("Effect Volume", 100)/100f * masterVolume;
        _music.volume = PlayerPrefs.GetFloat("Music Volume", 100)/500f * masterVolume;

        // Update the volume on all players' PlayerAudio:

        List<GameObject> players = new List<GameObject>();
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        players.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

        foreach (GameObject player in players)
            player.GetComponent<PlayerAudio>().updateVolume(effectVolume);
    }



    private float findDistanceToWater() {
        if (!NPCWorldView.ready)
            return -1;

        NPCWorldView.worldCellData[,] waterCells = NPCWorldView.water; // Getting a map of the water from here because it should already be generated for the npcs anyways.
        int[] playercell = NPCWorldView.convertWorld2Cell(cameraTransform.position);
        Vector2 playerCellPos = new Vector2(playercell[0], playercell[1]);
        int count = 0;

        Vector2 closest = new Vector2(500, 500);
        for (int x = 0; x < waterCells.GetLength(0); x++) {
            for (int z = 0; z < waterCells.GetLength(1); z++) {
                if (!waterCells[x, z].blocked) {
                    if(Vector2.Distance(playerCellPos, new Vector2(x, z)) < 
                       Vector2.Distance(playerCellPos, closest)) {
                        closest.x = x;
                        closest.y = z;
                        count++;
                    }
                }
            }
        }

        Vector3 closest_vec3 = waterCells[(int)closest.x, (int)closest.y].pos;
        closest_vec3.y = waterLevel;
        float tileCenterToCorner = Mathf.Sqrt(Mathf.Pow(NPCWorldView.cellSize, 2) * 2);
        Debug.Log(Vector3.Distance(cameraTransform.position, closest_vec3) + "   ::   " + Mathf.Max(Vector3.Distance(cameraTransform.position, closest_vec3), tileCenterToCorner) * (1 / tileCenterToCorner));
        return Mathf.Max(Vector3.Distance(cameraTransform.position, closest_vec3), tileCenterToCorner) * (1/ tileCenterToCorner);
    }

}
