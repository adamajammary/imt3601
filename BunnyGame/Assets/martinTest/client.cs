using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;

public class client : MonoBehaviour {
    public Queue<string> messages;
    public Queue<string> input;

    public string clientName { get; private set; }
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public bool connectToServer(string name, string host, int port) {
        if (this.socketReady)
            return false;

        try {
            Debug.Log("Connecting to server " + host + ":" + port.ToString());
            DontDestroyOnLoad(gameObject);
            this.socket = new TcpClient(host, port);
            this.stream = socket.GetStream();
            this.writer = new StreamWriter(stream);
            this.reader = new StreamReader(stream);
            this.socketReady = true;
            this.clientName = name;
            this.messages = new Queue<string>();
            this.input = new Queue<string>();
            send("clientName|" + this.clientName);
        } catch(Exception e) {
            Debug.Log(e.Message);
        }
        return this.socketReady;
    } 

    private void Update() {
        if (this.socketReady) {
            if (stream.DataAvailable) {
                string data = reader.ReadLine();
                if(data != null) 
                    onIncomingData(data);
            }
            sendInputToServer();
        }
    }
    private void sendInputToServer() {
        sendKeyToServer(KeyCode.W, "w");
        sendKeyToServer(KeyCode.S, "s");
    }
    private void sendKeyToServer(KeyCode key, string keyName) {
        if (Input.GetKeyDown(key))
            send(string.Format("key|{0}|{1}Down", this.clientName, keyName));
        if (Input.GetKeyUp(key))
            send(string.Format("key|{0}|{1}Up", this.clientName, keyName));
    }
    private void send(string data) {
        if (!this.socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }
    private void onIncomingData(string data) {
        Debug.Log("Raw data recieved by client: " + data);
        string[] d = data.Split('|');
        switch (d[0]) {
            case "key":
                this.input.Enqueue(data);
                break;
            case "startgame":
                GameObject.FindObjectOfType<GameManager>().init(this, data);
                break;
        }
    }
    private void OnApplicationQuit() {
        closeSocket();
    }
    private void OnDisable() {
        closeSocket();
    }
    private void closeSocket() {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        this.socketReady = false;
    }
}