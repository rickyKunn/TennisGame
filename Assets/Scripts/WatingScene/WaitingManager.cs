using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;


public class WaitingManager : NetworkBehaviour
{

    private int playerNum;
    private int maxNum = 2, thisId;
    private int thisroomPlayerNum;
    private bool canPlay;
    [SerializeField] public TextMeshProUGUI WaitingText = null;
    [SerializeField] private TextMeshProUGUI StartText = null;
    [SerializeField] private Button StartButton = null;

    [SerializeField] private NetworkObject NetworkData;
    [SerializeField] private GameObject NetworkDatas;
    public bool characterChosen;
    private bool Ijoined, Invoked;
    private NetworkObject MyNetworkData = null;
    public int checkedNum; //スタートボタンを押した人数
    private bool IChecked;
    private bool hasClicked;
    public bool willStart;
    public int myPictureId;

    [SerializeField] private TextMeshProUGUI NPCText;
    public bool NPC = false;
    private void Start()
    {
        StartText.text = "Choose the Character!";
        thisId = GameObject.Find("PlayerManager").GetComponent<playerManager>().id;

    }
    private void Update()
    {
        if (Ijoined == false) return;
        playerNum = Runner.ActivePlayers.Count();
        if (playerNum == maxNum && Invoked == false)
        {
            Invoked = true;
        }

        if (willStart == false && IChecked == true && checkedNum >= playerNum) //押されたボタンの回数が現在のプレイヤー人数以上だった場合
        {
            willStart = true;
            LoadScene();
        }
    }

    public override void Spawned()
    {
        DontDestroyOnLoad(gameObject);
        Ijoined = true;
        MyNetworkData = Runner.Spawn(NetworkDatas, new Vector3(1, 1, 1));
        MyNetworkData.name = "networkData";

    }

    public void CharacterChosen()
    {
        characterChosen = true;
        StartText.text = "Start the Game!";
    }

    public async void ConfirmPlayerNum(int ThisRoomPlayerNum, int myId)
    {
        thisId = myId;
        thisroomPlayerNum = ThisRoomPlayerNum;
        if (maxNum <= ThisRoomPlayerNum)
        {
            NPC = false;
            NPCText.text = "Online\nMODE";
            canPlay = true;
            WaitingText.text = $"プレイヤー人数:{ThisRoomPlayerNum}";
            StartButton.interactable = true;
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            MyNetworkData.GetComponent<NetworkDatas>().IdChanged(thisId); //プレイヤー人数が更新されたら0.5sec待ってからキャラクターの名前を更新
        }
        else
        {
            if (NPC)
            {
                canPlay = true;
            }
            WaitingText.text = $"プレイヤー人数:{ThisRoomPlayerNum}";
            canPlay = false;
            StartButton.interactable = false;
        }


    }
    private playerManager playermanager;
    public void StartButtonPressed() //startButtonが押された時にお呼び出される
    {
        if (hasClicked) return;
        if (canPlay == false) return;
        if (characterChosen == false)
        {
            return;

        }
        hasClicked = true;
        IChecked = true;
        checkedNum++;
        MyNetworkData.GetComponent<NetworkDatas>().CanPlay();  //NetworkDatasの変数をtrueにする関数を呼び出す
    }

    public void NPCButtonPressed()
    {

        if (hasClicked) return;
        NPC = !NPC;
        NPCText.text = NPC ? "NPC\nMODE" : "Online\nMODE";
    }


    PhysicsScene physicsScene;
    public async void LoadScene()
    {
        playermanager = GameObject.Find("PlayerManager").GetComponent<playerManager>();

        if (playermanager == false) return;

        WaitingText.text = "Let's Start!!";

        int newNPCID = FindObjectOfType<OtherPicture>().ChangeOtherPictureNPC(NPC);
        GameObject newNPCPlayer = FindObjectOfType<ButtonInfo>().playerList[newNPCID];

        SendMyPictrueId();
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        SceneManager.LoadScene("GameScene");
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        playermanager.MakePrefab(NPC, newNPCPlayer);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        if (this.gameObject != null)
        {
            Destroy(this.gameObject);
        }
    }


    private void SendMyPictrueId()
    {
        var networkData = GameObject.Find("networkData");

        networkData.GetComponent<NetworkDatas>().ChangePicureId(myPictureId + 1);


    }

}
