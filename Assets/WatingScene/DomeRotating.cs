using UnityEngine;
using UnityEngine.SceneManagement;

public class DomeRotating : MonoBehaviour
{
    private string scene;

    private float rotateSpeed;

    private void Start()
    {
        scene = SceneManager.GetActiveScene().name;
        rotateSpeed = 10f;
    }

    private void Update()
    {
        base.transform.Rotate(new Vector3(0f, rotateSpeed, 0f) * Time.deltaTime);
        if (scene == "GameScene")
        {
            rotateSpeed -= Time.deltaTime * 3f;
            if (rotateSpeed <= 0f)
            {
                Object.Destroy(this);
            }
        }
    }
}
