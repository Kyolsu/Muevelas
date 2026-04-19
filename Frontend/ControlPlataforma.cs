using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ControlPlataforma : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int puerto = 5052;

    // Variables para guardar la coordenada más reciente
    private float targetX = 0f;
    private float targetY = 0f;

    // Qué tan lejos se puede mover la plataforma en Unity
    public float multiplicadorRangoX = 15f; 
    public float multiplicadorRangoY = 10f; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Iniciamos el receptor en segundo plano
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
                byte[] data = client.Receive(ref anyIP); // Aquí se queda esperando datos
                string text = Encoding.UTF8.GetString(data);

                // Python nos manda "0.5,0.5". Lo partimos en dos:
                string[] splitData = text.Split(',');
                
                if (splitData.Length == 2)
                {
                    // Convertimos el texto a decimales 
                    float parsedX = float.Parse(splitData[0], System.Globalization.CultureInfo.InvariantCulture);
                    float parsedY = float.Parse(splitData[1], System.Globalization.CultureInfo.InvariantCulture);

                    // Mapeamos los valores de Python (0 a 1) al espacio de Unity (-1 a 1)
                    // Además invertimos el eje Y, porque en cámaras abajo es Y+, pero en Unity abajo es Y-
                    targetX = (parsedX - 0.5f) * multiplicadorRangoX;
                    targetY = -(parsedY - 0.5f) * multiplicadorRangoY; 
                }
            }
            catch 
            {
                // Si cerramos el juego, el receptor da error y se apaga suavemente
                break; 
            }
        }
    }

    void FixedUpdate()
    {
        // Movemos físicamente la plataforma hacia el objetivo de forma suave (Lerp)
        Vector2 targetPosition = new Vector2(targetX, targetY);
        rb.MovePosition(Vector2.Lerp(rb.position, targetPosition, 0.2f));
    }

    // Limpieza al cerrar Unity
    void OnApplicationQuit()
    {
        if (client != null) client.Close();
        if (receiveThread != null) receiveThread.Abort();
    }
}