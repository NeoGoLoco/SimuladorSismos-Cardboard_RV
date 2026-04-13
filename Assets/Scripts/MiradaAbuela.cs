using UnityEngine;

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
        if (jugador == null)
            jugador = GameObject.FindGameObjectWithTag("Player").transform;

        rotacionInicialLocal = transform.localRotation;
        raizNPC = transform.root;
    }

    void LateUpdate()
    {
        if (jugador == null) return;

        Vector3 direccionAlJugador = jugador.position - transform.position;

        // --- LA MAGIA NUEVA AQUÍ ---
        // Rotamos el vector "adelante" de la raíz para alinear el radar con el modelo 3D
        Vector3 frenteCorregido = Quaternion.Euler(0, rotacionConoY, 0) * raizNPC.forward;

        // Calculamos el ángulo usando nuestro nuevo frente corregido
        float angulo = Vector3.Angle(frenteCorregido, direccionAlJugador);

        if (angulo < limiteAngulo)
        {
            // --- ESTÁS EN SU RANGO DE VISIÓN ---
            Quaternion rotacionMirar = Quaternion.LookRotation(direccionAlJugador);
            Quaternion rotacionCorregida = rotacionMirar * Quaternion.Euler(compensacionEjes);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionCorregida, Time.deltaTime * velocidadGiro);
        }
        else
        {
            // --- TE FUISTE POR DETRÁS ---
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacionInicialLocal, Time.deltaTime * velocidadGiro);
        }
    }
}