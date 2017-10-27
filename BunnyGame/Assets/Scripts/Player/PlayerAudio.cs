using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour {

    private CharacterController _characterController;
    private PlayerController _playerController;
    private float _volume;

    // Animal sound
    private AudioSource _animalSoundPlayer;
    public AudioClip animalSound;

    // Movement
    private AudioSource _movementPlayer;
    private string _currentGroundType;
    public float footStepFrequency = 1f;
    public float runSpeedFrequency = 0.5f;
    private Dictionary<string, AudioClip> _footStepClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _groundHitClips = new Dictionary<string, AudioClip>();
    

    void Awake() {
        _movementPlayer = gameObject.AddComponent<AudioSource>();
        _movementPlayer.volume = 0;
        _movementPlayer.rolloffMode = AudioRolloffMode.Logarithmic;
        _movementPlayer.minDistance = 0;
        _movementPlayer.maxDistance = 25;



        _animalSoundPlayer = gameObject.AddComponent<AudioSource>();
        _animalSoundPlayer.clip = animalSound;
        _animalSoundPlayer.volume = 0;
        _animalSoundPlayer.rolloffMode = AudioRolloffMode.Logarithmic;
        _animalSoundPlayer.minDistance = 0;
        _animalSoundPlayer.maxDistance = 25;
    }

    void Start () {
        _characterController = gameObject.GetComponent<CharacterController>();
        _playerController = gameObject.GetComponent<PlayerController>();


        foreach (string name in new string[] { "leaf", "dirt", "stone", "wood" }) {
            _footStepClips.Add(name, Resources.Load<AudioClip>("Audio/Movement/" + name));
            _groundHitClips.Add(name, Resources.Load<AudioClip>("Audio/GroundHit/" + name));
        }


        updateVolume(PlayerPrefs.GetFloat("Effect Volume", 100) / 100f * (PlayerPrefs.GetFloat("Master Volume", 100) / 100f));

        StartCoroutine(playFootSteps());
    }

    void Update () {
        string newGroundType = getGroundType();
        if(newGroundType != _currentGroundType && newGroundType != "") {
            _currentGroundType = newGroundType;
        }

	}

    public void updateVolume(float v) {
        _volume = v;
        _movementPlayer.volume = v;
        _animalSoundPlayer.volume = v;
    }

    public float getVolume() {
        return _volume;
    }

    // Figures out what type of ground the player is on
    private string getGroundType() {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit = new RaycastHit();
        bool didHit = Physics.Raycast(ray, out hit, 10);

        if (!didHit)
            return "";
        else if (hit.transform.GetComponent<MeshRenderer>() == null)
            return "";

        Debug.Log("Material of ground below is: " + hit.transform.GetComponent<MeshRenderer>().material.name);
        
        if (hit.transform.CompareTag("ground")) {
            switch (hit.transform.GetComponent<MeshRenderer>().material.name.TrimEnd(" (Instance)".ToCharArray())) {
                case "mat9":
                case "mat10":
                case "mat12":
                    return "leaf";
                case "mat16":
                case "mat17":
                    return "stone";
                case "mat18": // !! This is because the big mountain is in the same mesh as the ground... so I have to manually check whether it is stone or dirt...
                    Debug.Log(transform.position + "; dist:" + Vector3.Distance(transform.position, new Vector3(24, transform.position.y, 44)));
                    if (transform.position.y > -12.5f && Vector3.Distance(transform.position, new Vector3(24,transform.position.y,44)) < 80)
                        return "stone";
                    else return "dirt";
                case "mat20":
                    return "wood";
                default:
                    throw new System.Exception("PlayerAudio.getGroundType(): Could not match material \"" + hit.transform.GetComponent<MeshRenderer>().material.name + "\"");
            }
        }
        return "";
    }


    private IEnumerator playFootSteps() {
        while (true) {
            if (_characterController.isGrounded && _characterController.velocity.magnitude > 0.1f) {
                _movementPlayer.PlayOneShot(_footStepClips[_currentGroundType]);
                yield return new WaitForSeconds(_playerController.running ? runSpeedFrequency : footStepFrequency);
            }
            else yield return null;
        }
    }


    public void playGroundHit(float vel) {
        _movementPlayer.PlayOneShot(_groundHitClips[_currentGroundType]);
        // To be fully implemented
    }

    public void playWaterHit(float vel)
    {
        // To be implemented
    }
}
