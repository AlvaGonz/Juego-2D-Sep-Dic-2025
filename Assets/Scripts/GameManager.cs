using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("═══ REFERENCIAS UI ═══")]
    [SerializeField] private Text textoPuntos;
    [SerializeField] private Text textoNivel;
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private bool debug = true;
    
    private int puntosActuales = 0;
    private int puntosAcumulados = 0;
    private int nivelActual = 1;

    void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Persistir entre escenas (opcional)
        // DontDestroyOnLoad(gameObject);
        
        if (debug) Debug.Log("✓ GameManager inicializado");
    }

    void Start()
    {
        ActualizarNivel();
        ActualizarUI();
        
        // Cargar puntos previos si existen
        if (PlayerPrefs.HasKey("PuntosAcumulados"))
        {
            puntosAcumulados = PlayerPrefs.GetInt("PuntosAcumulados");
        }
    }

    public void AgregarPuntos(int cantidad)
    {
        puntosActuales += cantidad;
        puntosAcumulados += cantidad;
        
        if (debug) Debug.Log($"⭐ Puntos: {puntosActuales} (Total: {puntosAcumulados})");
        
        ActualizarUI();
    }

    public void GuardarPuntos()
    {
        PlayerPrefs.SetInt("PuntosAcumulados", puntosAcumulados);
        PlayerPrefs.Save();
        
        if (debug) Debug.Log($"💾 Puntos guardados: {puntosAcumulados}");
    }

    private void ActualizarNivel()
    {
        nivelActual = SceneManager.GetActiveScene().buildIndex;
    }

    private void ActualizarUI()
    {
        if (textoPuntos != null)
        {
            textoPuntos.text = $"Puntos: {puntosActuales}";
        }
        
        if (textoNivel != null)
        {
            int totalNiveles = SceneManager.sceneCountInBuildSettings - 1; // -1 para excluir menú
            textoNivel.text = $"Nivel {nivelActual}/{totalNiveles}";
        }
    }

    public int ObtenerPuntosActuales()
    {
        return puntosActuales;
    }

    public int ObtenerPuntosAcumulados()
    {
        return puntosAcumulados;
    }
}
