using System;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static BallMove;

public class PlayerNetworkedChange : NetworkBehaviour
{

    //  定数・フィールド
    // ──────────────────────────────────────────────────────────────
    private const int STARTING_LIVES = 3;
    private int count;
    private int id;
    private bool gameEndLocal;

    private GameObject ball;
    private PlayerOverviewPanel overviewPanel;

    public enum PlayPhase { Wait, TossWait, Toss, Serve, Raley, RaleyEnd }
    public enum BallKinds { Nothing, Drive, Lob, Slice, Drop, Bad }


    [Networked, OnChangedRender(nameof(OnPhaseChanged))]
    public PlayPhase CurrentPhase { get; set; }

    [Networked, OnChangedRender(nameof(OnBallKindChanged))]
    public BallKinds CurrentBall { get; set; }

    [Networked, OnChangedRender(nameof(OnHitNumChanged))]
    public int HitNumNetworked { get; set; }

    [Networked, OnChangedRender(nameof(OnMissInfoChanged))]
    public int MissInfoNetworked { get; set; }

    [HideInInspector]
    [Networked, OnChangedRender(nameof(OnScoreChanged))]
    public int Score1 { get; private set; }

    [HideInInspector]
    [Networked, OnChangedRender(nameof(OnScoreChanged))]
    public int Score2 { get; private set; }

    // ──────────────────────────────────────────────────────────────
    //  Unity / Fusion Lifecycle
    // ──────────────────────────────────────────────────────────────
    private void Start()
    {
        CurrentPhase = PlayPhase.Wait;
        CurrentBall = BallKinds.Nothing;
        HitNumNetworked = 0;
        ball = gameObject;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            id = FindObjectOfType<PlayerService>().id;
        }

        Score1 = 0;
        Score2 = 0;

        overviewPanel = FindObjectOfType<PlayerOverviewPanel>();
        overviewPanel.AddEntry(Object.InputAuthority, this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        overviewPanel.RemoveEntry(Object.InputAuthority);
    }


    private void OnPhaseChanged()
    {
        if (CurrentPhase == PlayPhase.Wait) return;

        var bm = ball ? ball.GetComponent<BallMove>() : null;
        if (bm != null)
        {
            bm.CurrentPhaseInt = (int)CurrentPhase;
            if (CurrentPhase == PlayPhase.Serve)
                bm.floar_collision = 0;
        }

        count++;
    }

    private void OnBallKindChanged()
    {
        if (CurrentBall == BallKinds.Nothing) return;

        var bm = ball ? ball.GetComponent<BallMove>() : null;
        if (bm != null)
            bm.CurrentBallKind = (int)CurrentBall;
    }

    private void OnHitNumChanged()
    {
        Debug.Log($"HitNum updated: {HitNumNetworked}");
    }

    private async void OnMissInfoChanged()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.2));

        var bm = ball.GetComponent<BallMove>();
        if (id != bm.server_id) return;

        int missPointNum = MissInfoNetworked;

        for (int i = 0; i < bm.PointInfoList.Count; i++)
        {
            if (bm.PointInfoList[i].hittedNum == missPointNum)
            {
                var info = bm.PointInfoList[i];
                info.recievedNum++;
                bm.PointInfoList[i] = info;
                break;
            }
        }
    }

    private void OnScoreChanged()
    {
        overviewPanel.UpdateScore(Object.InputAuthority, Score1, Score2);
    }

    // ──────────────────────────────────────────────────────────────
    //  RPCs
    // ──────────────────────────────────────────────────────────────
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ChangeCurrentBallKinds(BallKinds kind)
    {
        CurrentBall = kind;
    }


    public void ScoreChange(int sc1, int sc2)
    {
        Score1 = sc1;
        Score2 = sc2;
    }
}
