using Fusion;
using TMPro;
using UnityEngine;

public class StartManager : MonoBehaviour
{
    [SerializeField]
    private NetworkRunner _networkRunnerPrefab;

    [SerializeField]
    private PlayerData _playerDataPrefab;

    [SerializeField]
    private TMP_InputField _nickName;

    [SerializeField]
    private TextMeshProUGUI _nickNamePlaceholder;

    [SerializeField]
    private TMP_InputField _roomName;

    [SerializeField]
    private string _gameSceneName;

    [SerializeField]
    private GameObject LoadParticle;

    private NetworkRunner _runnerInstance;

    public void StartSharedSession()
    {
        SetPlayerData();
        StartGame(GameMode.Shared, _roomName.text, _gameSceneName);
        Object.Destroy(base.gameObject);
    }

    private void SetPlayerData()
    {
        PlayerData playerData = Object.FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            playerData = Object.Instantiate(_playerDataPrefab);
        }
        if (string.IsNullOrWhiteSpace(_nickName.text))
        {
            playerData.SetNickName(_nickNamePlaceholder.text);
        }
        else
        {
            playerData.SetNickName(_nickName.text);
        }
    }

    private async void StartGame(GameMode mode, string roomName, string sceneName)
    {
        Object.Instantiate(LoadParticle, new Vector3(0f, -3f, 0f), Quaternion.identity);
        _runnerInstance = Object.FindObjectOfType<NetworkRunner>();
        if (_runnerInstance == null)
        {
            _runnerInstance = Object.Instantiate(_networkRunnerPrefab);
        }
        _runnerInstance.ProvideInput = true;
        MonoBehaviour.print(roomName);
        StartGameArgs startGameArgs = default(StartGameArgs);
        startGameArgs.GameMode = mode;
        startGameArgs.SessionName = roomName;
        startGameArgs.PlayerCount = 2;
        StartGameArgs args = startGameArgs;
        await _runnerInstance.StartGame(args);
        _runnerInstance.SetActiveScene(sceneName);
    }
}