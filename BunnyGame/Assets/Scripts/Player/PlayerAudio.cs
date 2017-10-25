using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour {

    private string _currentGroundType;
    private Dictionary<string, AudioClip> _footStepClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _groundHitClips = new Dictionary<string, AudioClip>();
    public float footStepFrequency = 1f;

    private AudioSource _oneShotPlayer;

    private CharacterController _characterController;


    void Start () {
        _characterController.GetComponent<CharacterController>();

        _oneShotPlayer = gameObject.AddComponent<AudioSource>();
        _oneShotPlayer.volume = 0;

        foreach (string name in new string[] { "bush", "dirt", "rock", "wood" }) {
            _footStepClips.Add(name, Resources.Load<AudioClip>("Audio/move/" + name));
            _footStepClips.Add(name, Resources.Load<AudioClip>("Audio/groundhit/" + name));
        }


        StartCoroutine(playFootSteps());
    }

    void Update () {
        string newGroundType = getGroundType();
        if(newGroundType != _currentGroundType && newGroundType != "") {
            _currentGroundType = newGroundType;
        }

	}

    public void updateVolume(float v) {
        _oneShotPlayer.volume = v;
    }

    // Figures out what type of ground the player is on
    private string getGroundType() {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(ray, out hit, 10);
        
        if (hit.transform.CompareTag("ground")) {
            switch (hit.transform.GetComponent<MeshRenderer>().material.name) {
                case "mat9":
                case "mat10":
                case "mat12":
                    return "bush";
                case "mat15":
                    return "dirt";
                case "mat16":
                    return "rock";
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
            if (_characterController.isGrounded)
                _oneShotPlayer.PlayOneShot(_footStepClips[_currentGroundType]);
            yield return new WaitForSeconds(footStepFrequency);
        }
    }


    public void playGroundHit(float vel) {
        _oneShotPlayer.PlayOneShot(_groundHitClips[_currentGroundType]);
    }
}
