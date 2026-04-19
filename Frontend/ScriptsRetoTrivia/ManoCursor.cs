using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ManoCursor : MonoBehaviour
{
    // --- Red y Movimiento ---
    Thread receiveThread;
    UdpClient client;
    public int puerto = 5052;
    private float targetX = 0f, targetY = 0f;
    public float multiplicadorRangoX = 15f, multiplicadorRangoY = 10f; 
    public ManagerTrivia manager; // Conexión al sonido

    // --- Mecánica de Selección (Hover) ---
    public Image circuloProgreso;
    public float tiempoParaSeleccionar = 1.5f; 
    private float timerActual = 0f;
    private GameObject respuestaHover;

    public bool puedeJugar = false; 

    void Start()
    {
        if(circuloProgreso != null) circuloProgreso.fillAmount = 0f;
        receiveThread = new Thread(new ThreadStart(RecibirDatos));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void RecibirDatos()
    {
        client = new UdpClient(puerto);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP); 
                string[] splitData = Encoding.UTF8.GetString(data).Split(',');
                if (splitData.Length == 2)
                {
                    targetX = (float.Parse(splitData[0], System.Globalization.CultureInfo.InvariantCulture) - 0.5f) * multiplicadorRangoX;
                    targetY = -(float.Parse(splitData[1], System.Globalization.CultureInfo.InvariantCulture) - 0.5f) * multiplicadorRangoY; 
                }
            } catch { break; }
        }
    }

    void FixedUpdate()
    {
        if (!puedeJugar) return;
        transform.position = Vector2.Lerp(transform.position, new Vector2(targetX, targetY), 0.2f);
    }

    // --- SELECCIONAR RESPUESTAS ---

    // 1. ENTER: Suena el "pop" UNA sola vez cuando tocas el botón
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!puedeJugar) return;

        // OJO: Asegúrate de que el Tag aquí sea exactamente el que tienen tus botones en Unity
        if (collision.CompareTag("Respuesta") || collision.CompareTag("BotonRespuesta")) 
        {
            if (manager != null)
            {
                manager.ReproducirSonidoHover();
            }

            // Empezamos a mirar este botón
            respuestaHover = collision.gameObject;
            timerActual = 0f;
        }
    }

    // 2. STAY: Va llenando la barrita de tiempo mientras dejas la mano ahí
    void OnTriggerStay2D(Collider2D collision)
    {
        if (!puedeJugar) return;

        // Verificamos que sigamos tocando el mismo botón
        if (collision.gameObject == respuestaHover)
        {
            timerActual += Time.deltaTime; // Suma el tiempo real que ha pasado
            
            if (circuloProgreso != null) 
            {
                circuloProgreso.fillAmount = timerActual / tiempoParaSeleccionar;
            }

            // ¿Ya se llenó el tiempo? Confirmamos la respuesta
            if (timerActual >= tiempoParaSeleccionar)
            {
                collision.GetComponent<BloqueRespuesta>().ComprobarRespuesta();
                
                // Reseteamos todo para no hacer "doble clic"
                timerActual = 0f;
                if (circuloProgreso != null) circuloProgreso.fillAmount = 0f;
                respuestaHover = null; 
            }
        }
    }

    // 3. EXIT: Si te arrepientes y quitas la mano antes de tiempo, se cancela
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == respuestaHover)
        {
            timerActual = 0f;
            respuestaHover = null;
            if (circuloProgreso != null) circuloProgreso.fillAmount = 0f;
        }
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