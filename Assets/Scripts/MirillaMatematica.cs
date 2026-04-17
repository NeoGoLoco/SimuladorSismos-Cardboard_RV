using UnityEngine;

// Proyecta la mirilla en el espacio VR, ajustando su profundidad para evitar que se hunda en paredes o el suelo.
public class MirillaMatematica : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraVR;

    [Header("Configuración")]
    public float distanciaMax = 1.5f;
    public LayerMask capasQueBloquean; // Capas que deben detener la mirilla (Piso, Paredes, etc.)

    void LateUpdate()
    {
        if (camaraVR == null) return;

        // 1. CÁLCULO DE POSICIÓN (Evitar clipping)
        RaycastHit hit;
        
        // Lanzamos un rayo de seguridad para detectar obstáculos antes de la distancia máxima
        if (Physics.Raycast(camaraVR.transform.position, camaraVR.transform.forward, out hit, distanciaMax, capasQueBloquean))
        {
            // Si hay un obstáculo, posicionamos la mirilla un poco antes del punto de impacto
            // El offset de 0.05f evita que la mirilla se parpadee o se entierre visualmente.
            transform.position = hit.point - (camaraVR.transform.forward * 0.05f);
        }
        else
        {
            // Si el camino está despejado, la situamos a la distancia de confort visual definida
            transform.position = camaraVR.transform.position + (camaraVR.transform.forward * distanciaMax);
        }

        // 2. ORIENTACIÓN ESPACIAL
        // Alineamos la rotación de la mirilla con la vista del usuario para que siempre esté "de frente"
        // Usamos el 'up' de la cámara para que la mirilla mantenga la inclinación natural de la cabeza.
        transform.rotation = Quaternion.LookRotation(camaraVR.transform.forward, camaraVR.transform.up);
    }
}