using UnityEngine;

public class AnimadorMenuUI : MonoBehaviour
{
    [Header("Efecto de Flote (Botones)")]
    // Se puede agragar más de un botón
    public RectTransform[] botonesFlotantes;
    public float velocidadFlote = 2f; 
    public float alturaFlote = 10f;   

    [Header("Efecto de Latido (Título)")]
    
    public RectTransform titulo;
    public float velocidadLatido = 3f;
    public float variacionEscala = 0.05f; 

    // Variables para guardar las posiciones y tamaños originales
    private Vector3[] posicionesIniciales;
    private Vector3 escalaInicialTitulo;

    void Start()
    {
        posicionesIniciales = new Vector3[botonesFlotantes.Length];
        for (int i = 0; i < botonesFlotantes.Length; i++)
        {
            if (botonesFlotantes[i] != null)
                posicionesIniciales[i] = botonesFlotantes[i].anchoredPosition;
        }

        // Para mantener la escala del Titulo
        if (titulo != null)
            escalaInicialTitulo = titulo.localScale;
    }

    void Update()
    {
        // Creamos la "onda matemática basada en el tiempo"... Sólo es el nombrexd
        float ondaTiempo = Time.time;

        // Flote de botones
        for (int i = 0; i < botonesFlotantes.Length; i++)
        {
            if (botonesFlotantes[i] != null)
            {
                // Calculamos la nueva altura
                float nuevoY = posicionesIniciales[i].y + (Mathf.Sin(ondaTiempo * velocidadFlote) * alturaFlote);

                // Aplicamos la nueva posición, manteniendo la X original
                botonesFlotantes[i].anchoredPosition = new Vector2(posicionesIniciales[i].x, nuevoY);
            }
        }

        // Latido del Titulo
        if (titulo != null)
        {
            // El +1 al principio es para que el tamaño base sea 100% y oscile un poquito arriba y abajo
            float multiplicadorEscala = 1f + (Mathf.Sin(ondaTiempo * velocidadLatido) * variacionEscala);

            titulo.localScale = escalaInicialTitulo * multiplicadorEscala;
        }
    }
}