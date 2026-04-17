using UnityEngine;

// Simula el comportamiento errático de objetos con Rigidbody mediante vibraciones y sacudidas laterales reactivas al sismo.
[RequireComponent(typeof(Rigidbody))]
public class ObjetoSismico : MonoBehaviour
{
    [Header("Configuración del Sismo")]
    [Tooltip("Fuerza continua con la que el objeto vibra suavemente.")]
    public float fuerzaVibracion = 2f;

    [Tooltip("Fuerza de los jalones bruscos de izquierda a derecha (Oscilatorio).")]
    public float fuerzaImpulsoLateral = 3f;

    [Tooltip("Qué tan seguido ocurre un jalón brusco (0.01 = raro, 0.1 = muy seguido).")]
    public float probabilidadImpulsoLateral = 0.05f;

    private Rigidbody rb;
    private ControladorTerremoto controlador;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Vinculamos el objeto al estado global del terremoto para saber cuándo empezar a temblar
        controlador = Object.FindFirstObjectByType<ControladorTerremoto>();
    }

    // Usamos FixedUpdate porque todas las interacciones aquí son físicas (fuerzas sobre Rigidbodies)
    void FixedUpdate()
    {
        // El objeto solo reacciona si el sistema de terremoto está encendido en el controlador principal
        if (controlador != null && controlador.terremotoActivo)
        {
            // 1. VIBRACIÓN CONSTANTE (Ruido de fondo)
            // Generamos una fuerza aleatoria horizontal para simular el "temblor" base del suelo
            Vector3 fuerzaAleatoria = new Vector3(
                Random.Range(-1f, 1f),
                0f, // Bloqueamos el eje Y para que los objetos no leviten de forma irreal
                Random.Range(-1f, 1f)
            ) * fuerzaVibracion;

            rb.AddForce(fuerzaAleatoria, ForceMode.Force);

            // 2. IMPULSOS LATERALES (Sacudidas bruscas)
            // Simulamos ondas sísmicas más fuertes que desplazan los objetos violentamente
            if (Random.value < probabilidadImpulsoLateral)
            {
                // Decidimos el sentido del "latigazo" (Izquierda o Derecha)
                float direccionX = (Random.value > 0.5f) ? 1f : -1f;
                Vector3 empujeLateral = Vector3.right * direccionX;

                // Aplicamos un impulso instantáneo para romper la inercia del objeto
                rb.AddForce(empujeLateral * fuerzaImpulsoLateral, ForceMode.Impulse);
            }

            // 3. TORQUE ALEATORIO (Efecto de resbalón)
            // Añadimos una rotación sutil para que los objetos se caigan o giren mientras se desplazan
            Vector3 torqueAleatorio = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * (fuerzaVibracion * 0.5f);

            rb.AddTorque(torqueAleatorio, ForceMode.Force);
        }
    }
}