using UnityEngine;

public class JudgeDevice : MonoBehaviour
{
    private playerManager playermanager;

    private void Start()
    {
        playermanager = Object.FindObjectOfType<playerManager>();
        if (playermanager.Device == "PC")
        {
            gameObject.SetActive(false);
        }
    }
}
