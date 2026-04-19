using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking; // ¡Súper importante para la API!
using UnityEngine.SceneManagement;

public class ManagerTrivia : MonoBehaviour
{
    [Header("Efectos de Sonido")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoGolpe;      // punch.mp3
    public AudioClip sonidoVictoria;   // winners.mp3
    public AudioClip sonidoHover;      // bubble-pop.mp3
    public AudioClip sonidoCorrecto;   // correct.mp3
    public AudioClip sonidoError;      // error.mp3
    
    [Header("UI del Juego")]
    public TextMeshProUGUI textoPregunta;
    public TextMeshProUGUI textoReloj;
    public TextMeshProUGUI textoAvisoTurno;
    public TextMeshProUGUI textoHPP1, textoHPP2;
    public GameObject contenedorCombate;

    [Header("Sprites de Personajes")]
    public Image imagenVisualP1;
    public Image imagenVisualP2;
    public Sprite p1Estatico, p1Golpe;
    public Sprite p2Estatico, p2Golpe;

    [Header("Referencias Técnicas")]
    public ManoCursor mano;
    public GameObject prefabRespuesta;
    public Transform[] posicionesRespuestas;

    private int vidaP1 = 10, vidaP2 = 10;
    private int turnoActual = 1;
    private float tiempoTurno = 30f;
    private int dañoAcumulado = 0;

    private List<GameObject> respuestasActivas = new List<GameObject>();

    void Start()
    {
        ActualizarHUD();
        StartCoroutine(RutinaTurno());
    }

    IEnumerator RutinaTurno()
    {
        yield return StartCoroutine(PrepararBodegaIA());
        // --- FASE 1: INTRO ---
        LimpiarPantalla();
        mano.puedeJugar = false;
        contenedorCombate.SetActive(true);
        dañoAcumulado = 0;

        imagenVisualP1.gameObject.SetActive(turnoActual == 1);
        imagenVisualP2.gameObject.SetActive(turnoActual == 2);

        textoAvisoTurno.text = "¡Prepárate Jugador " + turnoActual + "!";
        yield return new WaitForSeconds(3f);

        // --- FASE 2: TRIVIA (Llamada a la API) ---
        textoAvisoTurno.text = "Conectando con la IA...";
        contenedorCombate.SetActive(false); // Escondemos personajes durante la trivia
        
        // Pedimos la primera pregunta a Python y esperamos a que llegue
        // Pedimos la pregunta a la IA
        yield return StartCoroutine(PedirPreguntaAPI(nuevaPregunta => {
            textoAvisoTurno.text = "";
            ConstruirBotonesPregunta(nuevaPregunta);
            mano.puedeJugar = true;
            
            // --- ¡NUEVO! REPRODUCIMOS LA VOZ ---
            if (!string.IsNullOrEmpty(nuevaPregunta.audio_url))
            {
                StartCoroutine(ReproducirVozIA(nuevaPregunta.audio_url));
            }
        }));

        // Arrancamos el reloj de 30 segundos
        tiempoTurno = 30f;
        while (tiempoTurno > 0)
        {
            tiempoTurno -= Time.deltaTime;
            textoReloj.text = Mathf.Ceil(tiempoTurno).ToString();
            yield return null;
        }

        // --- FASE 3: COMBATE ---
        mano.puedeJugar = false;
        LimpiarPantalla();
        textoReloj.text = "";
        contenedorCombate.SetActive(true);
        imagenVisualP1.gameObject.SetActive(true);
        imagenVisualP2.gameObject.SetActive(true);

        // Ejecutamos la animación de los golpes conseguidos
        yield return StartCoroutine(AnimacionCombate(dañoAcumulado));

        // Aplicar daño real
        if (turnoActual == 1) vidaP2 -= dañoAcumulado;
        else vidaP1 -= dañoAcumulado;

        ActualizarHUD();
        ComprobarGanador();
    }

    // --- CONEXIÓN CON FLASK / PYTHON ---
    IEnumerator PedirPreguntaAPI(System.Action<Pregunta> callback)
    {
        // Cambia 'historia de mexico' por la temática que quieras en el juego final
        string urlAPI = "http://127.0.0.1:5000/pregunta?tematica=cultura_general";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlAPI))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error de API: " + webRequest.error);
                
                // Pregunta de rescate por si falla el servidor de Python
                Pregunta rescate = new Pregunta();
                rescate.pregunta = "¿Error de conexión. Cuánto es 2+2?";
                rescate.opciones = new string[] { "4", "3", "5", "1" };
                rescate.correcta = "4";
                callback(rescate);
            }
            else
            {
                string jsonRespuesta = webRequest.downloadHandler.text;
                Debug.Log("JSON CRUDO DE PYTHON: " + jsonRespuesta);
                Pregunta preguntaGenerada = JsonUtility.FromJson<Pregunta>(jsonRespuesta);
                callback(preguntaGenerada);
            }
        }
    }

    // --- DESCARGAR Y REPRODUCIR VOZ DE LA IA ---
    // --- DESCARGAR Y REPRODUCIR VOZ DE LA IA ---
    // --- DESCARGAR Y REPRODUCIR VOZ DE LA IA ---
    IEnumerator ReproducirVozIA(string urlAudio)
    {
        Debug.Log("La IA me mandó este link: " + urlAudio); // <-- Esto nos dirá la verdad absoluta

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(urlAudio, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error descargando el audio: " + www.error);
            }
            else
            {
                AudioClip clipVoz = DownloadHandlerAudioClip.GetContent(www);
                if (clipVoz != null && clipVoz.length > 0)
                {
                    fuenteAudio.PlayOneShot(clipVoz);
                }
                else
                {
                    Debug.LogError("El archivo llegó vacío. Flask no lo está enviando bien.");
                }
            }
        }
    }

    IEnumerator PrepararBodegaIA()
    {
        textoAvisoTurno.text = "Generando bodega con IA...";
        string urlAPI = "http://127.0.0.1:5000/preparar-partida";
        
        // Ojo: Asegúrate de que esta temática coincida con la categoría de tu PDF
        string jsonData = "{\"tematica\":\"cultura_general\", \"cantidad\":5}";

        using (UnityWebRequest request = new UnityWebRequest(urlAPI, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al preparar la bodega: " + request.error);
            }
        }
    }
    // --- DIBUJAR PREGUNTA EN PANTALLA ---
    public void ConstruirBotonesPregunta(Pregunta p)
    {
        LimpiarPantalla();
        textoPregunta.text = p.pregunta;

        // Mezclamos las 4 opciones que nos dio la IA
        List<string> opcionesMezcladas = new List<string>(p.opciones);
        for (int i = 0; i < opcionesMezcladas.Count; i++)
        {
            string temp = opcionesMezcladas[i];
            int randomIndex = Random.Range(i, opcionesMezcladas.Count);
            opcionesMezcladas[i] = opcionesMezcladas[randomIndex];
            opcionesMezcladas[randomIndex] = temp;
        }

        // Creamos los botones físicos
        for (int i = 0; i < 4; i++)
        {
            GameObject nuevaResp = Instantiate(prefabRespuesta, posicionesRespuestas[i]);
            nuevaResp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            BloqueRespuesta scriptResp = nuevaResp.GetComponent<BloqueRespuesta>();
            scriptResp.Configurar(opcionesMezcladas[i], opcionesMezcladas[i] == p.correcta, this);
            respuestasActivas.Add(nuevaResp);
        }
    }

    public void RespuestaSeleccionada(bool esCorrecta)
    {
        if (esCorrecta)
        {
            fuenteAudio.PlayOneShot(sonidoCorrecto); 
            dañoAcumulado++;
            mano.puedeJugar = false;
            LimpiarPantalla();
            textoAvisoTurno.text = "¡Correcto! Cargando...";

            // ¡CORRECCIÓN AQUÍ! Arrancamos la corrutina de forma normal, sin "yield return"
            StartCoroutine(PedirPreguntaAPI(nuevaPregunta => {
                textoAvisoTurno.text = "";
                ConstruirBotonesPregunta(nuevaPregunta);
                mano.puedeJugar = true;
                
                // Reproducimos la voz
                if (!string.IsNullOrEmpty(nuevaPregunta.audio_url))
                {
                    StartCoroutine(ReproducirVozIA(nuevaPregunta.audio_url));
                }
            }));
        }
        else
        {
            fuenteAudio.PlayOneShot(sonidoError); 
            tiempoTurno -= 2f; 
        }
    }

    public void ReproducirSonidoHover()
    {
        // Le bajamos un poquito el volumen (0.5f) para que el "pop" no aturda si pasan rápido la mano
        fuenteAudio.PlayOneShot(sonidoHover, 0.5f); 
    }
    
    void LimpiarPantalla()
    {
        foreach (var r in respuestasActivas) Destroy(r);
        respuestasActivas.Clear();
        textoPregunta.text = "";
    }

    void ActualizarHUD()
    {
        textoHPP1.text = "P1 HP: " + vidaP1;
        textoHPP2.text = "P2 HP: " + vidaP2;
    }

    // --- ANIMACIONES Y LÓGICA FINAL ---
    IEnumerator AnimacionCombate(int golpes)
    {
        textoAvisoTurno.text = "¡ATAQUE!";

        for (int i = 0; i < golpes; i++)
        {
            fuenteAudio.PlayOneShot(sonidoGolpe);
            if (turnoActual == 1)
            {
                imagenVisualP1.sprite = p1Golpe;
                imagenVisualP2.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                imagenVisualP1.sprite = p1Estatico;
                imagenVisualP2.color = Color.white;
            }
            else
            {
                imagenVisualP2.sprite = p2Golpe;
                imagenVisualP1.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                imagenVisualP2.sprite = p2Estatico;
                imagenVisualP1.color = Color.white;
            }
            yield return new WaitForSeconds(0.1f);
        }

        if (golpes == 0)
        {
            textoAvisoTurno.text = "¡FALLÓ!";
            yield return new WaitForSeconds(1.5f);
        }
        textoAvisoTurno.text = "";
    }

    void ComprobarGanador()
    {
        if (vidaP1 <= 0 || vidaP2 <= 0)
        {
            StartCoroutine(EscenaVictoria());
        }
        else
        {
            turnoActual = (turnoActual == 1) ? 2 : 1;
            StartCoroutine(RutinaTurno());
        }
    }

    IEnumerator EscenaVictoria()
    {
        fuenteAudio.PlayOneShot(sonidoVictoria);
        int ganador = (vidaP1 <= 0) ? 2 : 1;
        textoAvisoTurno.text = "¡JUGADOR " + ganador + " ES EL GANADOR!";

        // --- 1. REGISTRAR LA VICTORIA EN EL CEREBRO GLOBAL ---
        if (ganador == 1)
        {
            DatosPersonaje.Instancia.victoriasP1++;
        }
        else
        {
            DatosPersonaje.Instancia.victoriasP2++;
        }

        // Animación final de golpes
        for (int i = 0; i < 5; i++)
        {
            yield return StartCoroutine(AnimacionCombate(1));
        }

        // --- 2. AVANZAR AL SIGUIENTE MINIJUEGO ---
        DatosPersonaje.Instancia.rondaActual++;

        // Esperamos 2 segundos para que los jugadores celebren/lloren
        yield return new WaitForSeconds(2f);

        // --- 3. REGRESAR AL MAPA ---
        // Esto cargará "Nivel1", los personajes caminarán al bioma 3 y luego irán al juego de Dibujo
        SceneManager.LoadScene("Nivel1"); 
    }
}

// --- ESTRUCTURA DE DATOS PARA LA IA ---
[System.Serializable]
public class Pregunta
{
    public string pregunta;
    public string[] opciones;
    public string correcta;
    public string audio_url; // ¡NUEVO! Aquí guardaremos el link del mp3
}