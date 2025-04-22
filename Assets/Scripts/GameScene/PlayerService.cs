using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;

public class PlayerService : NetworkBehaviour
{

    [SerializeField]
    private GameObject ball;
    // Start is called before the first frame update
    public bool ball_exist;
    public bool girl1_exist;
    public bool girl2_exist;
    public int id;
    private int server_id;
    private GameObject NPC;
    ServiceManager servicemanager;
    void Start()
    {
        ball_exist = false;
        servicemanager = GameObject.Find("service").GetComponent<ServiceManager>();
        server_id = servicemanager.ServerId;
    }

    // Update is called once per frame
    void Update()
    {
        if (HasStateAuthority)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(0))
            {
                ball_exist = GameObject.Find("Ball") == true ? true : false; //押されるたびにボールの有無を更新(ボールは2秒後に消されるため)
                server_id = servicemanager.ServerId; //ボタンが押されるたび更新
                id = FindObjectOfType<playerManager>().id;
                if (server_id == id && ball_exist == false)
                {
                    var newBall = Runner.Spawn(ball, this.transform.position + transform.up * 5, new Quaternion(0, 0, 0, 0));
                    ball_exist = true;
                    Rpc_Ball_Exist(newBall);

                }
            }
        }

    }

    public bool NPCMakeBall()
    {
        NPC = GameObject.Find("NPCPlayer");
        ball_exist = GameObject.Find("Ball") == true ? true : false; //押されるたびにボールの有無を更新(ボールは2秒後に消されるため)
        server_id = servicemanager.ServerId; //ボタンが押されるたび更新
        id = FindObjectOfType<playerManager>().id;
        if (server_id == 2 && ball_exist == false)
        {
            var newBall = Runner.Spawn(ball, NPC.transform.position + NPC.transform.up * 5, new Quaternion(0, 0, 0, 0));
            ball_exist = true;
            Rpc_Ball_Exist(newBall);

        }
        return ball_exist;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_Ball_Exist(NetworkObject newBall)
    {
        //Instantiate(ball, this.transform.position + transform.up * 2, new Quaternion(0, 0, 0, 0));
        newBall.transform.position = this.transform.position;
    }
}