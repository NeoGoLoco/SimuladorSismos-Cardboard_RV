using UnityEngine;
using System.Collections.Generic;

// Distribuye objetos clave en posiciones aleatorias para variar la experiencia de búsqueda en cada partida.
public class AleatorizadorObjetos : MonoBehaviour
{
    [Header("Objetos a Esconder")]
    [Tooltip("Arrastra aquí tu mochila, documentos, llaves, etc.")]
    public List<GameObject> objetosAleatorios;

    [Header("Puntos de Aparición (Lógicos)")]
    [Tooltip("Arrastra aquí los GameObjects vacíos que sirven como posiciones")]
    public List<Transform> puntosDeControl;

    void Start()
    {
        // Iniciamos la distribución en cuanto el nivel carga
        RepartirObjetos();
    }

    public void RepartirObjetos()
    {
        // Validación de seguridad: evitamos errores si faltan puntos de aparición para la cantidad de objetos
        if (puntosDeControl.Count < objetosAleatorios.Count)
        {
            Debug.LogWarning("Configuración insuficiente: Hay más objetos que puntos de control disponibles.");
            return;
        }

        // Generamos una copia de trabajo de las posiciones para poder mezclarlas sin alterar las originales
        List<Transform> puntosDisponibles = new List<Transform>(puntosDeControl);

        // --- BARAJADO Lógico
        // Implementación del Algoritmo Fisher-Yates. Mezclamos los puntos como un mazo de cartas
        // para garantizar que la distribución sea verdaderamente aleatoria y no se repitan lugares.
        for (int i = 0; i < puntosDisponibles.Count; i++)
        {
            Transform temp = puntosDisponibles[i];
            int indiceAleatorio = Random.Range(i, puntosDisponibles.Count);
            puntosDisponibles[i] = puntosDisponibles[indiceAleatorio];
            puntosDisponibles[indiceAleatorio] = temp;
        }

        // La Asignación Física (Barajeo)
        // Una vez mezclados los puntos, asignamos cada objeto de la lista a una posición única
        for (int i = 0; i < objetosAleatorios.Count; i++)
        {
            if (objetosAleatorios[i] != null)
            {
                // Teletransportamos el objeto a las coordenadas del punto de control seleccionado
                objetosAleatorios[i].transform.position = puntosDisponibles[i].position;

                // Sincronizamos también la rotación por si el punto está diseñado para una superficie específica
                objetosAleatorios[i].transform.rotation = puntosDisponibles[i].rotation;
            }
        }
    }
}