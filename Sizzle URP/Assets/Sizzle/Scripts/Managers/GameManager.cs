using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static Camera CurrentCam;
    public static Transform CameraTarget;

    [SerializeField] float sensitivityMin;
    [SerializeField] float sensitivityMax;
    [Tooltip("X sensitivity to Y (X:Y)")] [SerializeField] float sensitivityRatio;
    [SerializeField] gameStates state;
    [Range(0, 1)] [SerializeField] float camSensitivity;



    private CinemachineFreeLook cam;
    private SoundManager sm;
    private enum gameStates { play, UI };
    private float volume;



    /// <summary>
    /// Value is set from 0 to 1
    /// </summary>
    public float CamSensitivity { get { return camSensitivity; } set { camSensitivity = value; } }
    public float Volume { get { return volume; } set { volume = value; } }

    private void Awake()
    {
        cam = GameObject.FindObjectOfType<CinemachineFreeLook>();
        sm = GameObject.FindObjectOfType<SoundManager>();
    }

    private void Start()
    {
        //UpdateValues();
        volume = 1;
    }

    /// <summary>
    /// Updates all the objects that are associated with 
    /// the game manager to have the appropriate values 
    /// </summary>
    public void UpdateValues()
    {
        SetCamValues();
        SetVolumeMultiplier();
    }

    public void SetCamValues()
    {
        float difference = sensitivityMax - sensitivityMin;
        float disFromMin = difference * camSensitivity;
        float speed = sensitivityMin + disFromMin;

        cam.m_XAxis.m_MaxSpeed = speed;
        cam.m_YAxis.m_MaxSpeed = speed * sensitivityRatio;
    }

    public void SetVolumeMultiplier()
    {
        print("Volume is now " + volume);
        sm.SoundMultiplier = volume;
    }

    public static void SwapCamera(CinemachineVirtualCamera nextCam)
    {
        CurrentCam.gameObject.SetActive(false);

        nextCam.gameObject.SetActive(true);
        nextCam.LookAt = CameraTarget;
    }
}
