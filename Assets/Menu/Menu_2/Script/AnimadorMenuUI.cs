using UnityEngine;

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
        // Usamos unscaledTime para ignorar la pausa del juego y puedan flotar los elementos
        float ondaTiempo = Time.unscaledTime;

        // Flote de elementos
        for (int i = 0; i < elementosFlotantes.Length; i++)
        {
            if (elementosFlotantes[i] != null)
            {
                float nuevoY = posicionesIniciales[i].y + (Mathf.Sin(ondaTiempo * velocidadFlote) * alturaFlote);
                elementosFlotantes[i].anchoredPosition = new Vector2(posicionesIniciales[i].x, nuevoY);
            }
        }

        // Latido del Titulo
        if (titulo != null)
        {
            float multiplicadorEscala = 1f + (Mathf.Sin(ondaTiempo * velocidadLatido) * variacionEscala);
            titulo.localScale = escalaInicialTitulo * multiplicadorEscala;
        }
    }
}