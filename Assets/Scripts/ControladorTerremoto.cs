using UnityEngine;
using UnityEngine.InputSystem;

//Este script para el movimiento del Jugador y sus acciones, lejos de lo que hace con la mano... Y los objetos del Inventario oculto.
// Está optimizado para VR (Cardboard) al sacudir un pivote intermedio en lugar de la cámara.
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
    // Simula el balanceo natural de la cabeza al caminar para dar realismo
    public float velocidadBobbing = 2f;
    public float cantidadBobbing = 0.1f;
    private float timerBobbing = 0f;

    [Header("Efecto Terremoto (Cámara)")]
    public float intensidadTemblor = 0.3f;
    public float velocidadTemblor = 10f;
    public bool terremotoActivo = true;

    [Header("Inmersión FFT (Audio Reactivo)")]
    // Vinculamos la sacudida visual a la potencia del audio del sismo
    public AudioSource audioTerremoto; 
    public float multiplicadorAudio = 5f; 
    private float[] datosEspectro = new float[256];
    private float intensidadBajos = 0f;

    [Header("VR Rig")]
    // NOTA: Sacudimos este pivote y no la cámara directamente para evitar
    // conflictos con el tracking de rotación del Cardboard/VR.
    public Transform cabezaPivot;
    public Camera camaraJugador;

    private CharacterController controller;
    private Vector3 direccionMovimiento = Vector3.zero;
    private float alturaPivotOriginal;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Registramos la altura base para que los efectos de cámara siempre regresen al centro
        if (cabezaPivot != null)
            alturaPivotOriginal = cabezaPivot.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Solo procesamos movimiento si la simulación lo permite (ej. no estamos en un diálogo)
        if (puedeCaminar && controller.enabled)
        {
            MoverJugador();
            GestionarAgachado();
        }

        // --- ANÁLISIS SENSORIAL ---
        // Extraemos los datos de frecuencia del audio para que el temblor sea reactivo.
        // Si el sonido tiene un "pico" de bajos, la cámara vibrará con más fuerza.
        if (terremotoActivo && audioTerremoto != null && audioTerremoto.isPlaying)
        {
            audioTerremoto.GetSpectrumData(datosEspectro, 0, FFTWindow.BlackmanHarris);
            intensidadBajos = 0f;
            
            // Sumamos los primeros 5 bins del espectro (donde viven las frecuencias bajas/graves)
            for (int i = 0; i < 5; i++)
            {
                intensidadBajos += datosEspectro[i];
            }
        }
        else
        {
            intensidadBajos = 0f;
        }

        // Aplicamos todas las fuerzas calculadas a la visualización de la cámara
        ControlarEfectosCamara();
    }

    // Calcula la dirección y velocidad del personaje basándose en la vista de cámara
    void MoverJugador()
    {
        if (controller.isGrounded)
        {
            float moveX = 0f;
            float moveZ = 0f;

            // Lectura de inputs (Teclado)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) moveZ += 1f;
                if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
                if (Keyboard.current.dKey.isPressed) moveX += 1f;
                if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            }

            // Lectura de inputs (Gamepad)
            if (Gamepad.current != null)
            {
                moveX += Gamepad.current.leftStick.x.ReadValue();
                moveZ += Gamepad.current.leftStick.y.ReadValue();
            }

            // Normalizamos para evitar que camine más rápido en diagonal
            moveX = Mathf.Clamp(moveX, -1f, 1f);
            moveZ = Mathf.Clamp(moveZ, -1f, 1f);

            bool jumpInput = (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false) || 
                             (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);

            bool isCrouching = (Keyboard.current?.leftCtrlKey.isPressed ?? false) || 
                               (Gamepad.current?.leftStickButton.isPressed ?? false);

            float velocidadActual = isCrouching ? velocidadAgachado : velocidadCaminar;

            // Orientamos el movimiento relativo a la dirección horizontal de la cámara
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

        // Aplicamos gravedad y ejecutamos el movimiento físico
        direccionMovimiento.y -= gravedad * Time.deltaTime;
        controller.Move(direccionMovimiento * Time.deltaTime);
    }

    // Suaviza la transición de altura al agacharse usando interpolación (Lerp)
    void GestionarAgachado()
    {
        bool agacharseInput = (Keyboard.current?.leftCtrlKey.isPressed ?? false) || 
                              (Gamepad.current?.leftStickButton.isPressed ?? false);

        float alturaObjetivo = agacharseInput ? alturaAgachado : alturaNormal;
        float radioObjetivo = agacharseInput ? radioAgachado : radioNormal;

        // Modificamos el colisionador del personaje para que pueda pasar por sitios bajos
        controller.height = Mathf.Lerp(controller.height, alturaObjetivo, 10 * Time.deltaTime);
        controller.radius = Mathf.Lerp(controller.radius, radioObjetivo, 10 * Time.deltaTime);
    }

    // El núcleo estético del script: combina sismo, caminata y agachado en un solo pivote
    void ControlarEfectosCamara()
    {
        if (cabezaPivot == null) return;

        Vector3 posFinal = cabezaPivot.localPosition;
        float offsetYTotal = 0f;

        // --- EFECTO SISMO ---
        if (terremotoActivo)
        {
            // Combinamos una sacudida base (Perlin Noise) con la intensidad de los bajos del audio
            float fuerzaTotal = intensidadTemblor + (intensidadBajos * multiplicadorAudio);

            // Generamos un movimiento errático pero fluido en X e Y
            posFinal.x = (Mathf.PerlinNoise(Time.time * velocidadTemblor, 0) - 0.5f) * fuerzaTotal;
            offsetYTotal += (Mathf.PerlinNoise(0, Time.time * velocidadTemblor) - 0.5f) * fuerzaTotal;
        }
        else
        {
            posFinal.x = 0;
        }

        // --- EFECTO CAMINATA (BOBBING) ---
        Vector3 velocidadPlana = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        
        if (velocidadPlana.magnitude > 0.1f && controller.isGrounded && puedeCaminar)
        {
            // Usamos una función Seno para crear el vaivén rítmico de la cabeza
            timerBobbing += Time.deltaTime * (velocidadPlana.magnitude * velocidadBobbing);
            offsetYTotal += Mathf.Sin(timerBobbing) * cantidadBobbing;
        }
        else
        {
            timerBobbing = 0f;
        }

        // Ajuste visual extra si el jugador está agachado
        bool isCrouching = (Keyboard.current?.leftCtrlKey.isPressed ?? false) || 
                           (Gamepad.current?.leftStickButton.isPressed ?? false);
        if (isCrouching && puedeCaminar) offsetYTotal -= 0.5f;

        // Aplicamos el desplazamiento total suavizado para evitar saltos bruscos en la cámara
        posFinal.y = alturaPivotOriginal + offsetYTotal;
        cabezaPivot.localPosition = Vector3.Lerp(cabezaPivot.localPosition, posFinal, 10 * Time.deltaTime);
    }
}