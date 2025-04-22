using UnityEngine;

public class HitButtonManager : MonoBehaviour
{
    private void Start()
    {
    }

    public void ButtonEntered()
    {
        GameObject gameObject = GameObject.Find("Ball");
        if (!(gameObject == null))
        {
            BallMove component = gameObject.GetComponent<BallMove>();
            MonoBehaviour.print(base.name);
            if (base.name == "DriveButton")
            {
                component.drivePressed = true;
            }
            else if (base.name == "SliceButton")
            {
                component.slicePressed = true;
            }
            else if (base.name == "LobButton")
            {
                component.lobPressed = true;
            }
            else if (base.name == "DropButton")
            {
                component.dropPressed = true;
            }
        }
    }
}
