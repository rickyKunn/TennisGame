
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using TMPro;

public class NetworkDatas : NetworkBehaviour
{
    // ───────────────────────────────────────────────
    //  フィールド
    // ───────────────────────────────────────────────
    private int id;

    // ───────────────────────────────────────────────
    //  Networked プロパティ
    // ───────────────────────────────────────────────
    [HideInInspector]
    [Networked, OnChangedRender(nameof(ChangeWaitingSceneName))]
    public NetworkString<_16> NickName { get; private set; }

    [HideInInspector]
    [Networked, OnChangedRender(nameof(ChangeName))]
    public NetworkString<_16> NickName1 { get; private set; }

    [HideInInspector]
    [Networked, OnChangedRender(nameof(ChangeName))]
    public NetworkString<_16> NickName2 { get; private set; }

    [HideInInspector]
    [Networked, OnChangedRender(nameof(RecievePlaying))]
    public NetworkBool playButton { get; set; }

    [Networked, OnChangedRender(nameof(RecievePictureId))]
    public int pictureId { get; private set; }

    // ───────────────────────────────────────────────
    //  ローカル参照
    // ───────────────────────────────────────────────
    private TextMeshProUGUI myNameText;

    private void Start()
    {
        id = FindObjectOfType<playerManager>().id;

        if (GameObject.Find("MyName") is { } myNameObj)
            myNameText = myNameObj.GetComponent<TextMeshProUGUI>();
    }

    // ───────────────────────────────────────────────
    //  Public API
    // ───────────────────────────────────────────────
    public void IdChanged(int cameId)
    {
        id = FindObjectOfType<playerManager>().id;
        if (cameId != id) return;

        var nick = FindObjectOfType<PlayerData>().GetNickName();
        myNameText.text = nick;

        if (id == 1) NickName1 = nick;
        else if (id == 2) NickName2 = nick;

        NickName = nick;
    }

    public void CanPlay() => playButton = true;
    public void ChangePicureId(int pid) => pictureId = pid;   // 0 にならないよう保証
    public void NPCMode() => NickName2 = "AIくん";

    public override void FixedUpdateNetwork() { /* 何もしない */ }

    // ───────────────────────────────────────────────
    //  OnChanged コールバック
    // ───────────────────────────────────────────────

    public void ChangeName()
    {
        var playerData = FindObjectOfType<PlayerData>();

        if (!string.IsNullOrEmpty(NickName1.ToString()))
            playerData.name1 = NickName1.ToString();

        if (!string.IsNullOrEmpty(NickName2.ToString()))
            playerData.name2 = NickName2.ToString();
    }

    public void ChangeWaitingSceneName()
    {
        string name = NickName.ToString();

        if (!HasStateAuthority && !string.IsNullOrEmpty(name))
        {
            if (GameObject.Find("OtherName") is { } otherObj)
                otherObj.GetComponent<TextMeshProUGUI>().text = name;
        }
    }

    public void RecievePlaying()
    {
        if (!HasStateAuthority && playButton)
            FindAnyObjectByType<WaitingManager>().checkedNum++;
    }

    public void RecievePictureId()
    {
        if (!HasStateAuthority)
            FindObjectOfType<OtherPicture>()?.ChangeOtherPicture(pictureId - 1); // インクリメント分デクリメント
    }
}
