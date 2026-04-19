using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ManoPincel : MonoBehaviour
{
    // --- Red y Movimiento ---
    Thread receiveThread;
    UdpClient client;
    public int puerto = 5052;
    private float targetX = 0f, targetY = 0f;
    public float multiplicadorRangoX = 15f, multiplicadorRangoY = 10f; 

    // --- Sistema de Pincel ---
    private LineRenderer lineRenderer;
    public bool puedeDibujar = false;
    public float distanciaMinimaPunto = 0.1f; // Qué tan seguido deja tinta

    // Agrega esta función justo arriba de tu Start()
    void Awake() 
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer.positionCount = 0; 
        
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
        Vector2 targetPosition = new Vector2(targetX, targetY);
        transform.position = Vector2.Lerp(transform.position, targetPosition, 0.2f);

        // Si el juego está activo, vamos pintando
        if (puedeDibujar)
        {
            AñadirPuntoTinta(transform.position);
        }
    }

    void AñadirPuntoTinta(Vector3 nuevaPosicion)
    {
        nuevaPosicion.z = 0; // Mantenemos todo en 2D

        int puntosActuales = lineRenderer.positionCount;
        
        // Si es el primer punto, lo añadimos directo
        if (puntosActuales == 0)
        {
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, nuevaPosicion);
            return;
        }

        // Si ya hay puntos, revisamos que nos hayamos movido lo suficiente para no encimar tinta
        Vector3 ultimoPunto = lineRenderer.GetPosition(puntosActuales - 1);
        if (Vector3.Distance(ultimoPunto, nuevaPosicion) > distanciaMinimaPunto)
        {
            lineRenderer.positionCount = puntosActuales + 1;
            lineRenderer.SetPosition(puntosActuales, nuevaPosicion);
        }
    }

    public void LimpiarPincel()
    {
        lineRenderer.positionCount = 0;
    }

    // CAMBIAR OnApplicationQuit POR OnDestroy
    void OnDestroy()
    {
        if (client != null) client.Close();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();
    }
}