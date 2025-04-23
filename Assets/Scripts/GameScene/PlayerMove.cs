using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] private Vector3 localGravity;
    [SerializeField] private GameObject HitArea;

    private GameObject ball;
    private playerManager playermanager;
    private Rigidbody rBody;
    private bool lerped;
    private Vector3 lerp_vec;
    private float speed;
    public float HP;
    private Vector3 digrees;
    Camera cam;
    [SerializeField]
    private Animator playerAnimator;
    private bool IsAnimation;
    private int AnimationKind;
    float speed_an;
    bool isjump;
    bool onfloar;
    Vector3 movingDirection;
    public Vector3 movingVelocity;
    int touched;
    GameObject service_object;
    private GameObject playerdata;
    ServiceManager servicemanager;
    //ServeManager servemanager;
    public int id;
    private Quaternion hittingRotate;
    public NetworkMecanimAnimator networkAnimator;
    private FloatingJoystick MovingJoyStick;
    private string device;
    private ChatManager chatmanager;
    //public Dynami variableJoystick;






    private void Awake()
    {
        rBody = this.GetComponent<Rigidbody>();
        cam = Camera.main;
    }
    private void Start()
    {
        lerped = true;
        playermanager = FindObjectOfType<playerManager>();
        PlayerData playerdataScript = FindObjectOfType<PlayerData>();
        playerdata = playerdataScript.gameObject;
        speed = playerdataScript.status.speed;
        HP = playerdataScript.status.hitPoint;
        servicemanager = GameObject.Find("service").GetComponent<ServiceManager>();
        device = playermanager.Device;
        if (device != "PC") MovingJoyStick = GameObject.Find("MoveJoystick").GetComponent<FloatingJoystick>();
        chatmanager = FindObjectOfType<ChatManager>();
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }


    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (IsAnimation)
        {
            rBody.linearVelocity = Vector3.zero;
            return;
        }
        //if (chatmanager.isChating) return;
        float x = 0, z = 0;
        if (device == "PC")
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
        }
        else if (device != "")
        {
            x = MovingJoyStick.Horizontal;
            z = MovingJoyStick.Vertical;
        }

        movingDirection = new Vector3(x, 0.0f, z);
        Quaternion q = Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up);
        movingDirection = q * movingDirection;
        movingDirection.Normalize();//斜めの距離が長くなるのを防ぐ

        if (HP <= 0)
        {
            movingVelocity = movingDirection * (speed * 0.5f);

        }
        else
        {
            movingVelocity = movingDirection * speed;
        }

        //JoyCon-------------------------------------------------
        //m_pressedButtonL = null;
        //m_pressedButtonR = null;
        //SetControllers();

        //print(m_joycons +  " " + m_joycons.Count);

        //if (m_joycons != null || m_joycons.Count >= 1) //ジョイコンが1つ以上繋がっていたら
        //{
        //    SetControllers();

        //    foreach (var button in m_buttons)
        //    {
        //        if (m_joyconL.GetButton(button))
        //        {
        //            m_pressedButtonL = button;
        //        }
        //        if (m_joyconR.GetButton(button))
        //        {
        //            m_pressedButtonR = button;
        //        }
        //    }

        //    foreach (var joycon in m_joycons)
        //    {

        //        var isLeft = joycon.isLeft;
        //        if (isLeft == true)
        //        {
        //            var stick = joycon.GetStick();
        //            movingDirection = new Vector3(stick[0], 0.0f, stick[1]);
        //            Quaternion qJoy = Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up);
        //            movingDirection = qJoy * movingDirection;
        //            movingDirection.Normalize();//斜めの距離が長くなるのを防ぐ
        //            movingVelocity = movingDirection * speed;
        //        }


        //    }
        //}


        //------------------------------------------------------------------
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

    }

    void Update()
    {

        if (!HasStateAuthority) return;

        move();
        if (lerped == false)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, lerp_vec, 10 * Time.deltaTime); // 目的の位置に移動
            if (lerp_vec.x == transform.position.x && lerp_vec.z == transform.position.z) lerped = true;
            this.transform.rotation = hittingRotate;
        }

    }

    public async void Player_Move_to_Ball(Vector3 lerp_pos, Quaternion rotate, bool lerping)
    {
        lerped = false;
        lerp_vec = lerp_pos;
        hittingRotate = rotate;
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        lerped = true;
    }

    Vector3 latestPos;

    //private void FixedUpdate()
    //{
    //    Vector3 differenceDis = new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(latestPos.x, 0, latestPos.z);
    //    latestPos = transform.position;
    //    if (Mathf.Abs(differenceDis.x) > 0.001f || Mathf.Abs(differenceDis.z) > 0.001f)
    //    {
    //        if (movingDirection == new Vector3(0, 0, 0)) return;
    //        Quaternion rot = Quaternion.LookRotation(differenceDis);
    //        rot = Quaternion.Slerp(rBody.transform.rotation, rot, 0.2f);
    //        this.transform.rotation = rot;
    //    }

    //}

    private bool tossWait;
    private int servePosKind;
    public void TossWait(int posKind)
    {
        if (servicemanager.ServerId == id)
        {
            servePosKind = posKind;

            //tossWait = true;
        }
    }

    void move()
    {
        speed_an = rBody.linearVelocity.magnitude;
        playerAnimator.SetFloat("Speed", speed_an);

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

    //private void make_HitArea() //ヒットエリアの同名重複を防ぐ
    //{
    //    if (!HasStateAuthority) return;
    //    id = Runner.ActivePlayers.Count();
    //    Quaternion forward = Quaternion.Euler(0, 1, 0);
    //    var HitArea_Pre = GameObject.Find($"HitArea_game(Clone)");
    //    if (id == 1)
    //    {
    //        var hitArea = Instantiate(HitArea, this.transform.position + Vector3.up * 2, forward);
    //        hitArea.name = $"HitArea{id}";
    //    }
    //    if (id == 2)
    //    {
    //        if (id == 2)
    //        {
    //            var hitArea = Instantiate(HitArea, this.transform.position + Vector3.up * 2, forward);
    //            hitArea.name = $"HitArea{id}";
    //        }
    //    }
    //    //else
    //    //{
    //    //    if (id == 2)
    //    //    {
    //    //        var hitArea = Instantiate(HitArea, this.transform.position + Vector3.up * 2, forward);
    //    //    }
    //    //}
    //    //hitArea.name = $"HitArea{id}";
    //    //if (HitArea_Pre != true)
    //    //{
    //    //    var hitArea = Instantiate(HitArea, this.transform.position + Vector3.up * 2, forward);
    //    //    hitArea.name = $"HitArea{id}";

    //    //}
    //    //hitArea.gameObject.transform.parent = this.gameObject.transform;

    //}

    /// <summary>
    /// Kind = 1フォア,-1バック,2トス待機,3トス4,サーブ,0スマッシュ,-10サーブ中断
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
