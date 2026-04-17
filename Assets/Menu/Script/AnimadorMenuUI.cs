using UnityEngine;

// Crea dinamismo en la interfaz mediante efectos de oscilación y pulso que funcionan de forma independiente al tiempo del juego.
public class AnimadorMenuUI : MonoBehaviour
{
    [Header("Efecto de Flote (Botones, Textos, Imágenes)")]
    public RectTransform[] elementosFlotantes;
    public float velocidadFlote = 2f;
    public float alturaFlote = 10f;

    [Header("Efecto de Latido (Título)")]
    public RectTransform titulo;
    public float velocidadLatido = 3f;
    public float variacionEscala = 0.05f;

    private Vector3[] posicionesIniciales;
    private Vector3 escalaInicialTitulo;

    void Start()
    {
        // Guardamos las coordenadas locales originales para que el flote sea relativo a su posición en el Canvas
        posicionesIniciales = new Vector3[elementosFlotantes.Length];
        for (int i = 0; i < elementosFlotantes.Length; i++)
        {
            if (elementosFlotantes[i] != null)
                posicionesIniciales[i] = elementosFlotantes[i].anchoredPosition;
        }

        if (titulo != null)
            escalaInicialTitulo = titulo.localScale;
    }

    void Update()
    {
        // UNSCALED TIME -> Esto es vital para que las animaciones sigan funcionando 
        // aunque el juego esté en pausa (Time.timeScale = 0).
        float ondaTiempo = Time.unscaledTime;

        // ANIMACIÓN DE FLOTE
        // Aplicamos un movimiento sinusoidal a la posición vertical (Y) de cada elemento de la lista
        for (int i = 0; i < elementosFlotantes.Length; i++)
        {
            if (elementosFlotantes[i] != null)
            {
                float nuevoY = posicionesIniciales[i].y + (Mathf.Sin(ondaTiempo * velocidadFlote) * alturaFlote);
                elementosFlotantes[i].anchoredPosition = new Vector2(posicionesIniciales[i].x, nuevoY);
            }
        }

        // ANIMACIÓN DE LATIDO
        // Modificamos la escala del título para crear un efecto de "respiración" o pulsación constante
        if (titulo != null)
        {
            float multiplicadorEscala = 1f + (Mathf.Sin(ondaTiempo * velocidadLatido) * variacionEscala);
            titulo.localScale = escalaInicialTitulo * multiplicadorEscala;
        }
    }
}