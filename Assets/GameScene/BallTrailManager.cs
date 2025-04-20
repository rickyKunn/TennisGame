using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BallTrailManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject[] particles = new GameObject[4];

    //[SerializeField]
    private ParticleSystemRenderer ParticleRen;

    [SerializeField]
    private Material[] Materials = new Material[4];

    [SerializeField]
    private GameObject ShitParticle;

    private GameObject newShit;
    private GameObject newPer;
    void Start()
    {
        ParticleRen = ShitParticle.GetComponent<ParticleSystemRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ChangeColor(int kind ,bool Strong)
    {
        //TR = this.gameObject.GetComponent<TrailRenderer>();
        //Destroy(TR);
        //TrailRenderer trailRenderer = this.gameObject.AddComponent<TrailRenderer>();
        //trailMaterial = trailRenderer.material;
        //TR.endColor = Color.clear;
        if (newPer != null) Destroy(newPer);
        if (newShit != null)
        {
            Destroy(newShit);
            newShit = null;
        }

        if (kind == 0 || kind >= 6)
        {
            newPer = null;
            newShit = null;
        }
        else
        {
            newPer = particles[kind - 1];
            ParticleRen.material = Materials[kind - 1];
        }

        if (newPer != null)
        {
            newPer = Instantiate(newPer);
            newPer.transform.position = this.transform.position;
            newPer.transform.parent = this.transform;
        }
        if(newShit == null && Strong == true)
        {
            newShit = Instantiate(ShitParticle);
            newShit.transform.position = this.transform.position;
            newShit.transform.parent = this.gameObject.transform;
        }
    }
}
