using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;


public class ButtonInfo : MonoBehaviour
{
    private int buttonId;
    private Sprite thisImg;
    [SerializeField]
    public GameObject[] playerList;
    private playerManager playermanager;
    private PlayerData playerdata = null;

    private GameObject chosenCharacterImage;
    private WaitingManager waitingmanager;

    async void Start()
    {
        playermanager = FindObjectOfType<playerManager>();
        chosenCharacterImage = GameObject.Find("SelectedCharacter");
        waitingmanager = FindObjectOfType<WaitingManager>();

        await UniTask.WaitUntil(() => waitingmanager.willStart); //始まるまでawait
        this.GetComponent<Button>().enabled = false;

    }
    // Update is called once per frame
    void Update()
    {
    }

    public void SetThisProperty(int thisButtonId, Sprite thisButtonImg)
    {
        buttonId = thisButtonId;
        thisImg = thisButtonImg;
    }

    public void SendCharacterInfo()
    {
        if (playerList[buttonId] == null) return;
        if (playermanager != null)
        {
            print("playerPrefab:" + playermanager.PlayerPrefab);
            playerdata = FindObjectOfType<PlayerData>();
            playermanager.PlayerPrefab = playerList[buttonId];
            SetAbility(buttonId, false);
            var image = chosenCharacterImage.GetComponent<Image>();
            image.enabled = true;
            image.sprite = thisImg;
            FindObjectOfType<WaitingManager>().myPictureId = buttonId;
        }

    }
    public void SendNPCCharacterInfo(int NPCKind)
    {
        playerdata = FindObjectOfType<PlayerData>();
        SetAbility(NPCKind, true);
    }

    private void SetAbility(int playerKind, bool NPCMode)
    {

        switch (playerKind)
        {
            case 0:  //アモングアス
                playerdata.SetThisStruct(
                    NPCMode,//NPC or not
                    40,//speed
                    4,//range
                    0.85f,//drivePower
                    0.85f,//slicePower
                    1.1f,//driveServePower
                    1.2f,//sliceServePower
                    15,//sliceServeStrength
                    15,//sliceStrokeStrength
                    0.6f,//hitPose
                    1300, //hitPoint
                    1
                    );

                break;
            case 1:  //男
                playerdata.SetThisStruct(
                    NPCMode,//NPC or not
                    30,//speed
                    5,//range
                    1f,//drivePower
                    1f,//slicePower
                    0.9f,//driveServePower
                    1.1f,//sliceServePower
                    15,//sliceServeStrength
                    15,//sliceStrokeStrength
                    1f,//hitPose
                    1000, //hitPoint
                    2
                    );
                break;
            case 2:  //調子くん
                playerdata.SetThisStruct(
                    NPCMode,//NPC or not
                    25,//speed
                    4,//range
                    1.2f,//drivePower
                    1.2f,//slicePower 1.6
                    0.7f,//driveServePower
                    1.0f,//sliceServePower
                    2,//sliceServeStrength
                    25,//sliceStrokeStrength15
                    1f,//hitPose
                    1300,
                    3
                    );
                break;
            case 3:  //くまちゃん
                playerdata.SetThisStruct(
                    NPCMode,//NPC or not
                    30,//speed
                    5,//range
                    1.3f,//drivePower
                    1.3f,//slicePower 1.6
                    0.6f,//driveServePower
                    0.8f,//sliceServePower
                    33,//sliceServeStrength
                    15,//sliceStrokeStrength15
                    1f,//hitPose
                    600,
                    4
                    );
                break;

        }
    }
}
