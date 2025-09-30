using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Net.NetworkInformation;

public class CinemachineCameraEffects : MonoBehaviour
{
    public static CinemachineCameraEffects Instance;
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private CinemachineBasicMultiChannelPerlin _cinemachineBasicMultiChannelPerlin;
    [SerializeField] private float _movementTime;
    [SerializeField] private float _totalMovementTime;
    [SerializeField] private float _initialIntensity;
    //References variables from components
    private void Awake() {
        _cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        _cinemachineBasicMultiChannelPerlin = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        Instance = this;
    }
    //This method gradually reduces the intensity to 0 according to the chosen time, returns the camera to normal.
    private void Update() {
        if (_movementTime > 0){
            _movementTime -= Time.deltaTime;
            _cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                Mathf.Lerp(_initialIntensity, 0, 1 - (_movementTime/_totalMovementTime));
        }
        
    }

    //this method sets the values of the Noise profile (6D Shake), intensity and frequency to add the effect to the camera.
    public void CameraMovement(float intesity, float frequency, float time){
        _cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intesity;
        _cinemachineBasicMultiChannelPerlin.m_FrequencyGain = frequency;
        _initialIntensity = intesity;
        _totalMovementTime = time;
        _movementTime = time;
    }

}
