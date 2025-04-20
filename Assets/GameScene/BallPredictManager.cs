using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class BallPredictManager : MonoBehaviour
{
    private Rigidbody rBody;
    private PhysicsScene physicsScenePredict;
    private  Scene predictScene;
    private float simulateTime = 0.1f;
    public bool reached;
    private void Start()
    {
        rBody = this.GetComponent<Rigidbody>();
        predictScene = SceneManager.CreateScene("PredictScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScenePredict = predictScene.GetPhysicsScene();
        GameObject predictFloar = GameObject.Find("predictFloar");
        SceneManager.MoveGameObjectToScene(predictFloar, predictScene);
        SceneManager.MoveGameObjectToScene(this.gameObject, predictScene);

    }

    private void Update()
    {

    }

    public  void ThisAddVel(Vector3 hitPoint, Vector3 hitVel)
    {
        this.transform.position = hitPoint;
        rBody.velocity = Vector3.zero;
        rBody.AddForce(hitVel, ForceMode.Impulse);
        simulateTime = 7;

    }
    private void FixedUpdate()
    {
        physicsScenePredict.Simulate(Time.fixedDeltaTime *simulateTime);
        if (this.transform.position.z >= 160 || this.transform.position.z <= -160 || this.transform.position.x >= 90 || this.transform.position.x <= -90)
        {
            this.transform.position = Vector3.one;
            rBody.velocity = Vector3.zero;
            simulateTime = 0.1f;
        }
        //if (this.transform.position.z >= 70)
        //{
        //    rBody.velocity = Vector3.zero;
        //    simulateTime = 0.1f;
        //    reached = true;
        //    print("afsd");
        //}
    }
   
}
