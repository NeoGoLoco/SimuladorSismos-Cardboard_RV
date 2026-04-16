using UnityEngine;

public class MusicDataManager : MonoBehaviour
{
    // Instancia (Mientras) para que cualquier objeto pueda acceder sin referencias
    public static MusicDataManager Instance;

    [Header("Referencias")]
    public AudioSource musica;

    [Header("Sensibilidad Global")]
    public float sensibilidadBajos = 60f;
    public float sensibilidadMedios = 90f;
    public float sensibilidadAgudos = 150f;

    // Arreglo de los 512 samples/bins o cubetas para las frecuencias
    private float[] samplesAudio = new float[512];

    [HideInInspector] public float ValorBajo;  
    [HideInInspector] public float ValorMedio; 
    [HideInInspector] public float ValorAgudo;  

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (musica == null) musica = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (musica == null || !musica.isPlaying) return;

        // Extraemos el espectro de la canción una sola vez por fotograma
        musica.GetSpectrumData(samplesAudio, 0, FFTWindow.Blackman);

        // FRECUENCIAS: Los promedios->

        // BAJOS (0 a 10 samples) - Los beats
        float bajoTotal = 0f;
        for (int i = 0; i < 10; i++) bajoTotal += samplesAudio[i];
        ValorBajo = Mathf.Clamp01((bajoTotal / 10f) * sensibilidadBajos);

        // MEDIOS (10 a 250 samples) - Voces y guitarras
        float medioTotal = 0f;
        for (int i = 10; i < 250; i++) medioTotal += samplesAudio[i];
        ValorMedio = Mathf.Clamp01((medioTotal / 240f) * sensibilidadMedios);

        // AGUDOS (250 a 511 samples) - Platillos, silbidos
        float agudoTotal = 0f;
        for (int i = 250; i < 512; i++) agudoTotal += samplesAudio[i];
        ValorAgudo = Mathf.Clamp01((agudoTotal / 262f) * sensibilidadAgudos);
    }
}