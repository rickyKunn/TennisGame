using Fusion;
using UnityEngine;

public class ServiceManager : NetworkBehaviour
{
    public int ServerId;

    public playerManager ManagerObject;

    private playerManager playermanager;

    public int id;

    private bool collect;

    public bool serve_changed;

    private PlayerService playerservice;

    private PlayerMove playermove;

    private void Start()
    {
        while (!playermanager)
        {
            playermanager = UnityEngine.Object.FindObjectOfType<playerManager>();
        }
        serve_changed = false;
        ServerId = 1;
    }

    public void ServiceChange()
    {
        playerservice = GameObject.Find("player").GetComponent<PlayerService>();
        playermove = GameObject.Find("player").GetComponent<PlayerMove>();
        switch (ServerId)
        {
            case 1:
                ServerId = 2;
                break;
            case 2:
                ServerId = 1;
                break;
        }
    }
}