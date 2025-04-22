using UnityEngine;

public class PictureManager : MonoBehaviour
{
    private Vector3 targetPos;

    [SerializeField]
    private AudioClip Fault;

    [SerializeField]
    private AudioClip DoubleFault;

    [SerializeField]
    private AudioClip Out;

    [SerializeField]
    private AudioClip TwoBounds;

    private AudioSource audioSource;

    private ServiceManager servicemanager;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        servicemanager = GameObject.Find("service").GetComponent<ServiceManager>();
        string text = base.name;
        if (text == "Out(Clone)")
        {
            audioSource.PlayOneShot(Out);
        }
        if (text == "DoubleFault(Clone)")
        {
            audioSource.PlayOneShot(DoubleFault);
        }
        if (text == "Fault(Clone)")
        {
            audioSource.PlayOneShot(Fault);
        }
        if (text == "2bound(Clone)")
        {
            audioSource.PlayOneShot(TwoBounds);
        }
    }

    private void Update()
    {
    }
}
