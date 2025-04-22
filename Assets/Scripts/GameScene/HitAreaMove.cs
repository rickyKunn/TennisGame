using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class HitAreaMove : MonoBehaviour
{
    //[SerializeField]
    private GameObject girl;
    //[SerializeField]
    public GameObject ball;

    private BoxCollider boxcol;
    private float time;
    public bool girl_come;
    // Start is called before the first frame update
    NetworkObject girl_Network;
    Rigidbody rBody;
    Vector3 pos;
    int id;
    PlayerMove playermove;
    string HitAreaName;
    private void Awake()
    {
        boxcol = this.gameObject.GetComponent<BoxCollider>();
        girl = GameObject.Find("player");
        girl_Network = girl.GetComponent<NetworkObject>();
        playermove = girl.GetComponent<PlayerMove>();
        id = playermove.id;
        
        if (girl_Network.HasStateAuthority) HitAreaName = $"HitArea{id}";
    }


    // Update is called once per frame
    void Update()
    {
        //if (id == 1) pos = girl.transform.position + Vector3.forward * 5f;
        //else if (id == 2) pos = girl.transform.position + Vector3.forward * -5f;
        pos = girl.transform.position;
        if (ball.transform.position.x > this.transform.position.x)
        {
        }
        else
        {
        }
        this.transform.position = pos + Vector3.up * 8;
        timing();
    }

    void timing()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            boxcol.enabled = false;
        }
        else
        {
            if (HitAreaName == this.name)
            {
                boxcol.enabled = true;
            }

        }
    }
    private void OnCollisionEnter(Collision collision)
    {


    }
}

