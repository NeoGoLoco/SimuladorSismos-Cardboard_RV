using UnityEngine;
using System.Collections;
//Este script está inutilizado... Al principio era útil para el Cardboard, pero, el de MouseLook es mejor. Y funciona en los dosxd
public class SismoVR : MonoBehaviour
{
    private Vector3 posicionOriginal;
    public float intensidad = 0.01f; // La intensidad que está menos loca

    void Start()
    {
        // Guardamos la posición inicial del Pivot
        posicionOriginal = transform.localPosition;

        // Iniciamos el temblor de inmediato al empezar el juego
        StartCoroutine(SacudirIndefinidamente());
    }

    IEnumerator SacudirIndefinidamente()
    {
        while (true)
        {
            // Generamos el desplazamiento aleatorio para el pivote
            float x = Random.Range(-1f, 1f) * intensidad;
            float y = Random.Range(-1f, 1f) * intensidad;

            // Aplicamos el movimiento respecto a la posición original
            transform.localPosition = new Vector3(
                posicionOriginal.x + x,
                posicionOriginal.y + y,
                posicionOriginal.z
            );

            // Esperamos al siguiente frame para repetir
            yield return null;
        }
    }
}