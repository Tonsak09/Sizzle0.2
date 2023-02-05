using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamManager : MonoBehaviour
{
    [SerializeField] GameObject commonCam;
    [SerializeField] Transform pointOfInterest;

    /*[Header("FOV")]
    [SerializeField] float maxFOV;
    [SerializeField] float minFOV;
    [SerializeField] float scrollFOVSpeed;*/

    // Transitioning from current FOV to the desired one 
    [SerializeField] float minSmoothFOVSpeed;
    [SerializeField] float maxSmoothFOVSpeed;
    [Tooltip("The MAX difference the common cam's FOV can be from desired FOV to achieve max speed from curve")]
    [SerializeField] float maxDifferenceFOV;
    [SerializeField] AnimationCurve smoothFOVCurve;

    private float FOV;

    private GameObject current;
    private CinemachineFreeLook commonFreeLook;

    public GameObject Current { get { return current; } }
    public GameObject CommonCam { get { return commonCam; } }


    private void Awake()
    {
        if(commonCam == null)
        {
            commonCam = GameObject.FindGameObjectWithTag("CommonCam");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        current = commonCam;
        commonFreeLook = commonCam.GetComponent<CinemachineFreeLook>();
        FOV = commonCam.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView;
    }

    private void Update()
    {
        //ChangeFOV();
        //SmoothFOVChange();
    }

    /// <summary>
    /// Replaces the current camera with the ones cam 
    /// </summary>
    /// <param name="newCam"></param>
    public void ChangeCam(GameObject newCam)
    {
        current.SetActive(false);
        current = newCam;
        current.SetActive(true);
    }

    public void ReturnToCommon()
    {
        ChangeCam(commonCam);
    }

    public void ChangeCommonCamLookAt(Transform target)
    {
        commonCam.GetComponent<CinemachineFreeLook>().LookAt = target;
    }

    /*private void ChangeFOV()
    {
        if(current == commonCam)
        {
            float scroll = Input.mouseScrollDelta.y;

            if (scroll != 0)
            {
                FOV = Mathf.Clamp(FOV + scroll * -scrollFOVSpeed * Time.deltaTime, minFOV, maxFOV);
                    
                commonCam.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView = FOV;
            }
        }
    }*/

    private void SmoothFOVChange()
    {
        float difference = FOV - commonFreeLook.m_Lens.FieldOfView;
        print(difference);

        if(difference >= 0)
        {
            commonFreeLook.m_Lens.FieldOfView += Mathf.Lerp(minSmoothFOVSpeed, maxSmoothFOVSpeed, smoothFOVCurve.Evaluate(difference / maxDifferenceFOV));
        }
        else
        {
            commonFreeLook.m_Lens.FieldOfView -= Mathf.Lerp(minSmoothFOVSpeed, maxSmoothFOVSpeed, smoothFOVCurve.Evaluate(difference / maxDifferenceFOV));
        }

    }

}
