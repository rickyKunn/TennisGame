using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System.Linq;



public class NetworkDatas : NetworkBehaviour
{

    private int id;
    [HideInInspector]
    [Networked(OnChanged = nameof(ChangeWaitingSceneName))]
    public NetworkString<_16> NickName { get; private set; }
    [HideInInspector]
    [Networked(OnChanged = nameof(ChangeName))]
    public NetworkString<_16> NickName1 { get; private set; }
    [HideInInspector]
    [Networked(OnChanged = nameof(ChangeName))]
    public NetworkString<_16> NickName2 { get; private set; }
    [HideInInspector]
    [Networked(OnChanged = nameof(RecievePlaying))]
    public NetworkBool playButton { get; set; }
    [Networked(OnChanged = nameof(RecievePictureId))]
    public int pictureId { get; private set; }

    private TextMeshProUGUI MyNameText;
    private void Start()
    {
        id = FindObjectOfType<playerManager>().id;
        GameObject MyNameTextObj = GameObject.Find("MyName");
        if(MyNameTextObj != null) MyNameText = MyNameTextObj.GetComponent<TextMeshProUGUI>();

    }
    public void IdChanged(int CameId)
    {
        id = FindObjectOfType<playerManager>().id;
        if (CameId != id) return;
        var nickName = FindObjectOfType<PlayerData>().GetNickName();

        MyNameText.text = nickName; //自分の名前の欄に代入

        if (id == 1)
        {
            NickName1 = nickName;
        }
        else if (id == 2)
        {
            NickName2 = nickName;

        }

        NickName = nickName;
    }

    public void CanPlay()
    {
        playButton = true;
    }

    public void ChangePicureId(int _pictureId)
    {
        pictureId = _pictureId; //値が0のままにならないようにするため。
    }

    public override void FixedUpdateNetwork()
    {
    }

    public static void ChangeName(Changed<NetworkDatas> playerInfo)
    {
        
        var playerdata = FindObjectOfType<PlayerData>();
        if (playerInfo.Behaviour.NickName1.ToString() != "") playerdata.name1 = playerInfo.Behaviour.NickName1.ToString();
        if (playerInfo.Behaviour.NickName2.ToString() != "") playerdata.name2 = playerInfo.Behaviour.NickName2.ToString();
    }
    public static void ChangeWaitingSceneName(Changed<NetworkDatas> playerInfo)
    {
        string name = playerInfo.Behaviour.NickName.ToString();
        if (!playerInfo.Behaviour.HasStateAuthority && name != "")
        {
            GameObject otherNameObject = GameObject.Find("OtherName");
            if (otherNameObject)
            {
                var otherNameText = otherNameObject.GetComponent<TextMeshProUGUI>();
                otherNameText.text = name;
            }
        }
    }
    public static void RecievePlaying(Changed<NetworkDatas> playerInfo)
    {
        if(!playerInfo.Behaviour.HasStateAuthority && playerInfo.Behaviour.playButton == true) //他の人がスタートボタンを押したら呼び出される。
        {
            FindAnyObjectByType<WaitingManager>().checkedNum++;
        }
    }
    public static void RecievePictureId(Changed<NetworkDatas> playerInfo)
    {

        if (!playerInfo.Behaviour.HasStateAuthority) //let's start の時。
        {
            OtherPicture newScript = FindObjectOfType<OtherPicture>();
            if(newScript != null) newScript.ChangeOtherPicture(playerInfo.Behaviour.pictureId - 1); //インクリメントした分デクリメント
            //GameObject.Find("OtherSelectedCharacter").GetComponent<>
        }
    }
    public void NPCMode()
    {
        NickName2 = "AIくん";
    }
}
