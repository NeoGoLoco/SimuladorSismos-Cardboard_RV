using UnityEngine;

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

    // Referencias internas
    private Rigidbody rb;
    private ControladorTerremoto controlador;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Busca automáticamente tu controlador principal en la escena
        controlador = Object.FindFirstObjectByType<ControladorTerremoto>();
    }

    void FixedUpdate()
    {
        // Si el controlador existe y el terremoto está encendido...
        if (controlador != null && controlador.terremotoActivo)
        {
            // 1. Vibración general (Tiembla un poco en todas direcciones horizontales)
            Vector3 fuerzaAleatoria = new Vector3(
                Random.Range(-1f, 1f),
                0f, // Mantenemos el eje Y en 0 para evitar que brinque
                Random.Range(-1f, 1f)
            ) * fuerzaVibracion;

            rb.AddForce(fuerzaAleatoria, ForceMode.Force);

            // 2. Movimiento fuerte de Izquierda a Derecha (Oscilatorio)
            if (Random.value < probabilidadImpulsoLateral)
            {
                // Decidimos aleatoriamente si el jalón es hacia la izquierda o hacia la derecha
                float direccionX = (Random.value > 0.5f) ? 1f : -1f;

                // Vector3.right es el eje X (izquierda/derecha). Lo multiplicamos por 1 o -1.
                Vector3 empujeLateral = Vector3.right * direccionX;

                // Aplicamos la fuerza como un impulso repentino
                rb.AddForce(empujeLateral * fuerzaImpulsoLateral, ForceMode.Impulse);
            }

            // 3. Rotación aleatoria para que los objetos resbalen y giren
            Vector3 torqueAleatorio = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * (fuerzaVibracion * 0.5f);

            rb.AddTorque(torqueAleatorio, ForceMode.Force);
        }
    }
}