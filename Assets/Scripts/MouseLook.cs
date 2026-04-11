using UnityEngine;
using UnityEngine.XR;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 700f;
    public Transform playerBody;

    float xRotation = 0f;
    private InputDevice rightHandDevice;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!rightHandDevice.isValid) rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (rightHandDevice.isValid)
        {
            Vector2 vrLook;
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out vrLook))
            {
                mouseX += vrLook.x * 2.0f;
            }
        }

        float rotX = mouseX * mouseSensitivity * Time.deltaTime;
        float rotY = mouseY * mouseSensitivity * Time.deltaTime;

        xRotation -= rotY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * rotX);
    }
}