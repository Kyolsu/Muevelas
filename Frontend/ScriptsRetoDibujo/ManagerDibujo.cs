using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Para poder volver al menú

public class ManagerDibujo : MonoBehaviour
{
    [Header("UI por Turnos")]
    public TextMeshProUGUI textoAvisoTurno; 
    public TextMeshProUGUI textoCentral; 
    public TextMeshProUGUI textoReloj;
    public TextMeshProUGUI textoPuntaje;
    public Image imagenPersonajeActivo; // Para mostrar quién está dibujando

    [Header("UI Pantalla Final")]
    public GameObject panelFinal;
    public TextMeshProUGUI textoGanadorMinijuego;
    public TextMeshProUGUI textoCampeonGlobal;

    [Header("Referencias del Mundo")]
    public ManoPincel mano;
    public Transform contenedorPuntos;
    public GameObject plantillaForma; 

    [Header("Configuración")]
    public float tiempoDibujo = 20f;

    private int totalPuntos = 0;
    private int puntosTocados = 0;
    private bool juegoActivo = false;
    private int turnoActual = 1;

    private float precisionP1 = 0f;
    private float precisionP2 = 0f;

    void Start()
    {
        // Contamos cuántos puntos de control hay en la plantilla
        totalPuntos = contenedorPuntos.childCount;
        foreach(Transform hijo in contenedorPuntos)
        {
            hijo.GetComponent<PuntoControl>().manager = this;
        }
        
        panelFinal.SetActive(false);
        IniciarTurno(1);
    }

    void IniciarTurno(int turno)
    {
        turnoActual = turno;
        puntosTocados = 0; // Reiniciamos el contador
        
        // 1. Limpiamos la tinta y los puntos del turno anterior
        mano.LimpiarPincel();
        foreach(Transform hijo in contenedorPuntos)
        {
            hijo.GetComponent<PuntoControl>().ReiniciarPunto();
        }

        // 2. Leemos la imagen del jugador desde el Cerebro Global
        int idPersonaje = (turno == 1) ? DatosPersonaje.Instancia.idP1 : DatosPersonaje.Instancia.idP2;
        imagenPersonajeActivo.sprite = DatosPersonaje.Instancia.baseDeDatosGlobal[idPersonaje].imagen;
        textoAvisoTurno.text = "¡Turno del Jugador " + turno + "!";

        StartCoroutine(SecuenciaInicio());
    }

    IEnumerator SecuenciaInicio()
    {
        mano.puedeDibujar = false;
        if(plantillaForma != null) plantillaForma.SetActive(true);
        
        float conteo = 3f; // 3 segundos para prepararse
        while (conteo > 0)
        {
            textoCentral.text = Mathf.Ceil(conteo).ToString();
            yield return new WaitForSeconds(1f);
            conteo--;
        }

        textoCentral.text = "¡DIBUJA!";
        yield return new WaitForSeconds(0.5f);
        textoCentral.text = ""; 

        juegoActivo = true;
        mano.puedeDibujar = true;
        StartCoroutine(TemporizadorJuego());
    }

    IEnumerator TemporizadorJuego()
    {
        float reloj = tiempoDibujo;
        while (reloj > 0)
        {
            reloj -= Time.deltaTime;
            textoReloj.text = "Tiempo: " + Mathf.Ceil(reloj) + "s";
            
            float porcentajeActual = ((float)puntosTocados / totalPuntos) * 100f;
            textoPuntaje.text = "Precisión: " + Mathf.RoundToInt(porcentajeActual) + "%";
            
            yield return null;
        }

        FinalizarTurno();
    }

    public void RegistrarPuntoTocado()
    {
        if(juegoActivo) puntosTocados++;
    }

    void FinalizarTurno()
    {
        juegoActivo = false;
        mano.puedeDibujar = false;
        if(plantillaForma != null) plantillaForma.SetActive(false);
        
        float precisionFinal = ((float)puntosTocados / totalPuntos) * 100f;
        textoCentral.text = "¡TIEMPO!";

        if (turnoActual == 1)
        {
            precisionP1 = precisionFinal;
            textoAvisoTurno.text = "Precisión P1: " + Mathf.RoundToInt(precisionP1) + "%";
            Invoke("CambiarAlTurnoDos", 3f); // Esperamos 3 segs y va el P2
        }
        else
        {
            precisionP2 = precisionFinal;
            textoAvisoTurno.text = "Precisión P2: " + Mathf.RoundToInt(precisionP2) + "%";
            Invoke("MostrarResultadosFinales", 3f);
        }
    }

    void CambiarAlTurnoDos()
    {
        textoCentral.text = "";
        IniciarTurno(2);
    }

    void MostrarResultadosFinales()
    {
        panelFinal.SetActive(true);
        textoAvisoTurno.text = "";
        textoCentral.text = "";

        // --- 1. ¿QUIÉN GANÓ EL DIBUJO? ---
        if (precisionP1 > precisionP2)
        {
            textoGanadorMinijuego.text = "GANADOR DIBUJO: ¡JUGADOR 1!";
            DatosPersonaje.Instancia.victoriasP1++;
        }
        else if (precisionP2 > precisionP1)
        {
            textoGanadorMinijuego.text = "GANADOR DIBUJO: ¡JUGADOR 2!";
            DatosPersonaje.Instancia.victoriasP2++;
        }
        else
        {
            textoGanadorMinijuego.text = "DIBUJO: ¡EMPATE!";
        }

        // --- 2. ¿QUIÉN GANÓ TODO EL JUEGO? ---
        int totalP1 = DatosPersonaje.Instancia.victoriasP1;
        int totalP2 = DatosPersonaje.Instancia.victoriasP2;

        if (totalP1 > totalP2) textoCampeonGlobal.text = "¡CAMPEÓN SUPREMO: JUGADOR 1!";
        else if (totalP2 > totalP1) textoCampeonGlobal.text = "¡CAMPEÓN SUPREMO: JUGADOR 2!";
        else textoCampeonGlobal.text = "¡EL JUEGO TERMINÓ EN EMPATE TÉCNICO!";
    }

    // --- FUNCIÓN PARA EL BOTÓN DE "VOLVER A JUGAR" ---
    public void VolverAlMenu()
    {
        // Limpiamos el historial en el Cerebro Global
        DatosPersonaje.Instancia.ReiniciarJuego();
        
        // Cargamos la escena principal (Pon el nombre real de tu Menú)
        SceneManager.LoadScene("MenuPrincipal"); 
    }
}