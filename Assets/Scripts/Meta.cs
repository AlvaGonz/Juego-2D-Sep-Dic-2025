using UnityEngine;
using UnityEngine.SceneManagement;

public class Meta : MonoBehaviour
{
    [Header("═══ ESCENA SIGUIENTE ═══")]
    [SerializeField] private int indiceProximaEscena = -1; // -1 = siguiente automática
    
    [Header("═══ AUDIO Y EFECTOS ═══")]
    [SerializeField] private AudioClip sonidoVictoria;
    [SerializeField] private ParticleSystem particulasVictoria;
    
    [Header("═══ TRANSICIÓN ═══")]
    [SerializeField] private float tiempoTransicion = 1.5f;
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private bool debug = true;
    
    private bool yaActivada = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (yaActivada) return;
        
        if (collision.CompareTag("Jugador"))
        {
            yaActivada = true;
            
            if (debug) Debug.Log("🎉 ¡NIVEL COMPLETADO!");
            
            // Reproducir sonido
            if (sonidoVictoria != null)
            {
                AudioSource.PlayClipAtPoint(sonidoVictoria, transform.position);
            }
            
            // Crear partículas
            if (particulasVictoria != null)
            {
                Instantiate(particulasVictoria, transform.position, Quaternion.identity);
            }
            
            // Guardar puntos antes de cambiar escena
            if (GameManager.instance != null)
            {
                GameManager.instance.GuardarPuntos();
            }
            
            // Cargar siguiente nivel
            Invoke("CargarSiguientNivel", tiempoTransicion);
        }
    }

    private void CargarSiguientNivel()
    {
        if (indiceProximaEscena == -1)
        {
            // Cargar siguiente por índice
            int proximoIndice = SceneManager.GetActiveScene().buildIndex + 1;
            
            if (proximoIndice < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(proximoIndice);
            }
            else
            {
                if (debug) Debug.Log("✅ ¡JUEGO COMPLETADO!");
                SceneManager.LoadScene(0); // Volver al menú
            }
        }
        else
        {
            SceneManager.LoadScene(indiceProximaEscena);
        }
    }
}
