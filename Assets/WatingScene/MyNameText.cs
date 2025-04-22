using TMPro;
using UnityEngine;

public class MyNameText : MonoBehaviour
{
    private TextMeshProUGUI thisText;

    private void Start()
    {
        thisText = GetComponent<TextMeshProUGUI>();
        thisText.text = Object.FindObjectOfType<PlayerData>()._nickName;
    }

    private void Update()
    {
    }
}
