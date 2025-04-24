using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System;
using static PlayerNetworkedChange;
using static NPCPlayerMove;
using UnityCoreHaptics;
using UnityEngine.SceneManagement;


public class BallMove : NetworkBehaviour
{
    [Networked]
    private float NetworkedSpeed { get; set; }

    [SerializeField]
    private GameObject outObj, faultObj, TwoBoundsObj, DoubleFaultObj;
    private playerAbility playerAbility;  //構造体
    private playerAbility NPCPlayerAbility;
    [HideInInspector]
    public int Score { get; private set; }
    //[HideInInspector]
    //public int test2;
    //[HideInInspector]
    //public int Testing1;
    private GameObject mainPlayer;
    [SerializeField]
    private GameObject perticle_perfect;
    [SerializeField]
    private GameObject perticle_good;

    private GameObject Judger;
    ScoreManager scoremanager;
    private GameObject service_Object;
    private PlayerMove playermove;
    private PlayerService playerservice;

    Rigidbody rBody;
    float timing_time;
    float timing_long;
    private float charge_time;
    private float timing_dis;
    private bool ball_hit;
    public bool rec_scoreChanged = false;

    public bool Play_End;
    private bool beforeToss, tossed, tossing;
    private int hitting_player;
    [SerializeField]
    public int floar_collision;

    [HideInInspector]
    public bool service_hit = false;

    private bool ball_make;
    public int server_id;
    private bool ServiceJugde;
    private int FaultNum;
    public bool deuce = false;
    private Vector3 _prevPosition;
    NetworkObject girl_Network;
    ServiceManager servicemanager;
    PlayerNetworkedChange PlNwCh;
    BallTrailManager BTM;
    playerManager playermanager;
    [SerializeField]
    private AudioClip hit1, hit2, hit3, boundSound;
    int SoundKind;
    AudioSource audioSource;
    public int id, ball_hitting_player;
    private bool receiver_FirstHit = false;
    private bool End;
    private float collisionTime;
    private CameraMove cameramove;
    GameObject Cube;
    private float timeManager;
    private int servePosKind;
    public int CurrentPhaseInt; //現在の整数フェーズ管理(間接的な同期) PlayerNetworkedChanged.csから変更してるよ〜ん
    public int CurrentBallKind;
    private string device;
    private PlayerNetworkedChange PlNetChange;

    private bool NPCMode;
    private GameObject NPCPlayer;
    private NPCPlayerMove NPM;

    private PhysicsScene physicsScene;
    private PhysicsScene physicsScenePredict;
    public UnityEngine.SceneManagement.Scene predictScene;
    SceneManager scenemanager;
    private const string TossPointName = "TossPoint";

    [Tooltip("ボールの見た目に使っている全ての Renderer をここにドラッグ or 自動取得")]
    [SerializeField] private Renderer[] _renderers;

    public struct PointInfo //判断同期構造体
    {
        public int PointedId; //得点したID
        public int hittedNum; //打った回数
        public string missKind; //失点原因
        public Vector3 bouncePos; //失点原因となった着地地点の座標
        public int recievedNum; //受信した回数(プレイヤー人数と等しくなったら失点実行)
    }

    public List<PointInfo> PointInfoList = new List<PointInfo>(); //構造体のリスト

    void Awake()
    {
        // Inspector からセットされていなければ、自動で子階層から拾ってくる
        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = GetComponentsInChildren<Renderer>();
        }
    }
    void Start()
    {
        NetworkedSpeed = 1;
        this.name = "Ball";
        _prevPosition = transform.position;
        rBody = this.GetComponent<Rigidbody>();
        mainPlayer = GameObject.Find("player");
        playermove = mainPlayer.GetComponent<PlayerMove>();
        girl_Network = mainPlayer.GetComponent<NetworkObject>();
        cameramove = mainPlayer.GetComponent<CameraMove>();
        servicemanager = GameObject.Find("service").GetComponent<ServiceManager>();
        playerservice = mainPlayer.GetComponent<PlayerService>();
        Judger = GameObject.Find("judger");
        scoremanager = Judger.GetComponent<ScoreManager>();
        id = playerservice.id;
        server_id = servicemanager.ServerId;
        PlNwCh = this.GetComponent<PlayerNetworkedChange>();
        BTM = this.GetComponent<BallTrailManager>();
        audioSource = GetComponent<AudioSource>();
        beforeToss = false;
        tossed = false;
        PlNetChange = this.GetComponent<PlayerNetworkedChange>();
        //トス(サーバーのみRPC送信)
        this.transform.position += new Vector3(0, 5, 0);
        //レシーバーはtrueに
        if (server_id != id)
        {
            service_hit = true;
            rec_scoreChanged = true;
        }

        Rpc_SetGravity(false);
        playerAbility = FindObjectOfType<PlayerData>().status; //playerdataで宣言,代入した構造体をここで新たに取得
        NPCPlayerAbility = FindObjectOfType<PlayerData>().NPCStatus; //playerdataで宣言,代入した構造体をここで新たに取得
        playermanager = FindObjectOfType<playerManager>();
        device = playermanager.Device;
        NPCMode = playermanager.playerManagerNPCMode;
        if (NPCMode)
        {

            NPCPlayer = GameObject.Find("NPCPlayer");
            NPM = NPCPlayer.GetComponent<NPCPlayerMove>();
            NPM.ball = this.gameObject;
            id = 1;
        }

        obj = (GameObject)Resources.Load("destination");
        if (playermanager.subSceneMade == false)
        {
            playermanager.SubScene = SceneManager.CreateScene("SubScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            playermanager.subSceneMade = true;
        }
        UnityEngine.SceneManagement.Scene scene = playermanager.SubScene;
        physicsScene = scene.GetPhysicsScene();
        SceneManager.MoveGameObjectToScene(this.gameObject, scene);
        GameObject floar = GameObject.Find("floar");
        SceneManager.MoveGameObjectToScene(floar, scene);

        //if (NPCMode)
        //{
        //    predictScene = playermanager.PredictScene;
        //    physicsScenePredict = predictScene.GetPhysicsScene();
        //    GameObject predictFloar = GameObject.Find("predictFloar");
        //    SceneManager.MoveGameObjectToScene(predictFloar, predictScene);
        //    SceneManager.MoveGameObjectToScene(NPM.predictObj, predictScene);
        //}
    }

    GameObject obj;

    private bool vel_turned;
    float testing;
    private void FixedUpdate()
    {
        physicsScene.Simulate(Time.fixedDeltaTime * NetworkedSpeed);
    }
    void Update()
    {
        //Time.timeScale = 1.5f;
        collisionTime += Time.deltaTime;
        timeManager += Time.deltaTime;
        Receiver_functions();
        if (!girl_Network.HasStateAuthority) return;
        id = playerservice.id;

        if (service_hit == true)
        {
            HittingPlayer_change();
            ball_kinds();
            Ball_out();
            if ((id == server_id || NPCMode) && Play_End == false)
            {
                JudgeMiss();
            }

        }

        if (id == server_id) //サーブ等
        {
            if (beforeToss == true)
            {
                playermove.TossWait(servePosKind);
                transform.position = mainPlayer.transform.position + new Vector3(0, 5, 0);

            }
            if (service_hit == false && tossed == true)
            {
                if (device == "PC")
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(0)) Service(1);
                    else if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)) Service(3);
                }
                else
                {
                    if (drivePressed) Service(1);
                    else if (slicePressed) Service(3);
                }

            }
            if (tossed == false) Toss();
        }
        if (NPCMode)
        {
            if (server_id == 2)
            {
                if (beforeToss == true)
                {
                    NPM.TossWait(servePosKind);
                    this.transform.position = NPCPlayer.transform.position + transform.up * 7 + NPCPlayer.transform.forward * 3.6f;
                }
            }
            if (tossed == false) Toss();
        }
        if (device != "PC")
        {
            drivePressed = false;
            slicePressed = false;
            dropPressed = false;
            lobPressed = false;
        }


        //TossWaitフェーズの時はボールを透明にする(改善が必要)
        var phase = GetComponent<PlayerNetworkedChange>().CurrentPhase;

        bool visible = phase != PlayPhase.TossWait;
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].enabled = visible;
        }

    }


    private void JudgeMiss()
    {

        int ListNum = PointInfoList.Count;
        if (!playermanager)
        {
            playermanager = FindObjectOfType<playerManager>();
            return;
        }
        for (int i = 0; i < ListNum; i++)
        {

            if (PointInfoList[i].recievedNum == playermanager.PlayerNum)
            {
                int pointedId = PointInfoList[i].PointedId;
                string missName = PointInfoList[i].missKind;
                this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.RaleyEnd; //networkPropertyの列挙体
                if (NPCMode)
                {
                    NPM.BattlePhaseChange(BattlePhase.Waiting);
                }
                if (missName == "2bounds")
                {
                    if (pointedId == 1)
                    {
                        scoremanager.p1Score++;

                    }
                    else if (pointedId == 2)
                    {
                        scoremanager.p2Score++;
                    }
                    Rpc_InsPic(3, this.transform.position);
                    floar_collision = 0;
                    Play_End = true;
                    service_hit = false;
                    tossed = false;
                    beforeToss = false;
                    timeManager = 0;
                    vel_turned = false;
                    PointInfoList.Clear();
                    ListNum = 0;
                }
                else if (missName == "out")
                {
                    if (pointedId == 1)
                    {
                        scoremanager.p1Score++;

                    }
                    else if (pointedId == 2)
                    {
                        scoremanager.p2Score++;
                    }
                    Rpc_InsPic(1, this.transform.position);
                    floar_collision = 0;
                    Play_End = true;
                    service_hit = false;
                    tossed = false;
                    beforeToss = false;
                    timeManager = 0;
                    vel_turned = false;
                    PointInfoList.Clear();
                    ListNum = 0;

                }
                else if (missName == "Nothing")
                {
                    if (pointedId == 1)
                    {
                        scoremanager.p1Score++;

                    }
                    else if (pointedId == 2)
                    {
                        scoremanager.p2Score++;
                    }
                    floar_collision = 0;
                    Play_End = true;
                    service_hit = false;
                    tossed = false;
                    beforeToss = false;
                    timeManager = 0;
                    vel_turned = false;
                    PointInfoList.Clear();
                    ListNum = 0;
                }
                if (NPCMode && server_id == 2)
                {
                    NPM.BattlePhaseChange(BattlePhase.NPCService);
                }
            }
        }
    }

    public void Destroy()
    {
        Runner.Despawn(this.GetComponent<NetworkObject>());
    }
    public bool NPCWillToss;
    private async void Toss()
    {
        if (timeManager >= 0.6f && beforeToss == false)
        {
            if (id == server_id)
            {
                this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.TossWait; //networkPropertyの列挙体
                playermove.HP = playerAbility.hitPoint;
                if (NPCMode) NPM.HP = NPCPlayerAbility.hitPoint;
                EnterTossWait(); //物理挙動停止
                BTM.Rpc_ChangeColor(0, false);//Trailを透明に
                print("PLNWCH:" + PlNwCh);
                PlNwCh.ScoreChange(scoremanager.p1Score, scoremanager.p2Score);
                print($"{scoremanager.p1Score} - {scoremanager.p2Score}");

                if (scoremanager.p1Score >= 4 || scoremanager.p2Score >= 4)  //4点だったらDespawn
                {
                    if (deuce == false)
                    {
                        End = true;
                        servicemanager.serve_changed = true;
                        scoremanager.p1Score = 0;
                        scoremanager.p2Score = 0;
                        Invoke("Destroy", 2);
                        if (NPCMode) NPM.BattlePhaseChange(BattlePhase.NPCService);
                    }
                    else
                    {
                        int score1 = scoremanager.p1Score;
                        int score2 = scoremanager.p2Score;
                        int countDif = 0;
                        if (score1 > score2)
                        {
                            countDif = score1 - score2;

                        }
                        else if (score2 > score1)
                        {
                            countDif = score2 - score1;
                        }
                        else //デュース
                        {
                            countDif = 0;
                        }
                        if (countDif == 2)
                        {
                            End = true;
                            servicemanager.serve_changed = true;
                            scoremanager.p1Score = 0;
                            scoremanager.p2Score = 0;
                            Invoke("Destroy", 2);
                            if (NPCMode) NPM.BattlePhaseChange(BattlePhase.NPCService);
                        }
                    }
                }
                if (End == false)
                {
                    playermove.StartAnimation(2, 0);
                }
                if (id == 1)
                {
                    if ((scoremanager.p1Score + scoremanager.p2Score) % 2 == 0)
                    {
                        servePosKind = 1;
                        mainPlayer.transform.position = new Vector3(20, 1, -95);
                        if (NPCMode && !End)
                        {
                            NPCPlayer.transform.position = new Vector3(-30, 1, 95);
                            NPM.NPCMoving = NPCMovingPhase.Waiting;
                            if (NPCMode) NPM.BattlePhaseChange(BattlePhase.PlayerService);
                        }
                    }
                    else
                    {
                        servePosKind = 2;
                        mainPlayer.transform.position = new Vector3(-20, 1, -95);
                        if (NPCMode && !End)
                        {
                            NPCPlayer.transform.position = new Vector3(30, 1, 95);
                            NPM.NPCMoving = NPCMovingPhase.Waiting;
                            if (NPCMode) NPM.BattlePhaseChange(BattlePhase.PlayerService);
                        }

                    }
                }
                else if (id == 2)
                {
                    if ((scoremanager.p1Score + scoremanager.p2Score) % 2 == 0)
                    {
                        servePosKind = 2;
                        mainPlayer.transform.position = new Vector3(-20, 1, 95);
                    }
                    else
                    {
                        servePosKind = 1;
                        mainPlayer.transform.position = new Vector3(20, 1, 95);

                    }
                }
            }
            if (server_id == 2 && NPCMode)
            {
                this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.TossWait; //networkPropertyの列挙体
                playermove.HP = playerAbility.hitPoint;
                NPM.HP = NPCPlayerAbility.hitPoint;
                Rpc_SetGravity(false);
                BTM.Rpc_ChangeColor(0, false);//Trailを透明に
                PlNwCh.ScoreChange(scoremanager.p1Score, scoremanager.p2Score);
                print($"{scoremanager.p1Score} - {scoremanager.p2Score}");

                if (scoremanager.p1Score >= 4 || scoremanager.p2Score >= 4)  //4点だったらDespawn
                {
                    if (deuce == false)
                    {
                        End = true;
                        servicemanager.serve_changed = true;
                        scoremanager.p1Score = 0;
                        scoremanager.p2Score = 0;
                        Invoke("Destroy", 2);
                        if (NPCMode) NPM.BattlePhaseChange(BattlePhase.PlayerService);
                    }
                    else
                    {
                        int score1 = scoremanager.p1Score;
                        int score2 = scoremanager.p2Score;
                        int countDif = 0;
                        if (score1 > score2)
                        {
                            countDif = score1 - score2;

                        }
                        else if (score2 > score1)
                        {
                            countDif = score2 - score1;
                        }
                        else //デュース
                        {
                            countDif = 0;
                        }
                        if (countDif == 2)
                        {
                            End = true;
                            servicemanager.serve_changed = true;
                            scoremanager.p1Score = 0;
                            scoremanager.p2Score = 0;
                            Invoke("Destroy", 2);
                            if (NPCMode) NPM.BattlePhaseChange(BattlePhase.PlayerService);
                        }
                    }
                }
                if (End == false)
                {
                    NPM.StartAnimation(2, 0);
                }
                if (id == 1)
                {
                    if ((scoremanager.p1Score + scoremanager.p2Score) % 2 == 0)
                    {
                        servePosKind = 1;
                        mainPlayer.transform.position = new Vector3(20, 1, -95);
                        if (NPCMode && !End)
                        {
                            NPCPlayer.transform.position = new Vector3(-20, 1, 95);
                            NPM.NPCMoving = NPCMovingPhase.Waiting;
                        }
                    }
                    else
                    {
                        servePosKind = 2;
                        mainPlayer.transform.position = new Vector3(-20, 1, -95);
                        if (NPCMode && !End)
                        {
                            NPCPlayer.transform.position = new Vector3(20, 1, 95);
                            NPM.NPCMoving = NPCMovingPhase.Waiting;
                        }

                    }
                }
            }

            if (NPCMode)
            {
                NPM.player = mainPlayer;
            }
            beforeToss = true;
        }

        if (device == "PC" && (
            Input.GetKeyDown(KeyCode.LeftShift) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.LeftShift)
            // Input.GetMouseButtonDown(0) ||
            // Input.GetMouseButtonDown(1)
            )
            || device != "PC" && (slicePressed == true || drivePressed == true))
        {
            if (beforeToss == true && server_id == id)
            {
                if (End == true) return;

                this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.Toss; //networkPropertyの列挙体
                ExitTossWait();  //物理演算開始
                floar_collision = 0;
                ServiceJugde = false;
                beforeToss = false;
                tossed = true;
                tossing = true;
                service_hit = false;
                Play_End = false;
                playermove.StartAnimation(3, 0);
                mainPlayer.transform.rotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
                transform.position = mainPlayer.transform.position + new Vector3(0, 5, 0);
                Rpc_toss_addForce(Vector3.up * 40, mainPlayer.transform.position + new Vector3(0, 5, 0));

            }


        }
        if (NPCMode && NPCWillToss)
        {

            if (End == true) return;

            this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.Toss; //networkPropertyの列挙体

            Rpc_SetGravity(true);
            floar_collision = 0;
            ServiceJugde = false;
            beforeToss = false;
            tossed = true;
            tossing = true;
            service_hit = false;
            Play_End = false;
            NPM.StartAnimation(3, 0);
            //NPCPlayer.transform.rotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
            Rpc_toss_addForce(Vector3.up * 40, NPCPlayer.transform.position + transform.up * 5);


            NPCWillToss = false;
        }
    }


    private void Service(int kind)
    {
        service_hit = true;
        bool ServeStorngHit = false;
        Vector3 serve_Force = new Vector3(0, 0, 0);
        int SoundKind = 0;
        int serveLength = 0;
        float servePower = 0;
        float slicePower = 0;
        bool hitted = true;
        if (kind == 1) //ドライブ
        {
            Vector3 hit_point = new Vector3(this.transform.position.x, 21.6f, this.transform.position.z);
            float serve_distance = Mathf.Abs(Vector3.Distance(hit_point, this.transform.position));
            if (serve_distance < 2)
            {
                rBody.linearVelocity = Vector3.zero;
                ServeStorngHit = true;
                serveLength = 40;
                servePower = playerAbility.driveServePower;
                SoundKind = 2;
            }
            else if (serve_distance < 15)
            {
                rBody.linearVelocity = Vector3.zero;
                serveLength = 40;
                servePower = playerAbility.driveServePower + 0.3f;
                SoundKind = 1;
            }
            else
            {
                kind = 0;
                hitted = false;
            }
        }

        if (kind == 3) //スライス
        {
            ball_hit = true;
            Vector3 hit_point = new Vector3(this.transform.position.x, 21.6f, this.transform.position.z);
            float serve_distance = Mathf.Abs(Vector3.Distance(hit_point, this.transform.position));

            if (serve_distance < 2) //perfect
            {
                ServeStorngHit = true;
                serveLength = 40;
                servePower = playerAbility.sliceServePower;
                slicePower = playerAbility.sliceServeStrength * 2;
                SoundKind = 2;

            }
            else if (serve_distance < 15) //good
            {
                serveLength = 40;
                servePower = playerAbility.sliceServePower + 0.2f;
                slicePower = playerAbility.sliceServeStrength;
                SoundKind = 1;
            }
            else //bad
            {
                kind = 0;
                hitted = false;
            }
        }



        if (server_id == 1)
        {

            vel_turned = true;

        }
        else if (server_id == 2)
        {
            serveLength *= -1;
            slicePower *= -1;
            vel_turned = false;
        }
        if (device == "iOS" && ServeStorngHit == true)
        {
            UnityCoreHapticsProxy.PlayContinuousHaptics(1, 0.6f, 0.4f);

        }
        if (HasStateAuthority) PlNetChange.HitNumNetworked++;
        this.GetComponent<PlayerNetworkedChange>().CurrentBall = (BallKinds)kind;
        this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.Serve; //networkPropertyの列挙体
        serve_Force = Caculate_Force(serveLength, servePower); //サーブのベクトル計算
        if (slicePower != 0) Rpc_Set_Slice(slicePower, true);  //重力変更(スライス)
        ServiceJugde = true;
        tossing = false;
        BTM.Rpc_ChangeColor(kind, ServeStorngHit);//Trail
        mainPlayer.transform.rotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        playermove.StartAnimation(4, 0);
        Rpc_service_AddForce(serve_Force, SoundKind);

        if (NPCMode && hitted)
        {
            NPCPlayer.GetComponent<NPCPlayerMove>().NPCDestination(NPCMovingPhase.Chasing, kind, this.transform.position, serve_Force, true);
        }

    }

    public void NPCService(int kind)
    {
        service_hit = true;
        bool ServeStorngHit = false;
        Vector3 serve_Force = new Vector3(0, 0, 0);
        int SoundKind = 0;
        int serveLength = 0;
        float servePower = 0;
        float slicePower = 0;
        bool hitted = true;
        if (kind == 1) //ドライブ
        {
            Vector3 hit_point = new Vector3(this.transform.position.x, 21.6f, this.transform.position.z);
            float serve_distance = Mathf.Abs(Vector3.Distance(hit_point, this.transform.position));
            if (serve_distance < 2)
            {
                rBody.linearVelocity = Vector3.zero;
                ServeStorngHit = true;
                serveLength = 40;
                servePower = playerAbility.driveServePower;
                SoundKind = 2;
            }
            else if (serve_distance < 15)
            {
                rBody.linearVelocity = Vector3.zero;
                serveLength = 40;
                servePower = playerAbility.driveServePower + 0.3f;
                SoundKind = 1;
            }
            else
            {
                kind = 0;
                hitted = false;
            }
        }

        if (kind == 3) //スライス
        {
            ball_hit = true;
            Vector3 hit_point = new Vector3(this.transform.position.x, 21.6f, this.transform.position.z);
            float serve_distance = Mathf.Abs(Vector3.Distance(hit_point, this.transform.position));

            if (serve_distance < 2) //perfect
            {
                ServeStorngHit = true;
                serveLength = 40;
                servePower = playerAbility.sliceServePower;
                slicePower = playerAbility.sliceServeStrength * 2;
                SoundKind = 2;

            }
            else if (serve_distance < 15) //good
            {
                serveLength = 40;
                servePower = playerAbility.sliceServePower + 0.2f;
                slicePower = playerAbility.sliceServeStrength;
                SoundKind = 1;
            }
            else //bad
            {
                kind = 0;
                hitted = false;
            }
        }



        if (server_id == 1)
        {

            vel_turned = true;

        }
        else if (server_id == 2)
        {
            serveLength *= -1;
            slicePower *= -1;
            vel_turned = false;
        }

        if (HasStateAuthority) PlNetChange.HitNumNetworked++;
        this.GetComponent<PlayerNetworkedChange>().CurrentBall = (BallKinds)kind;
        this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.Serve; //networkPropertyの列挙体
        serve_Force = NPC_Serve_Caculate_Force(serveLength, servePower, 1); //サーブのベクトル計算
        if (slicePower != 0) Rpc_Set_Slice(slicePower, true);  //重力変更(スライス)
        ServiceJugde = true;
        tossing = false;
        BTM.Rpc_ChangeColor(kind, ServeStorngHit);//Trail
        NPCPlayer.transform.rotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        NPM.StartAnimation(4, 0);
        Rpc_service_AddForce(serve_Force, SoundKind);

        if (NPCMode && hitted)
        {
            NPM.NPCDestination(NPCMovingPhase.Backing, kind, this.transform.position, serve_Force, true);
        }

    }

    public int res_p1Score = 0, res_p2Score = 0;
    private bool serveBounded;
    //レシーバー側がスコア変更を検知した場合
    private void Receiver_functions()
    {
        if (rec_scoreChanged == true && id != server_id)
        {
            playermove.HP = playerAbility.hitPoint; //HPの初期化
            serveBounded = false;
            collisionTime = 0;
            floar_collision = 0;
            Play_End = false;
            if (id == 1)
            {
                if ((res_p1Score + res_p2Score) % 2 == 0)
                {
                    mainPlayer.transform.position = new Vector3(20, 1, -90);
                    print("偶数だよ");

                }
                else
                {
                    mainPlayer.transform.position = new Vector3(-20, 1, -90);
                    print("奇数だよ");
                }
            }
            else if (id == 2)
            {
                if ((res_p1Score + res_p2Score) % 2 == 0)
                {
                    mainPlayer.transform.position = new Vector3(-20, 1, 90);
                    print("偶数だよ2121");

                }
                else
                {
                    mainPlayer.transform.position = new Vector3(20, 1, 90);
                    print("奇数だよ2121");
                }
            }


            rec_scoreChanged = false;


        }
    }

    private void HittingPlayer_change()
    {

        Vector3 velo = rBody.linearVelocity;

        if (velo.z > 1 && vel_turned == true)
        {
            vel_turned = false;

            hitting_player = 2;
            floar_collision = 0;

            if (id == 1)
            {
                ball_hit = false;
                timing_dis = 0;
                charge_time = 0;

            }
        }
        if (velo.z < -1 && vel_turned == false)
        {
            vel_turned = true;
            hitting_player = 1;
            floar_collision = 0;

            if (id == 2)
            {
                timing_dis = 0;
                charge_time = 0;
                ball_hit = false;
            }
        }
        if (receiver_FirstHit == true || server_id == id) return;
        if (velo.z > 1) //一度も打ってないレシーバーの対応
        {
            vel_turned = false;

            hitting_player = 2;
            floar_collision = 0;

            if (id == 1)
            {
                ball_hit = false;
                timing_dis = 0;
                charge_time = 0;

            }
            receiver_FirstHit = true;
        }
        if (velo.z < -1)
        {
            vel_turned = true;
            hitting_player = 1;
            floar_collision = 0;

            if (id == 2)
            {
                timing_dis = 0;
                charge_time = 0;
                ball_hit = false;
            }
            receiver_FirstHit = true;
        }
    }

    int AnimationKind;
    public bool drivePressed, slicePressed, lobPressed, dropPressed;

    void ball_kinds()
    {
        if (ball_hit == false)
        {
            if (id == server_id && service_hit == false) return;

            if (id == 1)
            {
                if (this.transform.position.x > mainPlayer.transform.position.x) AnimationKind = 1;
                else AnimationKind = -1;
            }
            else if (id == 2)
            {
                if (this.transform.position.x > mainPlayer.transform.position.x) AnimationKind = -1;
                else AnimationKind = 1;
            }


            if (device == "PC" && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(0)) || device != "PC" && (drivePressed))
            {
                float timing_dis_pointX;
                float timing_dis_poitSqrt;
                if (id == 1)
                {
                    if (this.transform.position.z >= -5) return;
                }
                else if (id == 2)
                {
                    if (this.transform.position.z <= 5) return;

                }
                if (mainPlayer.transform.position.x < this.transform.position.x)
                {
                    timing_dis_pointX = this.transform.position.x - 5 - mainPlayer.transform.position.x; //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                else
                {
                    timing_dis_pointX = Mathf.Abs(this.transform.position.x + 5 - mainPlayer.transform.position.x); //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                timing_dis = mainPlayer.transform.position.z - this.transform.position.z;
                float timing_dis_x = this.transform.position.x - mainPlayer.transform.position.x; //0中心
                float timing_dis_sqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_x, 2));  //0中心
                Ball_hitting_func(AnimationKind, timing_dis_sqrt, 1);

            }
            if (device == "PC" && (Input.GetKeyDown(KeyCode.LeftControl)) || device != "PC" && (lobPressed))
            {
                float timing_dis_pointX = 0;
                float timing_dis_poitSqrt = 0;

                if (id == 1)
                {
                    if (this.transform.position.z >= -5) return;

                }
                else if (id == 2)
                {
                    if (this.transform.position.z <= 5) return;

                }


                if (mainPlayer.transform.position.x < this.transform.position.x)
                {
                    timing_dis_pointX = this.transform.position.x - 5 - mainPlayer.transform.position.x; //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                else
                {
                    timing_dis_pointX = Mathf.Abs(this.transform.position.x + 5 - mainPlayer.transform.position.x); //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                timing_dis = mainPlayer.transform.position.z - this.transform.position.z;
                float timing_dis_x = this.transform.position.x - mainPlayer.transform.position.x; //0中心
                float timing_dis_sqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_x, 2));  //0中心
                Ball_hitting_func(AnimationKind, timing_dis_sqrt, 2);

            }
            if (device == "PC" && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)) || device != "PC" && (slicePressed))
            {
                float timing_dis_pointX = 0;
                float timing_dis_poitSqrt = 0;
                if (id == 1)
                {
                    if (this.transform.position.z >= -5) return;

                }
                else if (id == 2)
                {
                    if (this.transform.position.z <= 5) return;

                }


                if (mainPlayer.transform.position.x < this.transform.position.x)
                {
                    timing_dis_pointX = this.transform.position.x - 5 - mainPlayer.transform.position.x; //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                else
                {
                    timing_dis_pointX = Mathf.Abs(this.transform.position.x + 5 - mainPlayer.transform.position.x); //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                timing_dis = mainPlayer.transform.position.z - this.transform.position.z;
                float timing_dis_x = this.transform.position.x - mainPlayer.transform.position.x; //0中心
                float timing_dis_sqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_x, 2));  //0中心
                Ball_hitting_func(AnimationKind, timing_dis_sqrt, 3);

            }
            if (device == "PC" && (Input.GetKeyDown(KeyCode.Z)) || device != "PC" && (dropPressed))
            {
                float timing_dis_pointX = 0;
                float timing_dis_poitSqrt = 0;
                if (id == 1)
                {
                    if (this.transform.position.z >= -5) return;

                }
                else if (id == 2)
                {
                    if (this.transform.position.z <= 5) return;

                }


                if (mainPlayer.transform.position.x < this.transform.position.x)
                {
                    timing_dis_pointX = this.transform.position.x - 5 - mainPlayer.transform.position.x; //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                else
                {
                    timing_dis_pointX = Mathf.Abs(this.transform.position.x + 5 - mainPlayer.transform.position.x); //5中心
                    timing_dis_poitSqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_pointX, 2)); //5中心
                }
                timing_dis = mainPlayer.transform.position.z - this.transform.position.z;
                float timing_dis_x = this.transform.position.x - mainPlayer.transform.position.x; //0中心
                float timing_dis_sqrt = Mathf.Sqrt(Mathf.Pow(timing_dis, 2) + Mathf.Pow(timing_dis_x, 2));  //0中心
                Ball_hitting_func(AnimationKind, timing_dis_sqrt, 4);

            }
        }

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_toss_addForce(Vector3 Force, Vector3 position)
    {
        Physics.gravity = new Vector3(0, -50, 0);
        rBody.linearVelocity = Vector3.zero;
        this.transform.position = position;
        rBody.AddForce(Force, ForceMode.Impulse);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_service_AddForce(Vector3 serve_Force, int SoundKind)
    {
        rBody.linearVelocity = Vector3.zero;
        rBody.AddForce(serve_Force, ForceMode.Impulse);
        if (SoundKind == 1) audioSource.PlayOneShot(hit1); //打球音
        else if (SoundKind == 2) audioSource.PlayOneShot(hit2);  //強打球音
    }

    private Vector3 Caculate_Force(float timing_dis, float ball_time)
    {
        float angle;
        if (Camera.main.transform.localEulerAngles.y > 180)
        {
            angle = (Camera.main.transform.localEulerAngles.y - 360) * Mathf.PI / 180;

        }
        else
        {
            angle = Camera.main.transform.localEulerAngles.y * Mathf.PI / 180;

        }
        Vector3 i_targetPosition = new Vector3(transform.position.x + Mathf.Sin(angle) * (timing_dis - transform.position.z), 0, timing_dis);
        Vector2 startPos = new Vector2(this.transform.position.x, this.transform.position.z);
        Vector2 targetPos = new Vector2(i_targetPosition.x, i_targetPosition.z);

        float distance = Vector3.Distance(i_targetPosition, this.transform.position);

        float x = distance;
        float g = 50;
        float y0 = this.transform.position.y;
        float y = i_targetPosition.y;
        float t = ball_time;

        float v_x = x / t;
        float v_y = (y - y0) / t + (g * t) / 2;
        Vector3 vec = new Vector3(0, v_y, v_x);

        float rad = Mathf.Atan2(v_x, v_y);

        float angle_x = rad * Mathf.Rad2Deg;
        float angle_y = angle * Mathf.Rad2Deg;

        var angleAxis = Quaternion.AngleAxis(angle_y, Vector3.up);
        Vector3 velocity = angleAxis * vec;

        return velocity;

    }

    private Vector3 NPC_Caculate_Force(float timing_dis, float ball_time, int ballkind)
    {
        float angle;
        float desX = 0;
        float playerPosition_x = mainPlayer.transform.position.x;
        float ballPosX = this.transform.position.x;
        float targetPosX = 0;
        if (playerPosition_x <= 0)
        {
            targetPosX = UnityEngine.Random.Range(10, 35);
        }
        else
        {
            targetPosX = UnityEngine.Random.Range(-10, -35);
        }
        desX = targetPosX - ballPosX;

        float desZ = timing_dis - this.transform.position.z;

        angle = Mathf.Atan2(desZ, desX);
        if (ballkind == 3)
        {
            angle += 0.2f;
        }

        if (angle >= -Mathf.PI / 2 && angle <= 0)
        {
            angle = -(angle - Mathf.PI / 2);
        }
        else if (angle >= -Mathf.PI)
        {
            angle = -angle - (3 * Mathf.PI / 2);
        }

        Vector3 i_targetPosition = new Vector3(targetPosX, 0, timing_dis);
        Vector2 startPos = new Vector2(this.transform.position.x, this.transform.position.z);
        Vector2 targetPos = new Vector2(i_targetPosition.x, i_targetPosition.z);
        var newObj = Instantiate(obj, i_targetPosition, Quaternion.identity);
        Destroy(newObj, 2f);
        float distance = Vector3.Distance(i_targetPosition, this.transform.position);

        float x = distance;
        float g = 50;
        float y0 = this.transform.position.y;
        float y = i_targetPosition.y;
        float t = ball_time;

        float v_x = x / t;
        float v_y = (y - y0) / t + (g * t) / 2;
        Vector3 vec = new Vector3(0, v_y, v_x);

        float rad = Mathf.Atan2(v_x, v_y);

        float angle_x = rad * Mathf.Rad2Deg;
        float angle_y = angle * Mathf.Rad2Deg;

        var angleAxis = Quaternion.AngleAxis(angle_y, Vector3.up);
        Vector3 velocity = angleAxis * vec;

        return velocity;

    }
    private Vector3 NPC_Serve_Caculate_Force(float timing_dis, float ball_time, int ballkind)
    {
        float angle;
        float desX = 0;
        float playerPosition_x = mainPlayer.transform.position.x;
        float ballPosX = this.transform.position.x;
        float targetPosX = 0;
        if ((scoremanager.p1Score + scoremanager.p2Score) % 2 == 0)
        {
            targetPosX = 15;
        }
        else
        {
            targetPosX = -15;
        }

        desX = targetPosX - ballPosX;

        float desZ = timing_dis - this.transform.position.z;

        angle = Mathf.Atan2(desZ, desX);
        if (ballkind == 3)
        {
            angle += 0.2f;
        }

        if (angle >= -Mathf.PI / 2 && angle <= 0)
        {
            angle = -(angle - Mathf.PI / 2);
        }
        else if (angle >= -Mathf.PI)
        {
            angle = -angle - (3 * Mathf.PI / 2);
        }

        Vector3 i_targetPosition = new Vector3(targetPosX, 0, timing_dis);
        Vector2 startPos = new Vector2(this.transform.position.x, this.transform.position.z);
        Vector2 targetPos = new Vector2(i_targetPosition.x, i_targetPosition.z);
        var newObj = Instantiate(obj, i_targetPosition, Quaternion.identity);
        Destroy(newObj, 2f);
        float distance = Vector3.Distance(i_targetPosition, this.transform.position);

        float x = distance;
        float g = 50;
        float y0 = this.transform.position.y;
        float y = i_targetPosition.y;
        float t = ball_time;

        float v_x = x / t;
        float v_y = (y - y0) / t + (g * t) / 2;
        Vector3 vec = new Vector3(0, v_y, v_x);

        float rad = Mathf.Atan2(v_x, v_y);

        float angle_x = rad * Mathf.Rad2Deg;
        float angle_y = angle * Mathf.Rad2Deg;

        var angleAxis = Quaternion.AngleAxis(angle_y, Vector3.up);
        Vector3 velocity = angleAxis * vec;

        return velocity;

    }

    private void Ball_out()
    {
        if ((server_id == id || NPCMode) && Play_End == false)
        {
            if (this.transform.position.z >= 150 || this.transform.position.z <= -150)
            {

                int ListNum = PointInfoList.Count;
                for (int i = 0; i < ListNum; i++) //ダブりを防ぐ
                {
                    if (PlNetChange.HitNumNetworked == PointInfoList[i].hittedNum)
                    {
                        return;
                    }
                }
                int pointingPlayer = 0;
                if (hitting_player == 1)
                {
                    pointingPlayer = 2;
                }
                else if (hitting_player == 2)
                {
                    pointingPlayer = 1;
                }
                PointInfo newPointInfo = MakePointInfo(pointingPlayer, PlNetChange.HitNumNetworked, "Nothing", this.transform.position, 1);
                PointInfoList.Add(newPointInfo);
            }

        }


        if (server_id != id && !NPCMode && CurrentPhaseInt == 4 && Play_End == false)
        {
            if (this.transform.position.z >= 150 || this.transform.position.z <= -150)
            {
                PlNetChange.Rpc_MissNumChange();
                Play_End = true;
            }

        }
    }
    //NPCのため全ての動作がServerIDと反転する
    public bool TryNPCHit(int AnimeKind, float timing_dis_sqrt, int ball_kind)
    {
        Vector3 position_now = this.transform.position;
        if (timing_dis_sqrt > playerAbility.range * 3) return false; //ダブり回避と遠すぎるボールの反応を無くす
        //if (id == server_id && serveBounded == false) return false; //レシーバーかつサーブがバウンドしてないときは反応しない
        if (CurrentPhaseInt != 3 && CurrentPhaseInt != 4) return false; //現在のフェーズ(列挙体)が4(Raley)の時意外は反応しない(サーバー,レシーバー問わない)
        bool smash = false;
        bool bad = false;
        bool StrongHit = false;
        float DisFromNet = transform.position.z;
        float sliceStrenth = 0;
        float drivePower = 1.4f;
        float slicePower = 1.5f;
        BallKinds ballkindsEnum = BallKinds.Nothing;
        DisFromNet = Mathf.Abs(DisFromNet);

        float TimeDic = 0.5f - 0.005f * DisFromNet; //ボレー

        float NSpeed = 1;
        float DisY = this.transform.position.y;

        if (DisY >= 20) return false; //高すぎるボールは見逃し

        if (TimeDic <= 0 || Mathf.Abs(this.transform.position.z) >= 50)
        {
            TimeDic = 0;//負の数にならないため
        }

        if (DisY >= 14.5 && DisY <= 20)
        {
            if (ball_kind != 2)
            {
                //スマッシュ
                TimeDic += 0.6f;
                smash = true;
                AnimeKind = 0;
            }
            if (timing_dis_sqrt >= NPCPlayerAbility.range * 2) return false; //スマッシュボールはグッドまでしか反応しない
        }

        if (hitting_player != id)
        {
            if (ball_kind == 1 || ball_kind == 3)//ドライブとスライス
            {
                if (timing_dis_sqrt <= NPCPlayerAbility.range && 0 < timing_dis_sqrt) //perfect
                {
                    timing_time = drivePower - TimeDic;
                    timing_long = 80;
                    StrongHit = true;
                    var newPerticle = Instantiate(perticle_perfect, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 2;
                    NSpeed = NPCPlayerAbility.drivePower;
                    if (ball_kind == 3)
                    {
                        sliceStrenth = NPCPlayerAbility.sliceStrokeStrength + 10;
                        timing_long = 75;
                        timing_time = slicePower - TimeDic;
                        NSpeed = NPCPlayerAbility.slicePower;
                    }
                }
                else if (timing_dis_sqrt <= NPCPlayerAbility.range * 2) //good
                {
                    timing_time = drivePower + 0.2f - TimeDic;
                    timing_long = 60;
                    var newPerticle = Instantiate(perticle_good, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 1;
                    NSpeed = NPCPlayerAbility.drivePower;
                    if (ball_kind == 3)
                    {
                        sliceStrenth = NPCPlayerAbility.sliceStrokeStrength;
                        timing_long = 55;
                        timing_time = slicePower + 0.1f - TimeDic;
                        NSpeed = NPCPlayerAbility.slicePower;
                    }
                }
                else if (timing_dis_sqrt <= NPCPlayerAbility.range * 3) //ボールの種類関係なくこの変数
                {
                    if (smash == true) return false;
                    ball_kind = 5;
                    timing_time = 1.8f;
                    timing_long = 50;
                    SoundKind = 1;
                    bad = true;
                    NSpeed = 0.8f;
                }
            }
            else if (ball_kind == 2) //ロブ
            {
                if (timing_dis_sqrt <= NPCPlayerAbility.range && 0 < timing_dis_sqrt)
                {
                    timing_time = 2.5f;
                    timing_long = 85;
                    StrongHit = true;
                    var newPerticle = Instantiate(perticle_perfect, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 2;
                    NSpeed = 1.5f;
                }
                else if (timing_dis_sqrt <= NPCPlayerAbility.range * 2)
                {
                    timing_time = 2f;
                    timing_long = 80;
                    var newPerticle = Instantiate(perticle_good, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 1;
                    NSpeed = 1;
                }
                else if (timing_dis_sqrt <= NPCPlayerAbility.range * 3) //ボールの種類関係なくこの変数
                {
                    if (smash == true) return false;
                    ball_kind = 5;
                    timing_time = 2f;
                    timing_long = 60;
                    SoundKind = 1;
                    bad = true;
                    NSpeed = 0.8f;
                }
            }
            if (ball_kind == 4)//ドロップ
            {
                float dropTime = 1 + (DisFromNet / 180); //ドロップボールの計算
                if (timing_dis_sqrt <= NPCPlayerAbility.range && 0 < timing_dis_sqrt)
                {
                    timing_time = dropTime;
                    timing_long = 30;
                    StrongHit = true;
                    var newPerticle = Instantiate(perticle_perfect, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 2;
                    NSpeed = 1.2f;
                }
                else if (timing_dis_sqrt <= NPCPlayerAbility.range * 2)
                {
                    timing_time = dropTime + 0.2f;
                    timing_long = 35;
                    var newPerticle = Instantiate(perticle_good, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 1;
                    NSpeed = 1;

                }
                else if (timing_dis_sqrt <= NPCPlayerAbility.range * 3) //ボールの種類関係なくこの変数
                {
                    if (smash == true) return false;
                    ball_kind = 5;
                    timing_time = 1.8f;
                    timing_long = 50;
                    SoundKind = 1;
                    bad = true;
                    NSpeed = 0.8f;
                }
            }
            if (id != 2)
            {
                timing_long *= -1;//反転
                sliceStrenth *= -1;

            }

            if (smash == true && bad == false) SoundKind = 3;
            if (bad)
            {
                playermove.HP -= 100;
                ballkindsEnum = (BallKinds)5;
            }
            else
            {
                ballkindsEnum = ((BallKinds)ball_kind);
            }
            this.GetComponent<PlayerNetworkedChange>().Rpc_ChangeCurrentBallKinds(ballkindsEnum);
            //this.GetComponent<PlayerNetworkedChange>().CurrentBall = ballkindsEnum;
            //弾を打つ
            Quaternion rotate = Camera.main.transform.rotation;
            Vector3 Force = NPC_Caculate_Force(timing_long, timing_time, ball_kind);
            rotate.x = 0;
            rotate.z = 0;
            floar_collision = 0;
            Vector3 Lerp_pos = Vector3.zero;
            Lerp_pos = new Vector3(position_now.x + AnimeKind * 8, 1, position_now.z);

            if (HasStateAuthority) PlNetChange.HitNumNetworked++;
            else
            {
                this.GetComponent<PlayerNetworkedChange>().Rpc_HitNumIncrement();
            }
            NPM.Player_Move_to_Ball(Lerp_pos, rotate, true);  //プレイヤーを球へ移動
            BTM.Rpc_ChangeColor(ball_kind, StrongHit); //Trail変更
            NPM.StartAnimation(AnimeKind, ball_kind); //アニメーション開始
            NPCPlayer.transform.LookAt(new Vector3(Force.x, NPCPlayer.transform.position.y, Force.z));
            NetworkedSpeed = NSpeed;

            if (service_hit == true) Rpc_Ball_add_vel(Force, position_now, SoundKind, ball_kind, sliceStrenth);
            return true;
        }
        return false;
    }


    private void Ball_hitting_func(int AnimeKind, float timing_dis_sqrt, int ball_kind)
    {//if (girl_Network.HasStateAuthority != true) return;
        Vector3 position_now = this.transform.position;

        if (server_id == id && service_hit == false) return; //サーバーがサーブを打っていなかったら反応しない
        if (server_id == id && tossed == false) return;      //↑のトスver
        if (ball_hit == true || timing_dis_sqrt >= 15) return; //ダブり回避と遠すぎるボールの反応を無くす
        if (id != server_id && serveBounded == false) return; //レシーバーかつサーブがバウンドしてないときは反応しない
        if (CurrentPhaseInt != 3 && CurrentPhaseInt != 4) return; //現在のフェーズ(列挙体)が4(Raley)の時意外は反応しない(サーバー,レシーバー問わない)
        bool smash = false;
        bool bad = false;
        bool StrongHit = false;
        float DisFromNet = transform.position.z;
        float sliceStrenth = 0;
        float drivePower = 1.4f;
        float slicePower = 1.5f;
        BallKinds ballkindsEnum = BallKinds.Nothing;
        DisFromNet = Mathf.Abs(DisFromNet);

        float TimeDic = 0.5f - 0.005f * DisFromNet; //ボレー

        float DisY = this.transform.position.y;
        float NSpeed = 1;
        if (DisY >= 20) return; //高すぎるボールは見逃し

        if (TimeDic <= 0 || Mathf.Abs(this.transform.position.z) >= 50)
        {
            TimeDic = 0;//負の数にならないため
        }

        if (DisY >= 14.5 && DisY <= 20)
        {
            if (ball_kind != 2)
            {
                //スマッシュ
                TimeDic += 0.6f;
                smash = true;
                AnimeKind = 0;
            }
            if (timing_dis_sqrt >= playerAbility.range * 2) return; //スマッシュボールはグッドまでしか反応しない
        }

        if (hitting_player == id)
        {

            if (ball_kind == 1 || ball_kind == 3)//ドライブとスライス
            {
                NSpeed = 1;
                if (timing_dis_sqrt <= playerAbility.range && 0 < timing_dis_sqrt)
                {
                    timing_time = drivePower - TimeDic;
                    timing_long = 80;
                    StrongHit = true;
                    var newPerticle = Instantiate(perticle_perfect, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 2;
                    NSpeed = playerAbility.drivePower;
                    if (ball_kind == 3)
                    {
                        sliceStrenth = playerAbility.sliceStrokeStrength + 10;
                        timing_long = 75;
                        timing_time = slicePower - TimeDic;
                        NSpeed = playerAbility.slicePower;
                    }
                }
                else if (timing_dis_sqrt <= playerAbility.range * 2)
                {
                    timing_time = drivePower + 0.2f - TimeDic;
                    timing_long = 60;
                    var newPerticle = Instantiate(perticle_good, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 1;
                    NSpeed = playerAbility.drivePower;
                    if (ball_kind == 3)
                    {
                        sliceStrenth = playerAbility.slicePower;
                        timing_long = 55;
                        timing_time = slicePower + 0.1f - TimeDic;
                        NSpeed = playerAbility.slicePower;
                    }
                }
                else if (timing_dis_sqrt <= playerAbility.range * 3) //ボールの種類関係なくこの変数
                {
                    if (smash == true) return;
                    ball_kind = 5;
                    timing_time = 1.8f;
                    timing_long = 50;
                    SoundKind = 1;
                    bad = true;
                    NSpeed = 0.8f;

                }
            }
            else if (ball_kind == 2) //ロブ
            {
                NSpeed = 2;
                if (timing_dis_sqrt <= playerAbility.range && 0 < timing_dis_sqrt)
                {
                    timing_time = 2.5f;
                    timing_long = 85;
                    StrongHit = true;
                    var newPerticle = Instantiate(perticle_perfect, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 2;
                    NSpeed = 1.5f;

                }
                else if (timing_dis_sqrt <= playerAbility.range * 2)
                {
                    timing_time = 2f;
                    timing_long = 80;
                    var newPerticle = Instantiate(perticle_good, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 1;
                    NSpeed = 1;
                }
                else if (timing_dis_sqrt <= playerAbility.range * 3) //ボールの種類関係なくこの変数
                {
                    if (smash == true) return;
                    ball_kind = 5;
                    timing_time = 2f;
                    timing_long = 60;
                    SoundKind = 1;
                    bad = true;
                    NSpeed = 0.8f;
                }
            }
            if (ball_kind == 4)//ドロップ
            {
                float dropTime = 1 + (DisFromNet / 180); //ドロップボールの計算
                if (timing_dis_sqrt <= playerAbility.range && 0 < timing_dis_sqrt)
                {
                    timing_time = dropTime;
                    timing_long = 30;
                    StrongHit = true;
                    var newPerticle = Instantiate(perticle_perfect, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 2;
                    NSpeed = 1.2f;
                }
                else if (timing_dis_sqrt <= playerAbility.range * 2)
                {
                    timing_time = dropTime + 0.2f;
                    timing_long = 35;
                    var newPerticle = Instantiate(perticle_good, this.transform.position, this.transform.rotation);
                    Destroy(newPerticle, 5f);
                    SoundKind = 1;
                    NSpeed = 1f;
                }
                else if (timing_dis_sqrt <= playerAbility.range * 3) //ボールの種類関係なくこの変数
                {
                    if (smash == true) return;
                    ball_kind = 5;
                    timing_time = 1.8f;
                    timing_long = 50;
                    SoundKind = 1;
                    bad = true;
                    NSpeed = 0.8f;
                }
            }
            if (id == 2)
            {
                timing_long *= -1;//反転
                sliceStrenth *= -1;

            }

            if (smash == true && bad == false) SoundKind = 3;
            if (bad)
            {
                playermove.HP -= 100;
                ballkindsEnum = (BallKinds)5;
            }
            else
            {
                ballkindsEnum = ((BallKinds)ball_kind);
            }
            this.GetComponent<PlayerNetworkedChange>().Rpc_ChangeCurrentBallKinds(ballkindsEnum);
            //this.GetComponent<PlayerNetworkedChange>().CurrentBall = ballkindsEnum;
            //弾を打つ
            Quaternion rotate = Camera.main.transform.rotation;
            Vector3 Force = Caculate_Force(timing_long, timing_time);
            rotate.x = 0;
            rotate.z = 0;
            ball_hit = true;
            timing_dis = 0;
            charge_time = 0;
            floar_collision = 0;
            Vector3 Lerp_pos = Vector3.zero;
            float cameraLerp = 0;
            if (id == 1)
            {
                Lerp_pos = new Vector3(position_now.x + AnimeKind * (-8), 1, position_now.z);
                cameraLerp = AnimeKind * (-8);
            }
            else if (id == 2)
            {
                Lerp_pos = new Vector3(position_now.x + AnimeKind * 8, 1, position_now.z);
                cameraLerp = AnimeKind * (-8);

            }
            if (StrongHit == true && device == "iOS")
            {
                UnityCoreHapticsProxy.PlayContinuousHaptics(1, 0.6f, 0.4f);

            }
            if (HasStateAuthority) PlNetChange.HitNumNetworked++;
            else
            {
                this.GetComponent<PlayerNetworkedChange>().Rpc_HitNumIncrement();
            }

            mainPlayer.gameObject.transform.rotation = rotate;
            playermove.Player_Move_to_Ball(Lerp_pos, rotate, true);  //プレイヤーを球へ移動
            BTM.Rpc_ChangeColor(ball_kind, StrongHit); //Trail変更
            cameramove.CameraShake(StrongHit);
            playermove.StartAnimation(AnimeKind, ball_kind); //アニメーション開始
            if (service_hit == true) Rpc_Ball_add_vel(Force, position_now, SoundKind, ball_kind, sliceStrenth);

            if (NPCMode)
            {
                NPCPlayer.GetComponent<NPCPlayerMove>().NPCDestination(NPCMovingPhase.Chasing, ball_kind, this.transform.position, Force, false);
            }
            if (HasStateAuthority)
            {
                NetworkedSpeed = NSpeed;
            }
            else
            {
                RPC_SpeedChange(NSpeed);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 position_now = this.transform.position;
        floar_collision = 0;
        if (server_id == id && service_hit == false) return;
        if (server_id == id && tossed == false) return;//トスver
        //自分の壁に当たったらボールを打った判定にする
        if (id == 1 && other.gameObject.name == "wall")
        {
            ball_hit = true;
            timing_dis = 0;
            charge_time = 0;
        }
        else if (id == 2 && other.gameObject.name == "wall1")
        {
            ball_hit = true;
            timing_dis = 0;
            charge_time = 0;
        }
        //相手のところの壁のみ情報を送る
        if (id == 2 && other.gameObject.name == "wall")
        {
            Rpc_Ball_add_vel(transform.forward * 120 + transform.up * 32, position_now, 0, 0, 0);
            ball_hit = false;
            timing_dis = 0;
            charge_time = 0;
            floar_collision = 0;
        }
        else if (id == 1 && other.gameObject.name == "wall1")
        {

            Rpc_Ball_add_vel(transform.forward * -120 + transform.up * 32, position_now, 0, 0, 0);
            ball_hit = false;
            timing_dis = 0;
            charge_time = 0;
            floar_collision = 0;
        }
    }

    private bool NotServerPlayEnd;
    private void OnCollisionEnter(Collision collision)
    {
        float collisionT = 1;
        collisionTime = 0;
        if (Time.time <= 1f) return; //出現してすぐは反応しない
        if (collision.gameObject.name == "floar")
        {
            floar_collision++;
            if (audioSource != null && boundSound != null) audioSource.PlayOneShot(boundSound);
            if (server_id != id) serveBounded = true;
            if (NPCMode && !Play_End) NPM.BattlePhaseChange(BattlePhase.Raley);
            if (device == "iOS") UnityCoreHapticsProxy.PlayContinuousHaptics(1, 0.6f, 0.01f);

        }
        float Judge_pos = this.transform.position.x;
        if (collisionT >= 0.5f && collision.gameObject.name == "floar" && (server_id == id || NPCMode))
        {
            int incrementNum = 0;
            if (hitting_player == id)
            {
                incrementNum = FindObjectOfType<playerManager>().PlayerNum;
            }
            else
            {
                incrementNum = 1;
            }

            bool fault = false;
            if (ServiceJugde == false && tossing == true) //トスを上げて,打たずに地面についた時
            {
                this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.RaleyEnd; //networkPropertyの列挙体
                playermove.StartAnimation(-10, 0);
                fault = true;
                FaultNum++;
                floar_collision = 0;
                Play_End = true;
                service_hit = false;
                tossed = false;
                beforeToss = false;
                tossing = false;
                timeManager = 0;
                vel_turned = false;
                if (FaultNum == 1) Rpc_InsPic(2, this.transform.position);
                else if (FaultNum == 2) Rpc_InsPic(4, this.transform.position);

                if (FaultNum < 2) return;

                if (FaultNum == 2)
                {
                    FaultNum = 0;
                    if (server_id == 1)
                    {
                        scoremanager.p2Score++;
                    }
                    else if (server_id == 2)
                    {
                        scoremanager.p1Score++;
                    }
                }

            }

            if (ServiceJugde && Play_End == false)
            {
                if ((scoremanager.p1Score + scoremanager.p2Score) % 2 == 0) //クロス側
                {
                    if (server_id == 1)
                    {
                        if (this.transform.position.x >= -40 && this.transform.position.x <= 2)
                        {
                            this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.Serve; //networkPropertyの列挙体
                            fault = false;
                        }
                        else
                        {
                            fault = true;

                        }

                    }
                    else if (server_id == 2)
                    {
                        if (this.transform.position.x >= -2 && this.transform.position.x <= 40)
                        {
                            fault = false;

                        }
                        else
                        {
                            fault = true;
                        }
                    }
                }
                else if ((scoremanager.p1Score + scoremanager.p2Score) % 2 == 1)  //逆クロス側
                {
                    if (server_id == 1)
                    {
                        if (this.transform.position.x >= -2 && this.transform.position.x <= 40)
                        {
                            fault = false;
                        }
                        else
                        {
                            fault = true;
                        }
                    }
                    else if (server_id == 2)
                    {
                        if (this.transform.position.x >= -40 && this.transform.position.x <= 2)
                        {
                            fault = false;
                        }
                        else
                        {
                            fault = true;
                        }
                    }
                }
                if (fault == false)
                {
                    this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.Raley; //networkPropertyの列挙体
                    FaultNum = 0;
                    ServiceJugde = false;
                }
                else if (fault == true)
                {
                    this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.RaleyEnd; //networkPropertyの列挙体
                    FaultNum++;
                    floar_collision = 0;
                    Play_End = true;
                    service_hit = false;
                    tossed = false;
                    beforeToss = false;
                    timeManager = 0;
                    vel_turned = false;
                    if (FaultNum == 1) Rpc_InsPic(2, this.transform.position);
                    else if (FaultNum == 2) Rpc_InsPic(4, this.transform.position);

                    if (FaultNum < 2) return;
                }
                if (FaultNum == 2)
                {
                    FaultNum = 0;
                    if (server_id == 1)
                    {
                        scoremanager.p2Score++;
                    }
                    else if (server_id == 2)
                    {
                        scoremanager.p1Score++;
                    }
                }
            }

            if (Play_End) return;
            if (floar_collision >= 2 && Play_End == false) //先に2バウンドの判定から,のちにアウトか否か
            {

                int pointingPlayer = 0;
                if (hitting_player == 1)
                {
                    pointingPlayer = 2;
                }
                else if (hitting_player == 2)
                {
                    pointingPlayer = 1;
                }
                print("2bounds");
                PointInfo newPointInfo = MakePointInfo(pointingPlayer, PlNetChange.HitNumNetworked, "2bounds", this.transform.position, incrementNum);
                PointInfoList.Add(newPointInfo);
                //PointInfoList.Add(new )
                //if (hitting_player == 1)
                //{
                //    scoremanager.p2Score++;
                //}
                //else if (hitting_player == 2)
                //{
                //    scoremanager.p1Score++;
                //}
                //this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.RaleyEnd; //networkPropertyの列挙体
                //Rpc_InsPic(3, this.transform.position);
                //floar_collision = 0;
                //Play_End = true;
                //service_hit = false;
                //tossed = false;
                //beforeToss = false;
                //timeManager = 0;
                //vel_turned = false;

                return;
            }

            if (Judge_pos >= -41 && Judge_pos <= 41)
            {

            }
            else if (floar_collision == 1)
            {
                print("out");
                PointInfo newPointInfo = MakePointInfo(hitting_player, PlNetChange.HitNumNetworked, "out", this.transform.position, incrementNum);
                PointInfoList.Add(newPointInfo);
                //if(Play_End == false)
                //{
                //    if (hitting_player == 1)
                //    {
                //        scoremanager.p1Score++;
                //        Rpc_InsPic(1, this.transform.position);

                //    }
                //    else if (hitting_player == 2)
                //    {
                //        scoremanager.p2Score++;
                //        Rpc_InsPic(1, this.transform.position);
                //    }
                //    this.GetComponent<PlayerNetworkedChange>().CurrentPhase = PlayPhase.RaleyEnd; //networkPropertyの列挙体
                //    floar_collision = 0;
                //    Play_End = true;
                //    service_hit = false;
                //    beforeToss = false;
                //    timeManager = 0;
                //    tossed = false;
                //    vel_turned = false;
                //    print("out");
                //}

            }
        }




        if ((id != server_id || !NPCMode) && CurrentPhaseInt == 4 && hitting_player == id)
        {
            if (floar_collision >= 2) //先に2バウンドの判定から,のちにアウトか否か
            {
                PlNetChange.Rpc_MissNumChange();
                print("2bound  " + PlNetChange.HitNumNetworked);
                return;
            }

            if (Judge_pos >= -41 && Judge_pos <= 41)
            {

                print("in");
            }
            else if (floar_collision == 1)
            {
                PlNetChange.Rpc_MissNumChange();
                print("out");
            }


        }
    }



    /// <summary>
    ///1得点したID,2ボールを打った回数(ネットワークで共通しているもの),3ミスの種類,4着地地点,5受信した回数
    /// </summary>
    /// <param name="_PointedId"></param>
    /// <param name="_hittedNum"></param>
    /// <param name="_missKind"></param>
    /// <param name="_bouncePos"></param>
    /// <param name="_recievedNum"></param>
    /// <returns>ポインタインフォ</returns>
    private PointInfo MakePointInfo(int _PointedId, int _hittedNum, string _missKind, Vector3 _bouncePos, int _recievedNum)
    {
        PointInfo NewPointInfo;
        NewPointInfo.PointedId = _PointedId;
        NewPointInfo.hittedNum = _hittedNum;
        NewPointInfo.missKind = _missKind;
        NewPointInfo.bouncePos = _bouncePos;
        NewPointInfo.recievedNum = _recievedNum;
        return NewPointInfo;
    }

    /// <summary>
    /// ラリー終了後の TossWait フェーズに入るときに呼ぶ
    /// （物理停止や位置リセットなどの後で）
    /// </summary>
    public void EnterTossWait()
    {
        Rpc_SetGravity(false);
    }
    public void ExitTossWait()
    {
        Rpc_SetGravity(true);

    }

    /// <summary>
    /// 判定画像オブジェクトを生成
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="targetPos"></param>
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_InsPic(int kind, Vector3 targetPos)
    {
        GameObject obj;
        switch (kind)
        {
            case 1:
                obj = outObj;
                break;
            case 2:
                obj = faultObj;
                break;
            case 3:
                obj = TwoBoundsObj;
                break;
            case 4:
                obj = DoubleFaultObj;
                break;
            default:
                return;
        }
        var newObj = Instantiate(obj, targetPos + transform.up, new Quaternion(0, 0, 0, 0));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_Ball_add_vel(Vector3 Force, Vector3 position, int SoundKind, int Ballkinds, float sliceStrenth)
    {

        if (Ballkinds == 3) //スライス
        {
            Rpc_Set_Slice(sliceStrenth, true);
        }
        else //それ以外
        {
            Rpc_Set_Slice(0, false);
        }
        rBody.linearVelocity = Vector3.zero;
        this.transform.position = position;
        rBody.AddForce(Force, ForceMode.Impulse);
        if (SoundKind == 1) audioSource.PlayOneShot(hit1); //打球音
        else if (SoundKind == 2) audioSource.PlayOneShot(hit2);  //強打球音
        else if (SoundKind == 3) audioSource.PlayOneShot(hit3); //スマッシュ音
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_Set_Slice(float strenth, bool slice)
    {
        if (slice == true)
        {
            Physics.gravity = new Vector3(strenth, -50, 0);

        }
        else if (slice == false)
        {
            Physics.gravity = new Vector3(0, -50, 0);

        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_SetGravity(bool enableGravity)
    {
        if (rBody == null) return;

        rBody.useGravity = enableGravity;

        if (!enableGravity)
        {
            rBody.linearVelocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
            rBody.isKinematic = true;    // 物理演算を完全停止
        }
        else
        {
            rBody.Sleep();
            rBody.linearVelocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
            rBody.isKinematic = false;
            rBody.useGravity = true;
            rBody.WakeUp();
        }
    }

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    public void RPC_SpeedChange(float sp)
    {
        NetworkedSpeed = sp;
    }


}