using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using UnityEngine;

public class Server : MonoBehaviour {
    private int port = 6321;
    private List<ServerClient> clients;
    private List<ServerClient> disconnectedList;
    private TcpListener server;
    private bool serverStarted;
    private bool gameStarted;

    public void init() {
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectedList = new List<ServerClient>();

        try {
            this.server = new TcpListener(IPAddress.Any, port);
            this.server.Start();
            startListening();
            this.serverStarted = true;
            this.gameStarted = false;
        }catch(Exception e) {
            Debug.Log(e);
        }
    }
    public void startGame() {
        this.gameStarted = true;
        string clientNames = "";
        foreach (ServerClient c in clients)
            clientNames += "|" + c.clientName;
        broadcast("startgame" + clientNames, clients);
    }
   
    private void Update() {
        if (!this.serverStarted)   
            return;

        if (!this.gameStarted)
            startListening();

        foreach(ServerClient c in this.clients) {
            if (!isConnected(c.tcp)) {
                c.tcp.Close();
                disconnectedList.Add(c);
                continue;
            } else { 
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable) {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();
                    if (data != null)
                        onIncomingData(c, data);
                }
            }
        }
        for (int i = 0; i < disconnectedList.Count - 1; i++) {
            this.clients.Remove(disconnectedList[i]);
            this.disconnectedList.RemoveAt(i);
        }
    }
    private void startListening() {
        server.BeginAcceptTcpClient(acceptTcpClient, server);
    }
    private void acceptTcpClient(IAsyncResult ar) {
        TcpListener listener = (TcpListener)ar.AsyncState;

        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        clients.Add(sc);

        Debug.Log("Someone connected!");
    }
    private bool isConnected(TcpClient c) {
        try {
            if (c != null && c.Client != null && c.Client.Connected) {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                return true;
            } else
                return false;
        } catch {
            return false;
        }
    }
    private void broadcast(string data, List<ServerClient> cl) {
        foreach(ServerClient sc in cl) {
            try {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }catch(Exception e) {
                Debug.Log(e.Message);
            }
        }
    }
    private void onIncomingData(ServerClient c, string data) {
        Debug.Log("Raw data recieved by server: " + data);
        string[] d = data.Split('|');

        switch (d[0]) {
            case "clientName":
                c.clientName = d[1];
                break;
            case "key":
                broadcast(data, clients);
                break;
            default:
                Debug.Log("Unkown message tag: " + d[0]);
                break;
        }
    }
}

public class ServerClient {
    public string clientName;
    public TcpClient tcp;

    public ServerClient(TcpClient tcp) {
        this.tcp = tcp;
    }
}
