using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TryCamera : MonoBehaviour
{
    [SerializeField] private float _frecuency;
    [SerializeField] private float _time;
    [SerializeField] private float _intensity;
    public void ShakeCamera(){
          CinemachineCameraEffects.Instance.CameraMovement(_intensity, _frecuency, _time);
    }
}
