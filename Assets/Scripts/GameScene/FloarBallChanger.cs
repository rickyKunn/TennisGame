using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloarBallChanger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //private void OnCollisionExit(Collision collision)
    //{
    //    var Ball = collision.gameObject;
    //    if (Ball.name == "Ball")
    //    {
    //        BallMove ballmove = Ball.GetComponent<BallMove>();
    //        int ballKind = ballmove.CurrentBallKind;
    //        GameObject child = Ball.transform.GetChild(1).gameObject;
    //        if (child == null) return;
    //        var rbody = Ball.GetComponent<Rigidbody>();
    //        Vector3 vel = Ball.GetComponent<Rigidbody>().velocity;
    //        print(child.name);
    //        switch (child.name)
    //        {
    //            case "ParticleRed(Clone)": //ドライブ
    //                rbody.velocity = Vector3.zero;
    //                rbody.AddForce(new Vector3(vel.x,vel.y * 1.5f,vel.z) ,ForceMode.Impulse);
    //                break;
    //            case "ParticleOrange(Clone)": //ロブ
    //                rbody.velocity = Vector3.zero;
    //                rbody.AddForce(new Vector3(vel.x, vel.y * 1.5f, vel.z), ForceMode.Impulse);
    //                break;
    //            case "ParticleBlue(Clone)"://スライス
    //                rbody.velocity = Vector3.zero;
    //                rbody.AddForce(new Vector3(vel.x* 1.4f, vel.y * 1.5f, vel.z), ForceMode.Impulse);
    //                break;
    //            case "ParticleWhite(Clone)": //ドロップ
    //                rbody.velocity = Vector3.zero;
    //                rbody.AddForce(new Vector3(vel.x, vel.y, vel.z * 0.7f), ForceMode.Impulse);

    //                break;


    //        }
    //    }
    //}
}
