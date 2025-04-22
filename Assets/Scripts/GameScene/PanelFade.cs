using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelFade : MonoBehaviour
{
    private bool StartProcess;

    private Image image;

    [SerializeField]
    private float timeSpan;

    private float alpha = 0.8f;

    private void Start()
    {
        image = GetComponent<Image>();
        image.color = new Color(0.33f, 0.33f, 0.33f, 0.8f);
        StartCoroutine("StartFade");
    }

    private void Update()
    {
        if (StartProcess)
        {
            if (alpha >= 0f)
            {
                alpha -= Time.deltaTime / timeSpan;
            }
            else
            {
                Object.Destroy(base.gameObject);
            }
            image.color = new Color(0.33f, 0.33f, 0.33f, alpha);
        }
    }

    private IEnumerator StartFade()
    {
        yield return new WaitForSeconds(0.4f);
        StartProcess = true;
    }
}