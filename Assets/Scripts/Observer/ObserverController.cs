using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverController : MonoBehaviour
{
    private GameObject player; // player object
    private Transform ptf;
    private Transform ctf;
    private float currentX = 0f;
    private float currentY = 0f;
    private const float Y_ANGLE_MIN = 5.0f;
    private const float Y_ANGLE_MAX = 110.0f;

    [Header("Camera Variables")]
    public float camZoomDistance = 5.0f;
    public float camZoomSpeed = 4.0f;
    public float minZoomDistance = 1.5f;
    public float maxZoomDistance = 30.0f;
    public float camScrollSpeed = 20.0f;
    public float defaultCamAngle = 35.0f;

    private float inputVertical; // Variable for player vertical input, used for tank forward/reverse movement
    private float inputHorizontal; // Variable for player horizontal input, used for tank rotation

    private void Start()
    {
        player = this.transform.parent.gameObject;
        ptf = player.GetComponent<Transform>();
        ctf = GetComponent<Transform>();

        currentY = defaultCamAngle;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    void Update()
    {
        if (player != null)
        {
            // Adjust camera angle when mouse wheel button is pressed
            if (Input.GetButton("Mouse WheelButton"))
            {
                currentX += Input.GetAxis("Mouse X");
                currentY -= Input.GetAxis("Mouse Y");
                currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);
            }

            // Mouse wheel camera zoom
            camZoomDistance -= Input.GetAxis("Mouse ScrollWheel") * camZoomSpeed;
            camZoomDistance = Mathf.Clamp(camZoomDistance, minZoomDistance, maxZoomDistance);

            // Movement inputs
            inputVertical = Input.GetAxis("Vertical");
            inputHorizontal = Input.GetAxis("Horizontal");

            // Call functions to Move the observer
            ptf.Translate(Vector3.forward * inputVertical * camScrollSpeed * Time.deltaTime, Space.Self);
            ptf.Translate(Vector3.right * inputHorizontal * camScrollSpeed * Time.deltaTime, Space.Self);

            if (Input.GetButtonDown("Admin"))
            {   
                GameManager.instance.isAdminMenu = !GameManager.instance.isAdminMenu;
            }
            if (Input.GetButtonDown("Pause"))
            {
                GameManager.instance.isPaused = !GameManager.instance.isPaused;
            }
        }
    }
    private void LateUpdate()
    {
        // Set camera position and rotation
        Vector3 dir = new Vector3(0, 0, -camZoomDistance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        ctf.position = ptf.position + rotation * dir;
        ctf.LookAt(ptf.position + transform.up); // Direct camera at player

        // Set player "y" rotation to match the camera's, required to move the camera properly in local space
        Vector3 playerRotation = new Vector3(ptf.eulerAngles.x, ctf.eulerAngles.y, ptf.eulerAngles.z);
        ptf.eulerAngles = playerRotation;
    }
}
