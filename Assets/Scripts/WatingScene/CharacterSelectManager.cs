using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    [SerializeField]
    private Sprite[] ButtonImages;

    [SerializeField]
    private GameObject characterButton;

    private void Start()
    {
        int num = ButtonImages.Length;
        MonoBehaviour.print(num);
        for (int i = 0; i < num; i++)
        {
            GameObject obj = Object.Instantiate(characterButton, base.transform);
            obj.GetComponent<Image>().sprite = ButtonImages[i];
            obj.GetComponent<ButtonInfo>().SetThisProperty(i, ButtonImages[i]);
        }
    }

    private void Update()
    {
    }
}