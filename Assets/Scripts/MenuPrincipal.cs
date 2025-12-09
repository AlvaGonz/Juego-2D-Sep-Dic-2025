using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    [Header("═══ BOTONES ═══")]
    public Button btnJugar;
    public Button btnInstrucciones;
    public Button btnSalir;

    [Header("═══ PANELES ═══")]
    public GameObject panelMenu;
    public GameObject panelInstrucciones;

    [Header("═══ TEXTOS ═══")]
    public Text textoInstrucciones;
    public Button btnVolverInstrucciones;

    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private bool debug = true;

    void Start()
    {
        Time.timeScale = 1f;
        ConfigurarBotones();
        InicializarPaneles();

        if (debug) Debug.Log("✓ Menú Principal inicializado correctamente");
    }

    private void ConfigurarBotones()
    {
        if (btnJugar == null || btnInstrucciones == null || btnSalir == null)
        {
            Debug.LogError("❌ ERROR: Botones no asignados en el Inspector");
            return;
        }

        btnJugar.onClick.AddListener(ComenzarJuego);
        btnInstrucciones.onClick.AddListener(MostrarInstrucciones);
        btnSalir.onClick.AddListener(SalirJuego);
        btnVolverInstrucciones.onClick.AddListener(VolverAlMenu);
    }

    private void InicializarPaneles()
    {
        if (panelMenu != null)
            panelMenu.SetActive(true);

        if (panelInstrucciones != null)
            panelInstrucciones.SetActive(false);
    }

    public void ComenzarJuego()
    {
        int escenaActual = SceneManager.GetActiveScene().buildIndex;
        int proximaEscena = escenaActual + 1;

        if (proximaEscena < SceneManager.sceneCountInBuildSettings)
        {
            if (debug) Debug.Log($"▶️ Cargando escena índice: {proximaEscena}");
            SceneManager.LoadScene(proximaEscena);
        }
        else
        {
            Debug.LogError($"❌ ERROR: No existe escena en índice {proximaEscena} en Build Settings");
        }
    }

    public void MostrarInstrucciones()
    {
        if (debug) Debug.Log("📖 Abriendo instrucciones...");
        panelMenu.SetActive(false);
        panelInstrucciones.SetActive(true);
    }

    public void VolverAlMenu()
    {
        if (debug) Debug.Log("🔙 Volviendo al menú...");
        panelInstrucciones.SetActive(false);
        panelMenu.SetActive(true);
    }

    public void SalirJuego()
    {
        if (debug) Debug.Log("❌ Saliendo del juego...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
