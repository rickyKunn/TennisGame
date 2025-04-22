using UnityEngine;

public class JudgeDevice : MonoBehaviour
{
    private playerManager playermanager;

    private void Start()
    {
        playermanager = Object.FindObjectOfType<playerManager>();
        if (playermanager.Device == "PC")
        {
            base.gameObject.SetActive(value: false);
        }
    }
}
