using UnityEngine;

public class JudgerManager : MonoBehaviour
{
    private RectTransform _parentUI;

    [SerializeField]
    private GameObject obj;

    private GameObject newJudge;

    private GameObject canvas;

    private void Start()
    {
        canvas = GameObject.Find("Canvas");
        newJudge = Object.Instantiate(obj);
        Object.Destroy(base.gameObject, 1.5f);
        Object.Destroy(newJudge, 1.5f);
    }

    private void Update()
    {
        MakeJudge(base.transform.position);
    }

    public void MakeJudge(Vector3 pos)
    {
        _parentUI = canvas.GetComponent<RectTransform>();
        Vector3 vector = Camera.main.WorldToScreenPoint(pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentUI, vector, null, out var localPoint);
        newJudge.transform.SetParent(canvas.transform);
        newJudge.GetComponent<RectTransform>().localPosition = new Vector2(localPoint.x, localPoint.y + 40f);
    }
}
