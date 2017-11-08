using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour {

    private CharacterController _characterController;
    private PlayerController _playerController;
    private PlayerEffects _playerEffects;
    private float _volume = 1f;

    // Animal sound
    private AudioSource _animalSoundPlayer;
    public AudioClip animalSound;

    // Movement
    private AudioSource _movementPlayer;
    private string _currentGroundType;
    public float frequencyModifier = 1; // This is for matching the frequency with the animation
    public float distanceFromGroundToCenter = 1f;
    public float volumeModifier = 1f;
    private Dictionary<string, AudioClip> _footStepClips  = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _groundHitClips = new Dictionary<string, AudioClip>();
    

    void Awake() {
        _movementPlayer = gameObject.AddComponent<AudioSource>();
        _movementPlayer.rolloffMode  = AudioRolloffMode.Linear;
        _movementPlayer.minDistance  =  0;
        _movementPlayer.maxDistance  = 50;
        _movementPlayer.spatialBlend =  1;
        _movementPlayer.spatialize = true;



        _animalSoundPlayer = gameObject.AddComponent<AudioSource>();
        //_animalSoundPlayer.clip = animalSound;
        //_animalSoundPlayer.rolloffMode = AudioRolloffMode.Linear;
        //_animalSoundPlayer.minDistance  = 0;
        //_animalSoundPlayer.maxDistance  = 50;
        //_animalSoundPlayer.spatialBlend = 1;
        //_animalSoundPlayer.spatialize   = true;
    }

    void Start () {
        _characterController = gameObject.GetComponent<CharacterController>();
        _playerController    = gameObject.GetComponent<PlayerController>();
        _playerEffects       = gameObject.GetComponent<PlayerEffects>();


        foreach (string name in new string[] { "leaf", "dirt", "stone", "wood" }) {
            _footStepClips.Add(name,  Resources.Load<AudioClip>("Audio/Movement/" + name));
            _groundHitClips.Add(name, Resources.Load<AudioClip>("Audio/GroundHit/" + name));
        }
        
        updateVolume();

        StartCoroutine(playFootSteps());
    }

    void Update () {
        string newGroundType = getGroundType();
        if(newGroundType != _currentGroundType && newGroundType != "") {
            _currentGroundType = newGroundType;
        }
	}

    /*
     * PARAMS:
     * volume: what you would expect
     * volumeModifier: Used to modify the volume independently of the general effect volume (stealth ability)
     */
    public void updateVolume(float volume = -1, float volumeModifier = -1) {
        if (volumeModifier != -1) this.volumeModifier = volumeModifier;

        if (volume == -1) _volume = PlayerPrefs.GetFloat("Effect Volume", 100) / 100f * (PlayerPrefs.GetFloat("Master Volume", 100) / 100f) * this.volumeModifier;
        else _volume = volume * this.volumeModifier;
        _movementPlayer.volume = _volume;
        _animalSoundPlayer.volume = _volume;
    }

    public float getVolume() {
        return _volume;
    }

    // Figures out what type of ground the player is on
    private string getGroundType() {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit = new RaycastHit();

        if (!Physics.Raycast(ray, out hit, 10))
            return "";
        else if (hit.transform.GetComponent<MeshRenderer>() == null)
            return "";

        if (hit.transform.CompareTag("Island")) {
            switch (hit.transform.GetComponent<MeshRenderer>().material.name.TrimEnd(" (Instance)".ToCharArray())) {
                case "material_9___1665":
                case "material_9___1669":
                case "mat9":
                case "mat10":
                case "mat12":
                    return "leaf";
                case "material_16___1665":
                case "material_16___1669":
                case "mat16":
                case "mat17":
                    return "stone";
                case "material_18___1665":
                    return "dirt";
                case "mat18": // !! This is because the big mountain is in the same mesh as the ground... so I have to manually check whether it is stone or dirt...
                    if (hit.point.y > 2.5f && Vector3.Distance(transform.position, new Vector3(24,transform.position.y,44)) < 80)
                         return "stone";
                    else return "dirt";
                case "material_19___1665":
                case "material_19___1669":
                case "mat20":
                    return "wood";
                default:
                    throw new System.Exception("PlayerAudio.getGroundType(): Could not match material \"" + hit.transform.GetComponent<MeshRenderer>().material.name + "\"");
            }
        }
        return "";
    }

    private IEnumerator playFootSteps() {
        RaycastHit hit = new RaycastHit();
        float normalSpeed = _playerController.walkSpeed;
        while (true) {
            Physics.Raycast(transform.position, Vector3.down, out hit);
            if (hit.distance < this.distanceFromGroundToCenter && _characterController.velocity.magnitude > 1) {
                _movementPlayer.PlayOneShot(_footStepClips[_currentGroundType]);
                float time = Mathf.Clamp(normalSpeed / _characterController.velocity.magnitude, 0.3f, 1) * this.frequencyModifier;
                yield return new WaitForSeconds(time);
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
