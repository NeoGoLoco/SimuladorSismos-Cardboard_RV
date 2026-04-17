using UnityEngine;

// Dirige la mirada de un hueso específico hacia el jugador, corrigiendo la orientación del rig y limitando el rango de visión.
public class MiradaSinAnimacion : MonoBehaviour
{
    public Transform jugador;

    [Header("1. Corrección Visual (Hueso roto)")]
    [Tooltip("Arregla que te mire con la oreja")]
    public Vector3 compensacionEjes = new Vector3(0, 0, 0);

    [Header("2. Corrección del Cono de Detección")]
    [Tooltip("Mueve este valor en positivo o negativo para centrar el área de detección")]
    public float rotacionConoY = 0f;

    [Header("3. Límites (Anti-Exorcista)")]
    public float limiteAngulo = 70f;
    public float velocidadGiro = 3f;

    private Quaternion rotacionInicialLocal;
    private Transform raizNPC;

    void Start()
    {
        // Si no asignamos al jugador, lo buscamos automáticamente por su Tag
        if (jugador == null)
            jugador = GameObject.FindGameObjectWithTag("Player").transform;

        // Guardamos la postura original para que la cabeza regrese a su sitio cuando el jugador se aleja
        rotacionInicialLocal = transform.localRotation;
        raizNPC = transform.root;
    }

    // Usamos LateUpdate para que la rotación manual ocurra DESPUÉS de las animaciones del Animator
    void LateUpdate()
    {
        if (jugador == null) return;

        Vector3 direccionAlJugador = jugador.position - transform.position;

        // AJUSTE DEL RADAR
        // Corregimos el vector "frente" del NPC para que el cono de visión coincida con la cara del modelo
        Vector3 frenteCorregido = Quaternion.Euler(0, rotacionConoY, 0) * raizNPC.forward;

        // Calculamos la diferencia angular entre el frente del NPC y la posición del jugador
        float angulo = Vector3.Angle(frenteCorregido, direccionAlJugador);

        if (angulo < limiteAngulo)
        {
            // ESTADO: SIGUIENDO CON LA MIRADA
            // Calculamos la rotación hacia el jugador y aplicamos la compensación para corregir ejes mal orientados
            Quaternion rotacionMirar = Quaternion.LookRotation(direccionAlJugador);
            Quaternion rotacionCorregida = rotacionMirar * Quaternion.Euler(compensacionEjes);
            
            // Suavizamos el movimiento para que la cabeza no gire instantáneamente
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionCorregida, Time.deltaTime * velocidadGiro);
        }
        else
        {
            // ESTADO: REGRESO A POSICIÓN NATURAL
            // Si el jugador sale del cono de visión, la cabeza vuelve a su rotación de Idle
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacionInicialLocal, Time.deltaTime * velocidadGiro);
        }
    }
}