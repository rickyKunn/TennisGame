// playerManager.cs
// Photon Fusion 2.1+ 互換フルスタブ版
// （public / private 修飾子・ロジックは元コードを維持）  

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class playerManager : SimulationBehaviour, INetworkRunnerCallbacks
{
    // ───────────────────────────────────────────────
    //  公開フィールド
    // ───────────────────────────────────────────────
    public GameObject PlayerPrefab;
    public GameObject NPCPlayerPrefab;
    public int id;
    public int PlayerNum;
    public Canvas canvas;
    public string Device;
    public bool playerManagerNPCMode;

    public bool subSceneMade;
    public Scene SubScene;
    public Scene PredictScene;

    // ───────────────────────────────────────────────
    //  非公開フィールド
    // ───────────────────────────────────────────────
    private PlayerRef Player;
    private bool Ijoined;
    private WaitingManager waitingmanager;

    // ───────────────────────────────────────────────
    //  Unity lifetime
    // ───────────────────────────────────────────────
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
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

    private void Update() { }

    // ───────────────────────────────────────────────
    //  ローカル処理
    // ───────────────────────────────────────────────
    public void MakePrefab(bool NPC, GameObject NPCPlayer)
    {
        if (id == 1)
        {
            var newPlayer = Runner.Spawn(PlayerPrefab, new Vector3(20, 1, -95), Quaternion.identity, Player);
            newPlayer.name = "player";
            newPlayer.GetComponent<PlayerMove>().id = id;

            if (NPC)
            {
                playerManagerNPCMode = true;
                var newNPC = Runner.Spawn(NPCPlayer, new Vector3(-20, 1, 80), Quaternion.Euler(0, 180, 0));
                NPCMode(newNPC);
                newNPC.name = "NPCPlayer";
                newNPC.GetComponent<NPCPlayerMove>().player = newPlayer.gameObject;
            }
        }
        else if (id == 2)
        {
            var newPlayer = Runner.Spawn(PlayerPrefab, new Vector3(-20, 1, 80), Quaternion.Euler(0, 180, 0), Player);
            newPlayer.name = "player";
            newPlayer.GetComponent<PlayerMove>().id = id;
        }
    }

    private void NPCMode(NetworkObject npc)
    {
        npc.GetComponent<PlayerMove>().id = 2;
        npc.gameObject.AddComponent<NPCPlayerMove>();
        Destroy(npc.GetComponent<CameraMove>());
        Destroy(npc.GetComponent<PlayerService>());
        Destroy(npc.GetComponent<PlayerMove>());
    }

    private async void PlayerNumberCheck()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        waitingmanager = FindObjectOfType<WaitingManager>();
        if (waitingmanager != null)
            waitingmanager.ConfirmPlayerNum(PlayerNum, id);
    }

    // ───────────────────────────────────────────────
    //  INetworkRunnerCallbacks 実装
    // ───────────────────────────────────────────────
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        PlayerNum = player.PlayerId + 1;
        Debug.Log($"hello : {player.PlayerId}");

        if (player == runner.LocalPlayer && !Ijoined)
        {
            Ijoined = true;
            id = player.PlayerId + 1;
            Player = player;
        }
        PlayerNumberCheck();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Bye : {player.PlayerId}");
        PlayerNum--;
        PlayerNumberCheck();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason reason) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    // ★ 変更: NetDisconnectReason を受け取る
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress addr, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr msg) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> list) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken token) { }

    // ★ 変更: ReliableKey を受け取る
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    // ★ 新規: 受信進行状況コールバック
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
