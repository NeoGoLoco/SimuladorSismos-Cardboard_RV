using UnityEngine;

public class MirillaMatematica : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraVR;

    [Header("Configuración")]
    public float distanciaMax = 1.5f;
    public LayerMask capasQueBloquean; // Selecciona "Default" y las capas de tus paredes/piso

    void LateUpdate()
    {
        if (camaraVR == null) return;

        // 1. SOLUCIÓN AL SUELO (Rayo de seguridad)
        RaycastHit hit;
        // Lanzamos un rayo para ver si hay una pared o el piso antes de los 1.5 metros
        if (Physics.Raycast(camaraVR.transform.position, camaraVR.transform.forward, out hit, distanciaMax, capasQueBloquean))
        {
            // Si chocamos, ponemos la mirilla justo un milímetro ANTES de la pared para que no se hunda
            transform.position = hit.point - (camaraVR.transform.forward * 0.05f);
        }
        else
        {
            // Si hay espacio libre, la ponemos a la distancia matemática perfecta
            transform.position = camaraVR.transform.position + (camaraVR.transform.forward * distanciaMax);
        }

        // 2. SOLUCIÓN AL HOMBRO (Girar con la cabeza)
        // Le pasamos un segundo parámetro: cuál es la "coronilla" de tu cabeza (transform.up)
        transform.rotation = Quaternion.LookRotation(camaraVR.transform.forward, camaraVR.transform.up);
    }
}