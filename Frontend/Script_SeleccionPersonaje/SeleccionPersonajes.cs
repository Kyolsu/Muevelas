using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SeleccionPersonajes : MonoBehaviour
{
    [Header("UI Jugador 1")]
    public Image imgJugador1;
    public TextMeshProUGUI txtNombreJugador1;
    private int indiceP1 = 0;

    [Header("UI Jugador 2")]
    public Image imgJugador2;
    public TextMeshProUGUI txtNombreJugador2;
    private int indiceP2 = 1;

    void Start()
    {
        ActualizarUI();
    }

    public void CambiarPersonajeP1(int direccion)
    {
        indiceP1 = ObtenerSiguienteIndiceValido(indiceP1, indiceP2, direccion);
        ActualizarUI();
    }

    public void CambiarPersonajeP2(int direccion)
    {
        indiceP2 = ObtenerSiguienteIndiceValido(indiceP2, indiceP1, direccion);
        ActualizarUI();
    }

    private int ObtenerSiguienteIndiceValido(int indiceActual, int indiceOtro, int direccion)
    {
        // Leemos el tamaño de la lista directamente del cerebro
        int cantidad = DatosPersonaje.Instancia.baseDeDatosGlobal.Length;
        int nuevoIndice = indiceActual;
        do
        {
            nuevoIndice = (nuevoIndice + direccion + cantidad) % cantidad;
        } while (nuevoIndice == indiceOtro);

        return nuevoIndice;
    }

    private void ActualizarUI()
    {
        // Sacamos las fotos y nombres desde el cerebro global
        imgJugador1.sprite = DatosPersonaje.Instancia.baseDeDatosGlobal[indiceP1].imagen;
        txtNombreJugador1.text = DatosPersonaje.Instancia.baseDeDatosGlobal[indiceP1].nombre;

        imgJugador2.sprite = DatosPersonaje.Instancia.baseDeDatosGlobal[indiceP2].imagen;
        txtNombreJugador2.text = DatosPersonaje.Instancia.baseDeDatosGlobal[indiceP2].nombre;
    }

    public void BotonJugar()
    {
        // Guardamos las elecciones directo en la memoria RAM del cerebro
        DatosPersonaje.Instancia.idP1 = indiceP1;
        DatosPersonaje.Instancia.idP2 = indiceP2;

        SceneManager.LoadScene("Nivel1"); // O tu escena intermedia
    }
}