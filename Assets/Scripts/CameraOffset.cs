using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

public class CameraOffset : MonoBehaviour
{
    public CinemachineVirtualCamera cmCam;
    public GameObject targetObject;
    public float smoothTime;

    private Vector3 targetOffset;
    private CinemachineTransposer transposer;

    void Start()
    {
        transposer = cmCam.GetCinemachineComponent<CinemachineTransposer>();
    }

    void Update()
    {
        FindOffset();
        transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, targetOffset, smoothTime * Time.deltaTime);
    }

    private void FindOffset()
    {
        var x = (targetObject.transform.localScale.magnitude - 1) * 4;
        targetOffset = new Vector3(0, 30 + x, -30 - x);
    }
}
