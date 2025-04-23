using Fusion;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using Unity.VisualScripting;

public class CameraMove : NetworkBehaviour
{
    private FloatingJoystick directionJoystick;
    private bool Shaking;
    private float digree;
    float camdig;
    Camera cam;
    private bool firstMoved, firstAwait;
    private Vector3 FirstPos;
    //static public people People;

    private Tweener _shakeTweener;

    private string device;
    private async void Start()
    {
        playerManager playermanager = FindObjectOfType<playerManager>();
        device = playermanager.Device;
        if (device != "PC") directionJoystick = GameObject.Find("DirectionJoystick").GetComponent<FloatingJoystick>();
        FirstPos = transform.position + Vector3.up * 23 - cam.transform.forward * 45;
        firstAwait = true;
    }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            cam = Camera.main;
        }
    }
    void Update()
    {
        if (!firstMoved)
        {
            if (firstAwait)
            {
                cam.transform.position = Vector3.Lerp(cam.transform.position, FirstPos, 2 * Time.deltaTime); // 目的の位置に移動
                float distance = Vector3.Distance(cam.transform.position, FirstPos);
                if (distance <= 2)
                {
                    firstMoved = true;
                }
            }

            return;
        }
        if (!HasStateAuthority) return;
        cameraMoving();
    }

    private void cameraMoving()
    {
        if (Shaking == true) return;
        Vector3 cam_pos = transform.position + Vector3.up * 23 - cam.transform.forward * 45;
        cam.transform.position = Vector3.Lerp(cam.transform.position, cam_pos, 15 * Time.deltaTime); // 目的の位置に移動

        if (device == "PC")
        {
            //cam.transform.position = cam_pos;
            float rotateY, rotateX;
            if (Input.GetKey(KeyCode.RightArrow))
            {
                rotateX = 120f * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                rotateX = -120f * Time.deltaTime;
            }
            else
            {
                rotateX = 0;
            }
            //横回転
            //rotateX = Input.GetAxis("Mouse X") * 10;
            //cam.transform.RotateAround(transform.position, Vector3.up, rotateX); //マウス
            cam.transform.RotateAround(transform.position, Vector3.up, rotateX); //矢印キー

            if (Input.GetKey(KeyCode.DownArrow))
            {
                rotateY = 120f * Time.deltaTime;

            }

            else if (Input.GetKey(KeyCode.UpArrow))
            {
                rotateY = -120f * Time.deltaTime;
            }
            else
            {
                rotateY = 0;
            }
            //rotateY = Input.GetAxis("Mouse Y") * 10;
            //縦回転
            //cam.transform.RotateAround(this.transform.position, cam.transform.right, -rotateY);
            cam.transform.RotateAround(this.transform.position, cam.transform.right, rotateY);
        }
        else
        {
            float rotateY, rotateX;
            rotateX = directionJoystick.Horizontal * 150 * Time.deltaTime;
            rotateY = directionJoystick.Vertical * 150 * Time.deltaTime;

            cam.transform.RotateAround(transform.position, Vector3.up, rotateX); //矢印キー
            cam.transform.RotateAround(this.transform.position, cam.transform.right, -rotateY);
        }




        //Vector3 camRot = cam.transform.localEulerAngles;
        //float xRotation = Mathf.Clamp(camRot.x, 10, 40);
        //cam.transform.eulerAngles = new Vector3(xRotation, camRot.y, camRot.z);

    }
    public void CameraShake(bool shake)
    {
        if (shake == false) return;
        Shaking = true;
        if (_shakeTweener != null)
        {
            _shakeTweener.Kill();
        }
        _shakeTweener = cam.transform.DOShakePosition(0.5f, 3, 15, 180, false);
        Invoke("AfterShaked", 0.5f);
    }

    private void AfterShaked()
    {
        Shaking = false;
    }

    private void MouseCursor()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            if (Cursor.visible == false)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

            }
        }
    }
}
