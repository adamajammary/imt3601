using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;

public class client : MonoBehaviour {
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    
    public bool connectToServer(string host, int port) {
        if (this.socketReady)
            return false;

        try {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            this.socketReady = true;
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
        }
    }

    public void send(string data) {
        if (!this.socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    private void onIncomingData(string data) {
        Debug.Log(data);
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

public class GameClient {
    public string name;
    public bool isHost;
}