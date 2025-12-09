using UnityEngine;

public class Recolectable : MonoBehaviour
{
    [Header("═══ TIPO DE MONEDA ═══")]
    [SerializeField] private TipoMoneda tipoMoneda = TipoMoneda.Bronce;
    
    [Header("═══ AUDIO ═══")]
    [SerializeField] private AudioClip sonidoRecoleccion;
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private bool debug = true;
    
    public enum TipoMoneda
    {
        Bronce = 50,    // Común
        Plata = 150,    // Rara
        Oro = 500       // Muy Rara
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el que colisiona es el jugador
        if (collision.CompareTag("Jugador"))
        {
            int puntosOtorgados = (int)tipoMoneda;
            
            if (debug) Debug.Log($"✨ Recolectada moneda {tipoMoneda}: +{puntosOtorgados} puntos");
            
            // Notificar al gestor de puntuación
            GameManager.instance.AgregarPuntos(puntosOtorgados);
            
            // Reproducir sonido
            if (sonidoRecoleccion != null)
            {
                AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
            }
            
            // Destruir objeto
            Destroy(gameObject);
        }
    }
}
