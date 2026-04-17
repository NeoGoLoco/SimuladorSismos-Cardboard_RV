using UnityEngine;
using UnityEngine.XR;

// Gestiona la rotación híbrida (Mouse y VR), controlando el cabeceo vertical y la orientación del cuerpo.
public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 700f;
    public Transform playerBody;

    float xRotation = 0f;
    private InputDevice rightHandDevice;

    void Start()
    {
        // Bloqueamos el cursor al centro para evitar que se salga de la ventana de juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Buscamos el control de VR (Mano derecha) si no ha sido detectado aún
        if (!rightHandDevice.isValid) rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // Captura de movimiento base del mouse
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Si hay un control VR conectado, sumamos el movimiento del joystick a la rotación
        if (rightHandDevice.isValid)
        {
            Vector2 vrLook;
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out vrLook))
            {
                mouseX += vrLook.x * 2.0f;
            }
        }

        // Escalamos el movimiento con la sensibilidad y el tiempo entre frames
        float rotX = mouseX * mouseSensitivity * Time.deltaTime;
        float rotY = mouseY * mouseSensitivity * Time.deltaTime;

        // Calculamos la rotación vertical (X) y la limitamos a 90 grados para no "dar la vuelta"
        xRotation -= rotY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplicamos la rotación: vertical para la cámara y horizontal para el cuerpo entero
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * rotX);
    }
}