using UnityEngine.Networking;

class PlayerInformation : NetworkBehaviour
{
    [SyncVar]
    public int ConnectionID = -1;

    [SyncVar]
    public string playerName;
}
