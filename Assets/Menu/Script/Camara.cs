using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camara : MonoBehaviour
{
    public GameObject Jugador;
    public GameObject referencia;

    [Header("Configuración de Colisión")]
    public float distanciaMaxima = 5f; // La distancia ideal de la cámara
    public float radioColision = 0.2f; // Qué tan "ancha" es la cámara para chocar
    public LayerMask capasObstaculos;  // Selecciona "Default" o las capas que tengan tus paredes

    private Vector3 direccionOffset;
    private float distanciaActual;

    void Start()
    {
        // Guardamos la dirección inicial y la distancia
        direccionOffset = (transform.position - Jugador.transform.position).normalized;
        distanciaActual = (transform.position - Jugador.transform.position).magnitude;
        distanciaMaxima = distanciaActual;
    }

    void LateUpdate()
    {
        float giroHorizontal = 0f;
        var gamepad = Gamepad.current;

        if (gamepad != null)
        {
            giroHorizontal = gamepad.rightStick.ReadValue().x;
        }
        else
        {
            giroHorizontal = Input.GetAxis("Mouse X");
        }

        // Giramos la dirección base del offset
        direccionOffset = Quaternion.AngleAxis(giroHorizontal * 5, Vector3.up) * direccionOffset;

        // Calculamos la posición deseada (donde "querría" estar la cámara)
        Vector3 posicionDeseada = Jugador.transform.position + (direccionOffset * distanciaMaxima);

        // (Detección de Paredes)
        RaycastHit hit;
        // Lanzamos un rayo desde el jugador hacia la cámara
        if (Physics.SphereCast(Jugador.transform.position, radioColision, direccionOffset, out hit, distanciaMaxima, capasObstaculos))
        {
            // Si choca, la nueva distancia es donde golpeó el rayo. Un poquito antes para no traspasar la pared
            distanciaActual = hit.distance;
        }
        else
        {
            // Si no hay nada, volvemos a la distancia máxima poco a poco suavemente
            distanciaActual = Mathf.Lerp(distanciaActual, distanciaMaxima, Time.deltaTime * 5f);
        }

        
        transform.position = Jugador.transform.position + (direccionOffset * distanciaActual);
        transform.LookAt(Jugador.transform.position);

        
        Vector3 copiaRotacion = new Vector3(0, transform.eulerAngles.y, 0);
        referencia.transform.eulerAngles = copiaRotacion;
    }
}