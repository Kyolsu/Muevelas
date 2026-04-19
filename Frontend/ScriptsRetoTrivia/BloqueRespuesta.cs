using UnityEngine;
using TMPro;

public class BloqueRespuesta : MonoBehaviour
{
    public TextMeshProUGUI textoUI;
    private bool esCorrecta;
    private ManagerTrivia manager;

    public void Configurar(string texto, bool correcta, ManagerTrivia m)
    {
        textoUI.text = texto;
        esCorrecta = correcta;
        manager = m;
    }

    public void ComprobarRespuesta()
    {
        manager.RespuestaSeleccionada(esCorrecta);
    }
}