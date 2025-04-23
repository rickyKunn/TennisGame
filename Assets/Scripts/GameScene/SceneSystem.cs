using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSystem : MonoBehaviour
{
    public async void GameEnd()
    {
        GameObject.Find("player").GetComponent<PlayerMove>().thisDespawn();
        UnityEngine.Object.Destroy(base.gameObject);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5));
        SceneManager.LoadScene("StartScene");
    }
}
