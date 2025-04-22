using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static BallMove;
using System;

public class PlayerNetworkedChange : NetworkBehaviour
{
    int count;
    public enum PlayPhase
    {
        Wait,
        TossWait, //トス待機フェーズ
        Toss, // トスフェーズ
        Serve,//サーブフェーズ
        Raley,//ラリーフェーズ
        RaleyEnd,// ラリー終了フェーズ
    }

    public enum BallKinds
    {
        Nothing,//なし
        Drive,//ドライブボール
        Lob, //ロブボール
        Slice, // スライスボール
        Drop,//ドロップボール
        Bad, //ダメボール

    }

    [Networked(OnChanged = nameof(PhaseChanged))]
    public PlayPhase CurrentPhase { get; set; }

    [Networked(OnChanged = nameof(BallKindChange))]
    public BallKinds CurrentBall { get; set; }

    [Networked(OnChanged = nameof(HitNumChanged))]
    public int HitNumNetworked { get; set; }

    [Networked(OnChanged = nameof(MissInfoChanged))]
    public int MissInfoNetworked { get; set; } //サーバー以外がミスの判定をする時に用いる(数値はHitNumNetworked)

    private const int STARTING_LIVES = 3;
    private int id;
    private PlayerOverviewPanel _overviewPanel = null;

    private GameObject Ball;


    [HideInInspector]
    [Networked(OnChanged = nameof(OnScoreChanged))]
    public int Score1 { get; private set; }
    [HideInInspector]
    [Networked(OnChanged = nameof(OnScoreChanged))]
    public int Score2 { get; private set; }


    private bool GameEnd_NotNetworked;
    private void Start()
    {
        CurrentPhase = PlayPhase.Wait;
        CurrentBall = BallKinds.Nothing;
        HitNumNetworked = 0;
        Ball = this.gameObject;
    }
    public override void Spawned()
    {
        // Initialized game specific settings
        if (Object.HasStateAuthority)
        {
            var nickName = FindObjectOfType<PlayerData>().GetNickName();
            id = GameObject.Find("player").GetComponent<PlayerService>().id;

        }
        Score1 = 0;
        Score2 = 0;

        _overviewPanel = FindObjectOfType<PlayerOverviewPanel>();

        _overviewPanel.AddEntry(Object.InputAuthority, this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _overviewPanel.RemoveEntry(Object.InputAuthority);
    }

    public static async void MissInfoChanged(Changed<PlayerNetworkedChange> playerInfo)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        var ballmove = playerInfo.Behaviour.gameObject.GetComponent<BallMove>();
        if (playerInfo.Behaviour.id != ballmove.server_id) return;
        int missPointNum = playerInfo.Behaviour.MissInfoNetworked;
        print("一致前!!");

        int listNum = ballmove.PointInfoList.Count;
        for (int i = 0; i < listNum; i++)
        {
            //ミスした情報が送信された時の打った回数が一致していたら構造体のrecievedNum(受信回数)をインクリメントする
            if (ballmove.PointInfoList[i].hittedNum == missPointNum)
            {
                print(ballmove.PointInfoList[i].hittedNum);
                PointInfo newPointinfo = ballmove.PointInfoList[i];
                newPointinfo.recievedNum++; //構造体のListが参照型ではないため構造体そのものしか書き換えるとしかできない
                ballmove.PointInfoList[i] = newPointinfo;
                print("一致!!");
            }
        }
    }

    public static void HitNumChanged(Changed<PlayerNetworkedChange> playerInfo)
    {
        print(playerInfo.Behaviour.HitNumNetworked);

    }

    public static void PhaseChanged(Changed<PlayerNetworkedChange> playerInfo)
    {
        if (playerInfo.Behaviour.CurrentPhase == PlayPhase.Wait) return;
        if (playerInfo.Behaviour.Ball != null)
        {
            var script = playerInfo.Behaviour.Ball.GetComponent<BallMove>();
            script.CurrentPhaseInt = ((int)playerInfo.Behaviour.CurrentPhase);
            if (playerInfo.Behaviour.CurrentPhase == PlayPhase.Serve) script.floar_collision = 0;
        }
        playerInfo.Behaviour.count++;
        //print(playerInfo.Behaviour.CurrentPhase + " :  "+ playerInfo.Behaviour.Ball.GetComponent<BallMove>().CurrentPhaseInt + "  " + playerInfo.Behaviour.count.ToString());

    }

    public static void BallKindChange(Changed<PlayerNetworkedChange> playerInfo)
    {
        if (playerInfo.Behaviour.CurrentBall == BallKinds.Nothing) return;
        if (playerInfo.Behaviour.Ball != null) playerInfo.Behaviour.Ball.GetComponent<BallMove>().CurrentBallKind = ((int)playerInfo.Behaviour.CurrentBall);
        print(playerInfo.Behaviour.Ball.GetComponent<BallMove>().CurrentBallKind + "  " + playerInfo.Behaviour.count);
    }

    public void ScoreChange(int sc1, int sc2)
    {

        Score1 = sc1;
        Score2 = sc2;
    }
    [Rpc(RpcSources.Proxies, RpcTargets.All)]
    public void Rpc_HitNumIncrement()
    {
        HitNumNetworked++;
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    public void Rpc_MissNumChange()
    {
        MissInfoNetworked = HitNumNetworked;
    }

    public static void OnScoreChanged(Changed<PlayerNetworkedChange> playerInfo)
    {
        playerInfo.Behaviour._overviewPanel.UpdateScore(playerInfo.Behaviour.Object.InputAuthority,
            playerInfo.Behaviour.Score1,
            playerInfo.Behaviour.Score2);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ChangeCurrentBallKinds(BallKinds ballkinds)
    {
        CurrentBall = ballkinds;
    }
}
