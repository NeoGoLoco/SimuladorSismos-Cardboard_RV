using UnityEngine;
using UnityEngine.InputSystem;

//Este script para el movimiento del Jugador y sus acciones, lejos de lo que hace con la mano... Y los objetos del Inventario oculto.
public class ControladorTerremoto : MonoBehaviour
{
    [Header("Movimiento Base")]
    public float velocidadCaminar = 5f;
    public float velocidadAgachado = 2f;
    public float fuerzaSalto = 8f;
    public float gravedad = 20f;
    public bool puedeCaminar = true;

    [Header("Configuración Agachado")]
    public float alturaNormal = 2f;
    public float alturaAgachado = 1f;
    public float radioNormal = 0.5f;
    public float radioAgachado = 0.35f;

    [Header("Efecto Caminar (Head Bobbing)")]
    public float velocidadBobbing = 2f;
    public float cantidadBobbing = 0.1f;
    private float timerBobbing = 0f;

    [Header("Efecto Terremoto (Cámara)")]
    public float intensidadTemblor = 0.3f;
    public float velocidadTemblor = 10f;
    public bool terremotoActivo = true;

    [Header("Inmersión FFT (Audio Reactivo)")]
    public AudioSource audioTerremoto; // Aquí se arrastra el sonido del temblor
    public float multiplicadorAudio = 5f; // Multiplica la violencia cuando hay un sonido fuerte
    private float[] datosEspectro = new float[256];
    private float intensidadBajos = 0f;

    // Sacudimos el pivote, no la cámara... Por problemas de distancia y parentesco en el Cardboard 
    [Header("VR Rig")]
    public Transform cabezaPivot;
    public Camera camaraJugador;

    private CharacterController controller;
    private Vector3 direccionMovimiento = Vector3.zero;
    private float alturaPivotOriginal;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Guarda la altura original del EarthquakePivot, no de la cámara
        if (cabezaPivot != null)
            alturaPivotOriginal = cabezaPivot.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (puedeCaminar && controller.enabled)
        {
            MoverJugador();
            GestionarAgachado();
        }

        // LECTURA DEL AUDIO FFT EN TIEMPO REAL para el sonido del temblor
        if (terremotoActivo && audioTerremoto != null && audioTerremoto.isPlaying)
        {
            audioTerremoto.GetSpectrumData(datosEspectro, 0, FFTWindow.BlackmanHarris);
            intensidadBajos = 0f;
            // Solo lee los bajos más profundos (los primeros 5 valores)
            for (int i = 0; i < 5; i++)
            {
                intensidadBajos += datosEspectro[i];
            }
        }
        else
        {
            intensidadBajos = 0f;
        }

        ControlarEfectosCamara();
    }

    void MoverJugador()
    {
        if (controller.isGrounded)
        {
            float moveX = 0f;
            float moveZ = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) moveZ += 1f;
                if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
                if (Keyboard.current.dKey.isPressed) moveX += 1f;
                if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            }

            if (Gamepad.current != null)
            {
                moveX += Gamepad.current.leftStick.x.ReadValue();
                moveZ += Gamepad.current.leftStick.y.ReadValue();
            }

            moveX = Mathf.Clamp(moveX, -1f, 1f);
            moveZ = Mathf.Clamp(moveZ, -1f, 1f);

            bool jumpInput = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpInput = true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) jumpInput = true;

            bool isCrouching = false;
            if (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed) isCrouching = true;
            if (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed) isCrouching = true;

            float velocidadActual = isCrouching ? velocidadAgachado : velocidadCaminar;

            // Caminar hacia donde mira la cámara
            Vector3 forwardCamara = camaraJugador.transform.forward;
            forwardCamara.y = 0f;
            forwardCamara.Normalize();

            Vector3 rightCamara = camaraJugador.transform.right;
            rightCamara.y = 0f;
            rightCamara.Normalize();

            Vector3 move = rightCamara * moveX + forwardCamara * moveZ;

            direccionMovimiento.x = move.x * velocidadActual;
            direccionMovimiento.z = move.z * velocidadActual;

            if (jumpInput)
            {
                direccionMovimiento.y = fuerzaSalto;
            }
        }

        direccionMovimiento.y -= gravedad * Time.deltaTime;
        controller.Move(direccionMovimiento * Time.deltaTime);
    }

    void GestionarAgachado()
    {
        bool agacharseInput = false;

        if (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed) agacharseInput = true;
        if (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed) agacharseInput = true;

        float alturaObjetivo = agacharseInput ? alturaAgachado : alturaNormal;
        float radioObjetivo = agacharseInput ? radioAgachado : radioNormal;

        controller.height = Mathf.Lerp(controller.height, alturaObjetivo, 10 * Time.deltaTime);
        controller.radius = Mathf.Lerp(controller.radius, radioObjetivo, 10 * Time.deltaTime);
    }

    void ControlarEfectosCamara()
    {
        // Movemos el cabezaPivot (El objeto llamado EarthquakePivot)
        if (cabezaPivot == null) return;

        Vector3 posFinal = cabezaPivot.localPosition;
        float offsetYTotal = 0f;

        if (terremotoActivo)
        {
            // El truco para el movimiento oscilante: Sumamos el temblor base + la fuerza reactiva del audio
            float fuerzaTotal = intensidadTemblor + (intensidadBajos * multiplicadorAudio);

            posFinal.x = (Mathf.PerlinNoise(Time.time * velocidadTemblor, 0) - 0.5f) * fuerzaTotal;
            offsetYTotal += (Mathf.PerlinNoise(0, Time.time * velocidadTemblor) - 0.5f) * fuerzaTotal;
        }
        else
        {
            posFinal.x = 0;
        }

        Vector3 velocidadPlana = Vector3.zero;
        bool tocaPiso = false;

        if (controller.enabled)
        {
            velocidadPlana = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            tocaPiso = controller.isGrounded;
        }

        if (velocidadPlana.magnitude > 0.1f && tocaPiso && puedeCaminar)
        {
            timerBobbing += Time.deltaTime * (velocidadPlana.magnitude * velocidadBobbing);
            offsetYTotal += Mathf.Sin(timerBobbing) * cantidadBobbing;
        }
        else
        {
            timerBobbing = 0f;
        }

        bool isCrouching = false;
        if (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed) isCrouching = true;
        if (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed) isCrouching = true;

        if (isCrouching && puedeCaminar) offsetYTotal -= 0.5f;

        posFinal.y = alturaPivotOriginal + offsetYTotal;
        cabezaPivot.localPosition = Vector3.Lerp(cabezaPivot.localPosition, posFinal, 10 * Time.deltaTime);
    }
}