using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
public class playerManager : SimulationBehaviour, IPlayerJoined, INetworkRunnerCallbacks
{

    public GameObject PlayerPrefab;
    public GameObject NPCPlayerPrefab;
    public int id;
    public int PlayerNum;
    private PlayerRef Player;
    private bool Ijoined;
    private WaitingManager waitingmanager;
    public Canvas canvas;
    public string Device;
    public bool playerManagerNPCMode;

    public bool subSceneMade;
    public Scene SubScene;
    public Scene PredictScene;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
#if UNITY_Device
        Device = "PC";
        Debug.Log("Unityエディタ");
#elif UNITY_ANDROID
        Device = "Android";
　      Debug.Log("Android");
#elif UNITY_IOS
        Device = "iOS";
　      Debug.Log("iOS");
#else
        Device = "PC";
        Debug.Log("Other");
#endif
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer && Ijoined == false)
        {
            Ijoined = true;
            id = player.PlayerId + 1;
            Player = player;
        }
    }
    private void Update()
    {
    }

    public void MakePrefab(bool NPC, GameObject NPCPlayer)
    {
        GameObject player;
        if (id == 1)
        {
            var newPlayer = Runner.Spawn(PlayerPrefab, new Vector3(20, 1, -95), Quaternion.Euler(0, 0, 0), Player);
            newPlayer.name = "player";
            newPlayer.GetComponent<PlayerMove>().id = id;
            player = newPlayer.gameObject;
            if (NPC)
            {
                playerManagerNPCMode = true;
                var newNPC = Runner.Spawn(NPCPlayer, new Vector3(-20, 1, 80), Quaternion.Euler(0, 180, 0));
                NPCMode(newNPC);
                newNPC.name = "NPCPlayer";
                newNPC.GetComponent<NPCPlayerMove>().player = player;
            }
        }
        else if (id == 2)
        {
            var newPlayer = Runner.Spawn(PlayerPrefab, new Vector3(-20, 1, 80), Quaternion.Euler(0, 180, 0), Player);
            newPlayer.name = "player";
            newPlayer.GetComponent<PlayerMove>().id = id;
        }

    }

    private void NPCMode(NetworkObject NPCPlayer)
    {
        NPCPlayer.GetComponent<PlayerMove>().id = 2;
        NPCPlayer.gameObject.AddComponent<NPCPlayerMove>();
        Destroy(NPCPlayer.GetComponent<CameraMove>());
        Destroy(NPCPlayer.GetComponent<PlayerService>());
        Destroy(NPCPlayer.GetComponent<PlayerMove>());
    }
    private async void PlayerNumberCheck()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        waitingmanager = FindObjectOfType<WaitingManager>();
        if (waitingmanager == null) return;
        waitingmanager.ConfirmPlayerNum(PlayerNum, id);
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        PlayerNum = player.PlayerId + 1;
        print("hello :" + player.PlayerId);
        PlayerNumberCheck();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        print("Bye :" + player.PlayerId);
        PlayerNum--;
        PlayerNumberCheck();

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
