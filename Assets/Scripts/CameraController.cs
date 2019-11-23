using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player; // player object
    private Quaternion playerTargetRot;
    private Quaternion cameraTargetRot;
    private Transform ptf;
    private Transform ctf; // Camera Transform component

    [Header("Look Variables")]
    public float yAxisAngleDownLock = 45f;
    public float yAxisAngleUpLock = 45f;
    public float sensitivity = 2.0f;

    private void Start()
    {
        player = this.transform.parent.gameObject;
        ptf = player.GetComponent<Transform>();
        ctf = GetComponent<Transform>();
        playerTargetRot = ptf.rotation;
        cameraTargetRot = ctf.rotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (player != null)
        {
            Vector2 mouse = new Vector2(Input.GetAxisRaw("Mouse X") * sensitivity, Input.GetAxisRaw("Mouse Y") * sensitivity);
            cameraTargetRot *= Quaternion.Euler(-mouse.y, 0f, 0f);
            playerTargetRot *= Quaternion.Euler(-mouse.y, mouse.x, 0f);
            cameraTargetRot = LockCameraMovement(cameraTargetRot);
            ctf.localRotation = cameraTargetRot;
            ptf.localRotation = playerTargetRot;
            
        }
    }
    private Quaternion LockCameraMovement(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -yAxisAngleUpLock, yAxisAngleDownLock);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }
}
