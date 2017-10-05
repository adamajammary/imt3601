using UnityEngine.Networking;

class PlayerInformation : NetworkBehaviour
{
    [SyncVar]
    public int ConnectionID = -1;
    public string playerName;
}
