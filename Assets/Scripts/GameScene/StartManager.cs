using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class GameLauncher : MonoBehaviour
{
    [SerializeField] private NetworkRunner _networkRunnerPrefab;
    [SerializeField] private GameObject LoadParticle;

    private NetworkRunner _runnerInstance;

    /// <summary>
    /// Fusion 2 用シーン遷移付き StartGame
    /// </summary>
    private async Task StartGameAsync(GameMode mode, string roomName, string sceneName)
    {
        // ローディング演出
        Instantiate(LoadParticle, new Vector3(0, -3, 0), Quaternion.identity);

        _runnerInstance = FindObjectOfType<NetworkRunner>() ??
                          Instantiate(_networkRunnerPrefab);
        _runnerInstance.ProvideInput = true;

        // 開始パラメータ
        var startArgs = new StartGameArgs
        {
            GameMode = mode,
            SessionName = roomName,
            PlayerCount = 2,
        };

        // セッション開始
        var result = await _runnerInstance.StartGame(startArgs);
        if (!result.Ok)
        {
            Debug.LogError($"StartGame failed: {result.ShutdownReason}");
            return;
        }

        int buildIndex = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity");
        var target = SceneRef.FromIndex(buildIndex);

        await _runnerInstance.LoadScene(target, LoadSceneMode.Single);
    }
}
