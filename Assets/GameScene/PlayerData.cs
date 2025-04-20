using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Fusion;

public struct playerAbility
{
    public float speed;
    public float range;
    public float drivePower;
    public float slicePower;
    public float driveServePower;
    public float sliceServePower;
    public float sliceServeStrength;
    public float sliceStrokeStrength;
    public float hitPose;
    public float hitPoint;
    public int SkillKind;
    public playerAbility(float a, float b, float c, float d, float e, float f, float g, float h, float i, float j,int k)
    {
        speed = a;
        range = b;
        drivePower = c;
        slicePower = d;
        driveServePower = e;
        sliceServePower = f;
        sliceServeStrength = g;
        sliceStrokeStrength = h;
        hitPose = i;
        hitPoint = j;
        SkillKind = k;
    }
}

public class PlayerData : MonoBehaviour
{

    public playerAbility status = new playerAbility();
    public playerAbility NPCStatus = new playerAbility();

    public string _nickName = null;
    public string name1 = null, name2 = null; //NetworkDatasから代入され,GameSceneで出力される
    private void Start()
    {
        var count = FindObjectsOfType<PlayerData>().Length;
        if (count > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SetNickName(string nickName)
    {
        _nickName = nickName;
        if (_nickName == "YourName") _nickName = null;
    }

    public string GetNickName()
    {
        if (string.IsNullOrWhiteSpace(_nickName))
        {
            _nickName = GetRandomNickName();
        }

        return _nickName;
    }

    public static string GetRandomNickName()
    {
        var rngPlayerNumber = Random.Range(0, 9999);
        return $"Player {rngPlayerNumber.ToString("0000")}";
    }


    public void SetThisStruct(bool NPCMode,float a, float b, float c, float d, float e, float f, float g, float h, float i,float j,int k)
    {
        var waitingManger = FindObjectOfType<WaitingManager>();
        if (!NPCMode)
        {
            waitingManger.CharacterChosen();
            status = new playerAbility(
            a,//speed
            b,//range
            c,//drivePower
            d,//slicePower
            e,//driveServePower
            f,//sliceServePower
            g,//sliceServeStrength
            h,//sliceStrokeStrength
            i,//hitPose
            j,
            k
            );
        }
        else
        {
            NPCStatus = new playerAbility(
            a,//speed
            b,//range
            c,//drivePower
            d,//slicePower
            e,//driveServePower
            f,//sliceServePower
            g,//sliceServeStrength
            h,//sliceStrokeStrength
            i,//hitPose
            j,
            k
            );
        }
    }
}
