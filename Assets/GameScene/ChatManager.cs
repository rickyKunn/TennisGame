using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviour
{
    public bool isChating;
    [SerializeField]
    private GameObject ChatTimePanel;
    private GameObject newChatPanel;
    private RectTransform RTrans;
    private TextMeshProUGUI InputText;
    private TMP_InputField InField;
    public List<string> InputTexts = new List<string>(); 
    private void Start()
    {
        InField = GameObject.Find("ChatField").GetComponent<TMP_InputField>();
        InputText = this.GetComponent<TextMeshProUGUI>();
        print(InputText);
        //EventSystem.current.SetSelectedGameObject(this.transform.parent.parent.gameObject);

    }
    private void Update()
    {
        ChatChange();
    }

    private void ChatChange()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            if (!isChating)
            {
                InField.Select();
                isChating = ChatBool(isChating);
            }
        }
    }
    private bool ChatBool(bool Bool)
    {
        bool changedig;
        switch (Bool)
        {
            case true:
                if (InputText.text!="")
                {
                    if(InputText.text.Length > 1)
                    {
                        InputTexts.Add(InputText.text);
                        InField.text = "";
                        GettingRespond(InputTexts[InputTexts.Count - 1]);
                    }
                    if (EventSystem.current.currentSelectedGameObject != null)
                    {
                        EventSystem.current.SetSelectedGameObject(null); 
                    }
                }
                Destroy(newChatPanel);
            changedig = false;
                break;
            case false:
                newChatPanel = Instantiate(ChatTimePanel);
                newChatPanel.transform.SetParent(this.transform.parent.parent.parent);
                newChatPanel.GetComponent<RectTransform>().localPosition = Vector2.zero;
                changedig = true;
                break;
        }
        return changedig;
    }
    public void InpudChanged()
    {
        isChating = ChatBool(isChating);
    }
    public async void GettingRespond(string text)
    {
        print(text.Length);
    }

}
