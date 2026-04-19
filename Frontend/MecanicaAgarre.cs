using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class MecanicaAgarre : MonoBehaviour
{
    [Header("Efectos de Sonido")]
    public AudioSource fuenteAudioMano;
    public AudioClip sonidoPop;
    
    // --- NUEVO: REFERENCIAS DE IMÁGENES (SPRITES) ---
    [Header("Imágenes de la Mano")]
    public Sprite manoAbierta; // Arrastra tu imagen de mano abierta aquí
    public Sprite manoCerrada; // Arrastra tu imagen de mano cerrada aquí
    private SpriteRenderer spriteRendererMano; // El componente que dibuja la imagen

    // --- Red y Movimiento ---
    Thread receiveThread;
    UdpClient client;
    public int puerto = 5052;
    private float targetX = 0f;
    private float targetY = 0f;
    public float multiplicadorRangoX = 15f; 
    public float multiplicadorRangoY = 10f; 

    // --- Mecánicas del Juego ---
    public GeneradorInfinito spawner; 
    public Image circuloProgreso;
    
    public float tiempoParaAgarrar = 2f; 
    public float tiempoParaSoltar = 5f;
    public bool juegoActivo = false;
    private float timerActual = 0f;
    private Rigidbody2D rbMano;
    private Rigidbody2D bloqueHover;
    private FixedJoint2D jointAgarre;

    enum EstadoMano { Libre, IntentandoAgarre, Agarrando }
    private EstadoMano estado = EstadoMano.Libre;

    void Start()
    {
        rbMano = GetComponent<Rigidbody2D>();
        
        // --- NUEVO: OBTENER EL SPRITERENDERER ---
        spriteRendererMano = GetComponent<SpriteRenderer>();
        // Asegurarnos de que empiece con la mano abierta
        if(spriteRendererMano != null && manoAbierta != null)
        {
            spriteRendererMano.sprite = manoAbierta;
        }

        if(circuloProgreso != null) circuloProgreso.fillAmount = 0f;
        
        // Iniciamos el receptor aquí mismo para ahorrar tiempo
        receiveThread = new Thread(new ThreadStart(RecibirDatos));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // --- HILO DE RED (Sin Cambios) ---
    private void RecibirDatos()
    {
        client = new UdpClient(puerto);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP); 
                string text = Encoding.UTF8.GetString(data);
                string[] splitData = text.Split(',');
                
                if (splitData.Length == 2)
                {
                    float parsedX = float.Parse(splitData[0], System.Globalization.CultureInfo.InvariantCulture);
                    float parsedY = float.Parse(splitData[1], System.Globalization.CultureInfo.InvariantCulture);
                    targetX = (parsedX - 0.5f) * multiplicadorRangoX;
                    targetY = -(parsedY - 0.5f) * multiplicadorRangoY; 
                }
            }
            catch { break; }
        }
    }

    void FixedUpdate()
    {
        Vector2 targetPosition = new Vector2(targetX, targetY);
        rbMano.MovePosition(Vector2.Lerp(rbMano.position, targetPosition, 0.2f));
    }

    // --- LÓGICA DE AGARRE (Sin Cambios) ---
    void OnTriggerStay2D(Collider2D collision)
    {
        if (!juegoActivo) return;
        if (estado == EstadoMano.Libre && collision.CompareTag("Bloque"))
        {
            estado = EstadoMano.IntentandoAgarre;
            bloqueHover = collision.attachedRigidbody;
        }

        if (estado == EstadoMano.IntentandoAgarre && collision.attachedRigidbody == bloqueHover)
        {
            timerActual += Time.deltaTime;
            if(circuloProgreso != null) circuloProgreso.fillAmount = timerActual / tiempoParaAgarrar;

            if (timerActual >= tiempoParaAgarrar) AgarrarBloque();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (estado == EstadoMano.IntentandoAgarre && collision.attachedRigidbody == bloqueHover)
        {
            estado = EstadoMano.Libre;
            timerActual = 0f;
            bloqueHover = null;
            if(circuloProgreso != null) circuloProgreso.fillAmount = 0f;
        }
    }

    void Update()
    {
        if (estado == EstadoMano.Agarrando)
        {
            timerActual += Time.deltaTime;
            if(circuloProgreso != null) circuloProgreso.fillAmount = 1f - (timerActual / tiempoParaSoltar);

            if (timerActual >= tiempoParaSoltar) SoltarBloque();
        }
    }

    void AgarrarBloque()
    {
        fuenteAudioMano.PlayOneShot(sonidoPop);
        estado = EstadoMano.Agarrando;
        timerActual = 0f;
        
        jointAgarre = gameObject.AddComponent<FixedJoint2D>();
        jointAgarre.connectedBody = bloqueHover;
        
        // --- NUEVO: CAMBIAR A MANO CERRADA ---
        if(spriteRendererMano != null && manoCerrada != null)
        {
            spriteRendererMano.sprite = manoCerrada;
        }
        
        if(circuloProgreso != null) circuloProgreso.color = Color.red; 
    }

    void SoltarBloque()
    {
        fuenteAudioMano.PlayOneShot(sonidoPop);
        estado = EstadoMano.Libre;
        timerActual = 0f;
        bloqueHover = null;
        if (jointAgarre != null) Destroy(jointAgarre);
        
        // --- NUEVO: CAMBIAR A MANO ABIERTA ---
        if(spriteRendererMano != null && manoAbierta != null)
        {
            spriteRendererMano.sprite = manoAbierta;
        }
        
        if(circuloProgreso != null) 
        {
            circuloProgreso.fillAmount = 0f;
            circuloProgreso.color = Color.yellow;
        }

        if (spawner != null) spawner.CrearPieza();
    }

    // ¡ESTE ES EL CAMBIO MÁGICO!
    void OnDestroy()
    {
        // Cerramos el puerto de red
        if (client != null) 
        {
            client.Close();
        }

        // Matamos el hilo para que no se quede corriendo en el fondo
        if (receiveThread != null && receiveThread.IsAlive) 
        {
            receiveThread.Abort();
        }
    }
}