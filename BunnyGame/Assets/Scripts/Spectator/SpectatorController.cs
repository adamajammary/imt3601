using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SpectatorMode
{
    FREE,
    FOLLOW
}

public class SpectatorController : MonoBehaviour {
    private SpectatorMode _spectatorMode;
    

    private ThirdPersonCamera _thirdPersonCamera;
    private int _currentPlayerIdx = 0;
    public List<GameObject> _players = new List<GameObject>(); // Should contain only live players
    
    private float moveSpeed = 10;
    private float currentSpeed = 0;

    private GameObject _freeCameraTarget;

    private float _yaw = 0;
    private float _pitch = 0;
    private float _speedSmoothVelocity;

    private bool escMenu;
    private bool lockCursor;
    private EscMenu escButtonPress;


    // Use this for initialization
    void Start () {
        _thirdPersonCamera = GetComponent<ThirdPersonCamera>();

        _freeCameraTarget = new GameObject() {
            name = "Spectator Camera [Free Mode]"
        };
        _freeCameraTarget.AddComponent<CharacterController>();


        this.enabled = false;

        this.escButtonPress = FindObjectOfType<EscMenu>();
    }

    // Update is called once per frame
    void Update () {

        if (_spectatorMode == SpectatorMode.FREE) {
            freeMove();

        } else if (_spectatorMode == SpectatorMode.FOLLOW) {
            if (Input.GetKeyDown(KeyCode.PageUp))
                switchPlayerView(_currentPlayerIdx + 1);
            else if (Input.GetKeyDown(KeyCode.PageDown))
                switchPlayerView(_currentPlayerIdx - 1);
        }

        // Switch spectating mode with space
        if (Input.GetKeyDown(KeyCode.Space))
            setSpectatorMode((SpectatorMode)((int)~_spectatorMode & 1));
	}

    // This should be called when the players dies and wants to go into spectating mode
    public void startSpectating() {
        Debug.Log("Start spectating");

        this.enabled = true;
        setSpectatorMode(SpectatorMode.FREE);

        _players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        updatePlayers();
    }


    // Update the list of players to only include living ones
    private void updatePlayers() {
        _players = _players.Where(player => !player.GetComponent<PlayerHealth>().IsDead()).ToList();
    }



    private void setSpectatorMode(SpectatorMode mode) {
        switch (mode) {
            case SpectatorMode.FOLLOW:
                setFollowCameraMode();
                break;
            case SpectatorMode.FREE:
                setFreeCameraMode();
                break;
            default:
                throw new InvalidOperationException("This should not happen!");
        }
    }


    /**
     * 
     * FREE MOVE MODE
     * Allows the spectator to move freely around the arena
     * 
     **/

    private void setFreeCameraMode() {
        Debug.Log("Set spectator mode to free mode");

        _spectatorMode = SpectatorMode.FREE;
        _thirdPersonCamera.enabled = false;

        _freeCameraTarget.SetActive(true);
        _freeCameraTarget.transform.position = _thirdPersonCamera.getTarget().position;
        transform.SetParent(_freeCameraTarget.transform);
        transform.localPosition = Vector3.zero;
        Debug.Log("Setting " + _freeCameraTarget.name + " as parent for " + name);
    }


    private void freeMove() {
        // ROTATE:
        _yaw += Input.GetAxis("Mouse X") * _thirdPersonCamera.getSensitivity();
        _pitch -= Mathf.Clamp(Input.GetAxis("Mouse Y") * _thirdPersonCamera.getSensitivity(),
                              -89, 89);

        transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, new Vector3(_pitch, _yaw), 
            ref _thirdPersonCamera._rotationSmoothVelocity, _thirdPersonCamera._rotationSmoothTime);


        this.currentSpeed = Mathf.SmoothDamp(currentSpeed, moveSpeed, ref _speedSmoothVelocity, 0.2f);

        // MOVE:
        Vector3 direction = new Vector3(
            Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A)), // Left/Right
            Convert.ToSingle(Input.GetKey(KeyCode.Q)) - Convert.ToSingle(Input.GetKey(KeyCode.E)), // Up/Down
            Convert.ToSingle(Input.GetKey(KeyCode.W)) - Convert.ToSingle(Input.GetKey(KeyCode.S))  // Forward/Backward
        );


        Vector3 velocity = transform.TransformDirection(direction).normalized * currentSpeed;

        _freeCameraTarget.GetComponent<CharacterController>().Move(velocity * Time.deltaTime);
    }

    /**
     *
     * FOLLOW MODE
     * Makes the spectator follow a live player
     * 
     **/

    private void setFollowCameraMode() {
        Debug.Log("Set spectator mode to follow mode");

        _spectatorMode = SpectatorMode.FOLLOW;
        _thirdPersonCamera.enabled = true;
        switchPlayerView(_currentPlayerIdx);
    }


    // Switch what player you're following
    // index: index of the player to view
    private void switchPlayerView(int index) {
        _currentPlayerIdx = index % _players.Count;
        Debug.Log("Switch player view to player " + _currentPlayerIdx + " -- " + _players.Count + " players alive");
        updatePlayers();
        transform.localPosition = new Vector3(0, 0, 0);
        _thirdPersonCamera.enabled = true;
        _thirdPersonCamera.SetTarget(_players[_currentPlayerIdx].transform);
    }




    private void handleEscMenu()
    {
        if (SceneManager.GetActiveScene().name != "Island")
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            escMenu = !escMenu;

        escButtonPress.EscPress(escMenu);

        if (escButtonPress.resumePressed()) {
            escMenu = false;
            escButtonPress.rusumePressedReset();
            lockCursor = true;
        }
    }
}
