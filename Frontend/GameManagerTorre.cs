using UnityEngine;
using TMPro; // Para los textos UI
using UnityEngine.SceneManagement;

public class GameManagerTorre : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject panelInicio;
    public GameObject panelJuego;
    public GameObject panelFin;

    [Header("Textos de UI")]
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoAlturaActual;
    public TextMeshProUGUI textoAlturaFinal;

    [Header("Referencias del Juego")]
    public MecanicaAgarre mano;
    public GeneradorInfinito spawner;
    public Transform suelo; // Para medir desde dónde empieza la torre

    private float tiempoRestante = 45f;
    private float alturaMaxima = 0f;

    void Start()
    {
        // Estado inicial: Mostramos menú, ocultamos lo demás
        panelInicio.SetActive(true);
        panelJuego.SetActive(false);
        panelFin.SetActive(false);
        
        mano.juegoActivo = false;
    }

    // Esta función la llamará el botón "Comenzar"
    public void BotonComenzar()
    {
        panelInicio.SetActive(false);
        panelJuego.SetActive(true);
        
        mano.juegoActivo = true;
        spawner.CrearPieza(); // Soltamos la primera pieza
    }

    void Update()
    {
        if (mano.juegoActivo)
        {
            // 1. Reloj de 90 segundos
            tiempoRestante -= Time.deltaTime;
            textoTiempo.text = "Tiempo: " + Mathf.Ceil(tiempoRestante).ToString() + "s";

            // 2. Calcular la altura en tiempo real
            CalcularAltura();

            // 3. Fin del tiempo
            if (tiempoRestante <= 0)
            {
                TerminarReto();
            }
        }
    }

    void CalcularAltura()
    {
        GameObject[] bloques = GameObject.FindGameObjectsWithTag("Bloque");
        alturaMaxima = 0f;

        foreach(GameObject b in bloques)
        {
            // Medimos la distancia vertical desde el suelo hasta el bloque
            float alturaBloque = b.transform.position.y - suelo.position.y;
            
            if (alturaBloque > alturaMaxima)
            {
                alturaMaxima = alturaBloque;
            }
        }

        // Mostramos la altura con 1 decimal
        textoAlturaActual.text = "Altura: " + alturaMaxima.ToString("F1") + "m"; 
    }

    void TerminarReto()
    {
        mano.juegoActivo = false;
        panelJuego.SetActive(false);
        panelFin.SetActive(true);
        
        textoAlturaFinal.text = "Altura conseguida:\n" + alturaMaxima.ToString("F1") + " metros";
    }

    // Esta función la llamará el botón "Volver a Intentar"
    public void BotonReintentar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}