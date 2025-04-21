using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using static Unity.VisualScripting.Member;

public class NPCPlayerMove : NetworkBehaviour
{


    [SerializeField] private Vector3 localGravity;
    [SerializeField] ParticleSystem particleObject;
    [SerializeField] GameObject test;

    [NonSerialized]
    public GameObject ball;
    public GameObject player;
    private playerManager playermanager;
    private Rigidbody rBody;
    private bool lerped;
    private Vector3 lerp_vec;
    private float speed, range;
    public float HP;
    private Vector3 digrees;
    Camera cam;
    [System.NonSerialized]
    private Animator playerAnimator;
    private bool IsAnimation;
    private int AnimationKind;
    float speed_an;
    bool isjump;
    bool onfloar;
    Vector2 movingDirection;
    public Vector3 movingVelocity;
    int touched;
    GameObject service_object;
    private GameObject playerdata;
    ServiceManager servicemanager;
    //ServeManager servemanager;
    public int id;
    private int serverId;
    private Quaternion hittingRotate;
    public NetworkMecanimAnimator networkAnimator;
    private FloatingJoystick MovingJoyStick;
    private string device;
    private Vector3 destination;
    private Vector2 desVec;
    private float preDistance;
    private bool waitHitting;
    private bool emergencyHit;
    //public Dynami variableJoystick;

    private GameObject obj;
    public GameObject predictObj;



    public enum NPCMovingPhase
    {
        Waiting,//待機
        Chasing,//ボールを追いかける
        Hitting,//ボールを打つ
        Backing//位置を戻す
    }

    public enum BattlePhase
    {
        Waiting,
        PlayerService,
        NPCService,
        Raley,
    }
    public NPCMovingPhase NPCMoving = 0;
    public BattlePhase battlePhase = 0;
    private void Awake()
    {
        rBody = this.GetComponent<Rigidbody>();
        cam = Camera.main;
    }
    private void Start()
    {
        obj = Instantiate((GameObject)Resources.Load("destination"));
        predictObj = Instantiate((GameObject)Resources.Load("PredictBall"));
        playerAnimator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        lerped = true;
        playermanager = FindObjectOfType<playerManager>();
        PlayerData playerdataScript = FindObjectOfType<PlayerData>();
        playerdata = playerdataScript.gameObject;
        speed = playerdataScript.NPCStatus.speed;
        range = playerdataScript.NPCStatus.range;
        HP = playerdataScript.NPCStatus.hitPoint;
        servicemanager = GameObject.Find("service").GetComponent<ServiceManager>();
        serverId = servicemanager.ServerId;
        device = playermanager.Device;
        if (device != "PC") MovingJoyStick = GameObject.Find("MoveJoystick").GetComponent<FloatingJoystick>();
        if (serverId == 2)
        {
            BattlePhaseChange(BattlePhase.NPCService);
        }
    }


    public void FixedUpdate()
    {
        if (IsAnimation || NPCMoving == NPCMovingPhase.Waiting)
        {
            rBody.linearVelocity = Vector3.zero;
            return;
        }
        float x = 0, z = 0;
        if (device == "PC")
        {
        }
        else if (device != "")
        {
            x = MovingJoyStick.Horizontal;
            z = MovingJoyStick.Vertical;
        }
        if (ball)
        {
            Vector2 dt = desVec.normalized;
            Quaternion quaternion = Quaternion.identity;
            quaternion = Quaternion.Euler(dt);
            movingDirection = dt;
        }

        if (HP <= 0)
        {
            movingVelocity = new Vector3(movingDirection.x, 0, movingDirection.y) * (speed * 0.5f);

        }
        else
        {
            movingVelocity = new Vector3(movingDirection.x, 0, movingDirection.y) * speed;
        }

        var nowPos = transform.position;

        if (tossWait == true) //トスの移動制限
        {
            movingVelocity.z = 0;

            if (servePosKind == 1)
            {
                nowPos.x = Mathf.Clamp(nowPos.x, 0, 40);

            }
            else if (servePosKind == 2)
            {
                nowPos.x = Mathf.Clamp(nowPos.x, -40, 0);

            }
        }
        //移動制限
        if (id == 1)
        {
            nowPos.z = Mathf.Clamp(nowPos.z, -150, -5);

        }
        else if (id == 2)
        {
            nowPos.z = Mathf.Clamp(nowPos.z, 5, 150);

        }

        nowPos.x = Mathf.Clamp(nowPos.x, -95, 95);

        transform.position = nowPos;
        rBody.linearVelocity = new Vector3(movingVelocity.x, rBody.linearVelocity.y, movingVelocity.z);

        if (movingVelocity != Vector3.zero)
        {
            gameObject.transform.forward = movingVelocity;
        }
        if (Vector3.Distance(this.transform.position, destination) < 4)
        {
            NPCMoving = NPCMovingPhase.Waiting;
            waitHitting = true;
        }
    }
    private void NPCMove()
    {
        if (NPCMoving == NPCMovingPhase.Chasing)
        {

        }
    }
    /// <summary>
    /// フェーズ,ボールの種類,始点,ベクトル,サーブか否か,から適正の目的地ベクトルを算出
    /// </summary>
    /// <param name="newPhase"></param>
    /// <param name="Ballkind"></param>
    /// <param name="HitPoint"></param>
    /// <param name="HitVec"></param>
    public async void NPCDestination(NPCMovingPhase newPhase, int Ballkind, Vector3 HitPoint, Vector3 HitVec, bool serve)
    {
        NPCMoving = newPhase;
        preDistance = Vector2.Distance(new Vector2(destination.x, destination.z), new Vector2(ball.transform.position.x, ball.transform.position.z)); ;
        Vector3 des = Vector3.zero;
        if (newPhase == NPCMovingPhase.Waiting)
        {
            Vector3 posMe = this.transform.position;
            des = this.transform.position;
            des.x = 0;
            des.z = 80;
            destination = des;
            desVec = new Vector2(destination.x - posMe.x, destination.z - posMe.z);
            NPCMoving = NPCMovingPhase.Backing;
        }
        if (newPhase == NPCMovingPhase.Chasing)
        {

            //内積によりボールへの最短距離を取得(clampにより内積変数kを制限)尚,ベクトルAB(ボールの方向ベクトル)の大きさを1(Normalize)することにより媒介変数kでの計算を容易化
            //また,kの値を変更することによってどの位置でとるかを決めることがでいる
            Vector3 posMe = this.transform.position;
            Vector3 posBall = HitPoint;
            posBall.y = posMe.y;
            Vector3 posNormal = HitVec;
            posNormal.y = 0;

            float k = Vector3.Dot(posMe - posBall, posNormal.normalized);
            k += posBall.z;
            //kの制限範囲は実際の座標をclampに入力
            if (!serve)
            {
                switch (Ballkind)
                {
                    case 1://ドライブ
                        break;
                    case 2://ロブ
                        k = Mathf.Clamp(k, 70, 150);
                        break;
                    case 3://スライス
                        k = Mathf.Clamp(k, 70, 71);
                        BallPredictManager BPM = predictObj.GetComponent<BallPredictManager>();
                        BPM.ThisAddVel(HitPoint, HitVec);
                        await UniTask.WaitUntil(() => predictObj.transform.position.z > k);
                        print("complete");
                        destination = predictObj.transform.position;
                        predictObj.transform.position = Vector3.one;
                        break;
                    case 4://ドロップ
                        k = Mathf.Clamp(k, 10, 50);
                        break;
                    case 5://ダメボール
                        k = 36;
                        break;
                }

            }
            else
            {
                switch (Ballkind)
                {
                    case 1://ドライブ
                        k = Mathf.Clamp(k, 60, 100);
                        break;
                    case 3://スライス
                        break;

                }
            }
            k -= posBall.z;
            if (Ballkind != 3) destination = posBall + k * posNormal.normalized;
            destination.y = this.transform.position.y;
            desVec = new Vector2(destination.x - posMe.x, destination.z - posMe.z);
            print(desVec);
        }
    }

    private void HittingFunction()
    {
        if (!waitHitting || battlePhase != BattlePhase.Raley) return;
        if (!ball) return;
        float distance = Vector2.Distance(new Vector2(destination.x, destination.z), new Vector2(ball.transform.position.x, ball.transform.position.z));
        if (distance < 2 && emergencyHit == false)
        {
            int animeKind;
            if (ball.transform.position.x > this.transform.position.x) animeKind = -1;
            else animeKind = 1;

            float timing_dis = this.transform.position.z - ball.transform.position.z;
            float timing_dis_x = ball.transform.position.x - this.transform.position.x; //0中心
            float timing_dis_sqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_x, 2));  //0中心
            int rnd = UnityEngine.Random.Range(1, 5);
            print(rnd + "random");
            bool canHit = ball.GetComponent<BallMove>().TryNPCHit(animeKind, timing_dis_sqrt, rnd);
            if (canHit == true)
            {
                NPCDestination(NPCMovingPhase.Waiting, 1, Vector3.zero, Vector3.zero, false);
                waitHitting = false;

            }
        }
        if (emergencyHit && preDistance < range * 3)
        {
            int animeKind;
            if (ball.transform.position.x > this.transform.position.x) animeKind = -1;
            else animeKind = 1;

            //float timing_dis = this.transform.position.z - ball.transform.position.z;
            //float timing_dis_x = ball.transform.position.x - this.transform.position.x; //0中心
            //float timing_dis_sqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_x, 2));  //0中心
            int rnd = UnityEngine.Random.Range(1, 5);
            print(rnd + "randomEmergency");
            print(preDistance);
            bool canHit = ball.GetComponent<BallMove>().TryNPCHit(animeKind, preDistance, rnd);
            if (canHit == true)
            {
                NPCDestination(NPCMovingPhase.Waiting, 1, Vector3.zero, Vector3.zero, false);
                waitHitting = false;

            }
        }
        emergencyHit = false;
    }

    private void EmergencyHitFunc()
    {
        if (battlePhase != BattlePhase.Raley) return;
        if (!ball) return;
        //Chasingの時にボールとNPCの距離が離れ始めた瞬間打つ
        float Ball_Character_Dis = Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(ball.transform.position.x, ball.transform.position.z));
        if (NPCMoving == NPCMovingPhase.Chasing && Ball_Character_Dis > preDistance && Ball_Character_Dis < range * 3)
        {
            emergencyHit = true;
            waitHitting = true;
            NPCMoving = NPCMovingPhase.Waiting;
            print("わあわw");
        }
        preDistance = Ball_Character_Dis;
    }

    void Update()
    {
        obj.transform.position = new Vector3(destination.x, 8, destination.z);
        EmergencyHitFunc();
        move();
        HittingFunction();
        if (lerped == false)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, lerp_vec, 10 * Time.deltaTime); // 目的の位置に移動
            if (lerp_vec.x == transform.position.x && lerp_vec.z == transform.position.z) lerped = true;
        }

    }

    private async void ServeFunc()
    {

        await UniTask.Delay(TimeSpan.FromSeconds(1));
        ball.GetComponent<BallMove>().NPCWillToss = true;
        await UniTask.WaitUntil(() => ball.transform.position.y >= 20);
        ball.GetComponent<BallMove>().NPCService(1);
        print("aasfasdffa");
    }

    public async void Player_Move_to_Ball(Vector3 lerp_pos, Quaternion rotate, bool lerping)
    {
        lerped = false;
        lerp_vec = lerp_pos;
        hittingRotate = rotate;
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        NPCDestination(NPCMovingPhase.Waiting, 1, Vector3.zero, Vector3.zero, false);
        lerped = true;
    }

    public async void BattlePhaseChange(BattlePhase _BattlePhase)
    {
        if (battlePhase == _BattlePhase) return;
        battlePhase = _BattlePhase;
        switch (_BattlePhase)
        {
            case BattlePhase.Waiting:
                break;
            case BattlePhase.PlayerService:
                break;
            case BattlePhase.NPCService:
                await UniTask.Delay(TimeSpan.FromSeconds(3));
                print("a");
                bool exist = player.GetComponent<PlayerService>().NPCMakeBall();
                print("fsfsa" + exist);
                ServeFunc();
                break;
            case BattlePhase.Raley:
                break;
        }
    }

    Vector3 latestPos;



    private bool tossWait;
    private int servePosKind;
    public void TossWait(int posKind)
    {
        if (servicemanager.ServerId == 2)
        {
            servePosKind = posKind;
        }
    }

    void move()
    {
        speed_an = rBody.linearVelocity.magnitude;
        if (playerAnimator) playerAnimator.SetFloat("Speed", speed_an);
    }

    public void thisDespawn()
    {
        ball = GameObject.Find("Ball");
        if (ball == true)
        {
            ball.GetComponent<BallMove>().Destroy();
        }
        Destroy(playerdata);
        Destroy(playermanager.gameObject);
        Runner.Despawn(this.GetComponent<NetworkObject>());
    }


    /// <summary>
    /// Kind = 1フォア,-1バック,2トス待機,3トス,4サーブ,0スマッシュ,-10サーブ中断-------------------
    /// BallKind = 1ドライブ,2ロブ,3スライス4,ドロップ,5だめ
    /// </summary>
    /// <param name="kind">大まかな分類</param>
    /// <param name="BallKind">細かい分類(カットかドライブかなど)</param>
    public void StartAnimation(int kind, int BallKind)
    {
        float time = 0;
        IsAnimation = true;
        if (kind == 1)
        {
            if (BallKind == 2)
            {
                playerAnimator.SetTrigger("ForeLob");

            }
            else
            {
                playerAnimator.SetTrigger("ForeStroke");
            }
            time = 1;
            Invoke("EndAnimation", time);
            AnimationKind = kind;

        }
        else if (kind == -1)
        {
            if (BallKind == 2)
            {
                playerAnimator.SetTrigger("BackLob");

            }
            else if (BallKind == 3)
            {
                playerAnimator.SetTrigger("BackCut");
            }
            else
            {
                playerAnimator.SetTrigger("BackStroke");
            }
            time = 1;
            Invoke("EndAnimation", time);
            AnimationKind = kind;

        }
        if (kind == 2)
        {
            IsAnimation = false;
            tossWait = true;
            playerAnimator.SetTrigger("TossWait");
            time = 0.1f;
        }
        if (kind == 3)
        {
            tossWait = false;
            playerAnimator.SetTrigger("Toss");
            time = 0.2f;

        }
        if (kind == 4)
        {
            playerAnimator.SetTrigger("Serve");
            time = 0.4f;
            AnimationKind = 4;
            Invoke("EndAnimation", 0.4f);

        }
        if (kind == 0)
        {
            playerAnimator.SetTrigger("Smash");
            time = 1;
            Invoke("EndAnimation", time);
            AnimationKind = kind;
        }
        if (kind == -10)
        {
            IsAnimation = false;
            playerAnimator.SetTrigger("TossExit");
        }
    }
    private void EndAnimation()
    {
        if (AnimationKind == 1)
        {
            IsAnimation = false;
        }
        else if (AnimationKind == -1)
        {
            IsAnimation = false;

        }
        if (AnimationKind == 2)
        {
            IsAnimation = false;
        }
        if (AnimationKind == 3)
        {
            AnimationKind = 4;

        }
        if (AnimationKind == 4)
        {
            IsAnimation = false;

        }
        if (AnimationKind == 0)
        {
            IsAnimation = false;
        }
    }
    private void SetLocalGravity()
    {
        rBody.AddForce(localGravity, ForceMode.Acceleration);
    }


}
