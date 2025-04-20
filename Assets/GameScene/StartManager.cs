using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

namespace Asteroids.SharedSimple
{
    // A utility class which defines the behaviour of the various buttons and input fields found in the Menu scene
    public class StartManager : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _networkRunnerPrefab = null;
        [SerializeField] private PlayerData _playerDataPrefab = null;

        [SerializeField] private TMP_InputField _nickName = null;

        // The Placeholder Text is not accessible through the TMP_InputField component so need a direct reference
        [SerializeField] private TextMeshProUGUI _nickNamePlaceholder = null;

        [SerializeField] private TMP_InputField _roomName = null;
        [SerializeField] private string _gameSceneName = null;
        [SerializeField] private GameObject LoadParticle;
        private NetworkRunner _runnerInstance = null;

        // Attempts to start a new game session 
        public void StartSharedSession()
        {
            SetPlayerData();
            StartGame(GameMode.Shared, _roomName.text, _gameSceneName);
            Destroy(this.gameObject);
        }

        private void SetPlayerData()
        {
            var playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                playerData = Instantiate(_playerDataPrefab);
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

        private async void StartGame(GameMode mode, string roomName, string sceneName) //シーン遷移
        {
            Instantiate(LoadParticle,new Vector3(0,-3,0),Quaternion.identity);
            _runnerInstance = FindObjectOfType<NetworkRunner>();
            if (_runnerInstance == null)
            {
                _runnerInstance = Instantiate(_networkRunnerPrefab);
            }
            _runnerInstance.ProvideInput = true;

            print(roomName);

            var startGameArgs = new StartGameArgs() //プロパティ
            {
                GameMode = mode, //共有モード
                SessionName = roomName,  //ルーム名
                PlayerCount = 2, //最大人数2人
            };

            await _runnerInstance.StartGame(startGameArgs);            
            _runnerInstance.SetActiveScene(sceneName);
            
        }
    }
}
