using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects customInspectorObjects;
    private Collider2D _collider2D;

    private void Start()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (customInspectorObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance,customInspectorObjects.panTime,customInspectorObjects.panDirection,false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 exitDirection = (other.transform.position - _collider2D.bounds.center).normalized;
            if (customInspectorObjects.swapCameras && customInspectorObjects.cameraOnLeft !=null && customInspectorObjects.cameraOnRight !=null)
            {
                CameraManager.instance.SwapCameras(customInspectorObjects.cameraOnLeft,customInspectorObjects.cameraOnRight, exitDirection);
            }
            if (customInspectorObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance,customInspectorObjects.panTime,customInspectorObjects.panDirection,true);
            }
        }
    }
}
[System.Serializable]
public class CustomInspectorObjects
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;
    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right,
}
#if UNITY_EDITOR 
[CustomEditor(typeof(CameraControlTrigger))]
public class MytScriptEditor : Editor
{
    private CameraControlTrigger _cameraControlTrigger;

    private void OnEnable()
    {
        _cameraControlTrigger = (CameraControlTrigger)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (_cameraControlTrigger.customInspectorObjects.swapCameras)
        {
            _cameraControlTrigger.customInspectorObjects.cameraOnLeft=EditorGUILayout.ObjectField("Camera on left",_cameraControlTrigger.customInspectorObjects.cameraOnLeft,
                typeof(CinemachineVirtualCamera),true) as CinemachineVirtualCamera;
            _cameraControlTrigger.customInspectorObjects.cameraOnRight=EditorGUILayout.ObjectField("Camera on right",_cameraControlTrigger.customInspectorObjects.cameraOnRight,
                typeof(CinemachineVirtualCamera),true) as CinemachineVirtualCamera;
        }

        if (_cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        {
            _cameraControlTrigger.customInspectorObjects.panDirection=(PanDirection)EditorGUILayout.EnumPopup("Camera Pan direction", _cameraControlTrigger
                .customInspectorObjects.panDirection);
            _cameraControlTrigger.customInspectorObjects.panDistance = EditorGUILayout.FloatField("Pan distance",
                _cameraControlTrigger.customInspectorObjects.panDistance);
            _cameraControlTrigger.customInspectorObjects.panTime = EditorGUILayout.FloatField("Pan Time",
                _cameraControlTrigger.customInspectorObjects.panTime);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_cameraControlTrigger);
        }
    }
}
#endif

