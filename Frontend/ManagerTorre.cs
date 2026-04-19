using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static DatosPersonaje; // Conectamos con el Cerebro Global

public class ManagerTorre : MonoBehaviour
{
    [Header("UI y Textos")]
    public TextMeshProUGUI textoTurno;
    public TextMeshProUGUI textoReloj;
    public TextMeshProUGUI textoAlturaEnVivo;
    public Image imagenPersonajeActivo; // Para mostrar quién está jugando

    [Header("Base de Datos de Personajes")]

    [Header("Configuración del Turno")]
    public float tiempoPorTurno = 30f;
    private float tiempoRestante;
    private int turnoActual = 1;

    private float alturaP1 = 0f;
    private float alturaP2 = 0f;

    [Header("Referencias de tu Mecánica")]
    public Transform baseTorre; 
    public GeneradorInfinito spawner; // ¡Tu spawner real!
    public MecanicaAgarre manoScript; // Tu script de la mano
    void Start()
    {
        IniciarTurno(1);
    }

    void IniciarTurno(int turno)
    {
        turnoActual = turno;
        tiempoRestante = tiempoPorTurno;

        int idPersonaje = (turno == 1) ? DatosPersonaje.Instancia.idP1 : DatosPersonaje.Instancia.idP2;
        textoTurno.text = "¡Turno del Jugador " + turno + "!";
        imagenPersonajeActivo.sprite = DatosPersonaje.Instancia.baseDeDatosGlobal[idPersonaje].imagen;

        // Prendemos tu mano
        manoScript.gameObject.SetActive(true);
        manoScript.juegoActivo = true;

        // ¡Le decimos a TU spawner que suelte el primer bloque!
        if (spawner != null) spawner.CrearPieza();
    }

    void Update()
    {
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            textoReloj.text = Mathf.Ceil(tiempoRestante).ToString();

            if (textoAlturaEnVivo != null)
            {
                textoAlturaEnVivo.text = "Altura: " + CalcularAlturaMaxima().ToString("F1") + "m";
            }

            if (tiempoRestante <= 0)
            {
                FinalizarTurno();
            }
        }
    }

    // ¡Es pública para que los bloques le puedan avisar cuando ya cayeron!
    void FinalizarTurno()
    {
        manoScript.juegoActivo = false; 
        manoScript.gameObject.SetActive(false);

        if (turnoActual == 1)
        {
            alturaP1 = CalcularAlturaMaxima();
            textoTurno.text = "Altura P1: " + alturaP1.ToString("F1") + "m";
            
            // Esperamos 3 segundos para que vean su puntaje y cambiamos de turno
            Invoke("LimpiarYCambiarTurno", 3f); 
        }
        else
        {
            alturaP2 = CalcularAlturaMaxima();
            textoTurno.text = "Altura P2: " + alturaP2.ToString("F1") + "m";
            
            Invoke("AnunciarGanador", 3f);
        }
    }

    float CalcularAlturaMaxima()
    {
        // ¡SUPER IMPORTANTE! Asegúrate de que tu prefab del bloque tenga el Tag "Bloque"
        GameObject[] bloques = GameObject.FindGameObjectsWithTag("Bloque");
        float maxAltura = baseTorre.position.y;

        foreach (GameObject bloque in bloques)
        {
            if (bloque.transform.position.y > maxAltura)
            {
                maxAltura = bloque.transform.position.y;
            }
        }

        // Regresamos cuántos metros subió la torre desde la base
        return maxAltura - baseTorre.position.y;
    }

    void LimpiarYCambiarTurno()
    {
        // Destruimos la torre vieja del P1
        GameObject[] bloques = GameObject.FindGameObjectsWithTag("Bloque");
        foreach (GameObject bloque in bloques)
        {
            Destroy(bloque);
        }

        // Iniciamos el turno del P2
        IniciarTurno(2);
    }

    void AnunciarGanador()
    {
        if (alturaP1 > alturaP2)
        {
            textoTurno.text = "¡JUGADOR 1 GANA!";
            DatosPersonaje.Instancia.victoriasP1++; // Ahora le hablamos al cerebro
        }
        else if (alturaP2 > alturaP1)
        {
            textoTurno.text = "¡JUGADOR 2 GANA!";
            DatosPersonaje.Instancia.victoriasP2++; 
        }
        else
        {
            textoTurno.text = "¡EMPATE!";
        }

        // Avanzamos de ronda
        DatosPersonaje.Instancia.rondaActual++; 
        
        // Esperamos 4 segundos y regresamos al tablero
        Invoke("RegresarAlMapa", 4f);
    }

    void RegresarAlMapa()
    {
        // Regresamos a tu escena intermedia
        SceneManager.LoadScene("Nivel1"); 
    }
}