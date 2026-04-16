using UnityEngine;
using System.Collections;

//Este script debe ir en la escena del Laberinto, no en el Menú... Me lo robéxD
//Este script es para evitar desfases de la iluminación al cambiar de escenas. Por ejemplo si en la escena [1]
//Hay un cielo con un vídeo.
public class SceneFadeIn : MonoBehaviour
{
    // Se puede arrastrar aquí cualquier objeto para darle un brillo "adecuado al entorno"(?
    public Light directionalLight;

    public float finalLightIntensity = 1.0f;

    public float fadeDuration = 3.0f;

    private Color targetAmbientColor;

    private void Start()
    {
        // Configuraciones iniciales (todo debe empezar oscuro)

        // Asignamos la luz direccional por defecto si no se ha asignado
        if (directionalLight == null)
            directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();

        if (directionalLight != null)
        {
            directionalLight.intensity = 0f; // Empezamos sin luz solar
        }

        // Si se usa el modo 'Color' para la iluminación ambiental, también lo desvanecemos
        if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Flat)
        {
            targetAmbientColor = RenderSettings.ambientLight;
            RenderSettings.ambientLight = Color.black; // Empezamos con luz ambiental negra
        }

        // Iniciamos la Corrutina de desvanecimiento
        StartCoroutine(PerformFadeIn());
    }

    IEnumerator PerformFadeIn()
    {
        // Esperamos un frame para evitar tirones en la carga
        yield return null;

        float currentTime = 0f;

        // Bucle que dura exactamente la 'fadeDuration'
        while (currentTime < fadeDuration)
        {
            // Incrementamos el tiempo transcurrido
            currentTime += Time.deltaTime;

            // Calculamos el progreso (un valor entre 0 y 1)
            float t = currentTime / fadeDuration;

            // Aplicamos el incremento de luz (LERP)

            // Desvanecemos la intensidad de la luz direccional
            if (directionalLight != null)
            {
                directionalLight.intensity = Mathf.Lerp(0f, finalLightIntensity, t);
            }

            // Desvanecemos el color ambiental
            if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Flat)
            {
                RenderSettings.ambientLight = Color.Lerp(Color.black, targetAmbientColor, t);
            }

            // Esperamos al siguiente frame
            yield return null;
        }

        // Aseguramos valores finales perfectos para evitar errores de redondeo
        if (directionalLight != null) directionalLight.intensity = finalLightIntensity;
        if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Flat) RenderSettings.ambientLight = targetAmbientColor;

        // Desactivamos el script para ahorrar rendimiento
        this.enabled = false;

        //Es un huevo para arreglar un simples cambios de iluminación:( y hacer una entrada oscura
    }
}