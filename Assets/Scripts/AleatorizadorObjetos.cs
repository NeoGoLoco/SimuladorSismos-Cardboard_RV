using UnityEngine;
using System.Collections.Generic;

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
        RepartirObjetos();
    }

    public void RepartirObjetos()
    {
        // Seguridad: Asegurarnos de que hay suficientes puntos para los objetos:>
        if (puntosDeControl.Count < objetosAleatorios.Count)
        {
            Debug.LogWarning("Seguridad: Tenemos más objetos que lugares para esconderlos.");
            return;
        }

        // Crear una copia temporal de los puntos para "barajarlos" como cartas
        List<Transform> puntosDisponibles = new List<Transform>(puntosDeControl);

        // Se mezclan los puntos de "control" (Algoritmo Fisher-Yates)
        for (int i = 0; i < puntosDisponibles.Count; i++)
        {
            Transform temp = puntosDisponibles[i];
            int indiceAleatorio = Random.Range(i, puntosDisponibles.Count);
            puntosDisponibles[i] = puntosDisponibles[indiceAleatorio];
            puntosDisponibles[indiceAleatorio] = temp;
        }

        // Se le asigna a cada objeto uno de los puntos ya mezclados
        for (int i = 0; i < objetosAleatorios.Count; i++)
        {
            if (objetosAleatorios[i] != null)
            {
                // Teletransportamos el objeto a la nueva posición
                objetosAleatorios[i].transform.position = puntosDisponibles[i].position;

                // También copiamos la rotación por si el punto está inclinado... Esto es opcional
                objetosAleatorios[i].transform.rotation = puntosDisponibles[i].rotation;
            }
        }
    }
}