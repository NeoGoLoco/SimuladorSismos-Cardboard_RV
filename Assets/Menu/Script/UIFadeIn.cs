using UnityEngine;
using UnityEngine.UI; // Necesario para controlar elementos del Canvas
using System.Collections;

public class UIFadeIn : MonoBehaviour
{
    // "CortinaNegra"
    public Image cortinaNegra;

    // Duración del fundido en segundos
    public float duracionFade = 2.0f;

    void Start()
    {
        if (cortinaNegra != null)
        {
            cortinaNegra.color = new Color(0, 0, 0, 1);
            cortinaNegra.gameObject.SetActive(true);

            // Se inicia la animación
            StartCoroutine(DesvanecerCortina());
        }
    }

    IEnumerator DesvanecerCortina()
    {
        float tiempoActual = 0f;

        while (tiempoActual < duracionFade)
        {
            // unscaledDeltaTime ignora que el juego esté en Time.timeScale = 0
            tiempoActual += Time.unscaledDeltaTime;

            // Calculamos la transparencia (de 1 a 0)
            float alpha = Mathf.Lerp(1f, 0f, tiempoActual / duracionFade);

            // Aplicamos el nuevo color con la transparencia actualizada
            cortinaNegra.color = new Color(0, 0, 0, alpha);

            // yield return null espera al siguiente frame. Funciona perfecto con unscaledDeltaTime.
            yield return null;
        }

        // Al terminar, desactivamos el panel para que no consuma recursos
        cortinaNegra.gameObject.SetActive(false);
    }
}