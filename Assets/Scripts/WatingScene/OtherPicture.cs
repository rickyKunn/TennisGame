using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OtherPicture : MonoBehaviour
{
    [SerializeField]
    private Sprite[] myPicture;

    public void ChangeOtherPicture(int pictureId)
    {
        Image component = GetComponent<Image>();
        component.enabled = true;
        component.sprite = myPicture[pictureId];
    }

    public int ChangeOtherPictureNPC(bool NPCMode)
    {
        if (!NPCMode)
        {
            return 0;
        }
        Image component = GetComponent<Image>();
        int num = 3;
        component.enabled = true;
        component.sprite = myPicture[num];
        Object.FindObjectOfType<ButtonInfo>().SendNPCCharacterInfo(num);
        GameObject gameObject = GameObject.Find("OtherName");
        if ((bool)gameObject)
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = "AIくん";
            Object.FindObjectOfType<NetworkDatas>().NPCMode();
        }
        return num;
    }
}
