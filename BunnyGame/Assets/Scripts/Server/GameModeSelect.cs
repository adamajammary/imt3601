using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class GameModeSelect : Voter {

    override public IEnumerator registerNetworkHandlers() {
        bool success = false;
        do {
            if ((NetworkClient.allClients.Count > 0) && !NetworkClient.allClients[0].connection.CheckHandler((short)NetworkMessageType.MSG_GAMEMODE_VOTE)) {
                NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_GAMEMODE_VOTE, recieveVote);
                success = true; ;
            }
            yield return 0;
        } while (!success);
    }

    // Send the network message to the server.
    override public void sendVote(string vote) {
        if (NetworkClient.allClients.Count < 1)
            return;
        NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_GAMEMODE_SELECT, new StringMessage(vote));

        sendGfxUpdate(vote);
    }
}
