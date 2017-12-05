using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameInfo {

    private static bool _ready;
    private static bool _playersReady;
    private static string _gamemode;
    private static string _map;

	static GameInfo() {
        clear();
    }

    public static void clear() {
        _ready = false;
        _playersReady = false;
        _gamemode = "NOT SET";
        _map = "NOT SET";
    }

    public static bool ready { get { return _ready; } }
    public static bool playersReady {  get { return _playersReady; } }
    public static string gamemode { get { return _gamemode; } }
    public static string map { get { return _map; } }

    public static void setPlayersToReady() {
        _playersReady = true;
    }

    public static void init (string gamemode, string map) {
        Debug.Log("GameInfo: GameMode: " + gamemode + ", Map: " + map);
        _gamemode = gamemode;
        _map = map;
        _ready = true;
    }
}
