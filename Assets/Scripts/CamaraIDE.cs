using UnityEngine;

/// <summary>
/// Cámara que sigue al jugador en HORIZONTAL y VERTICAL.
/// Anticipa la dirección hacia donde mira el jugador.
/// </summary>
public class CamaraIDE : MonoBehaviour
{
    [Header("═══ TARGET (JUGADOR) ═══")]
    [SerializeField] private Transform jugador;
    
    [Header("═══ OFFSET BASE ═══")]
    [SerializeField] private Vector3 offsetBase = new Vector3(0, 1.5f, -10);
    
    [Header("═══ ANTICIPACIÓN DIRECCIONAL ═══")]
    [SerializeField] private float anticipacionHorizontal = 2f; // Distancia que anticipa hacia la dirección
    
    [Header("═══ SUAVIZADO GENERAL ═══")]
    [SerializeField] private float velocidadSeguimientoX = 3f;
    [SerializeField] private float velocidadSeguimientoY = 2.5f;
    
    [Header("═══ RESPUESTA A SALTOS ═══")]
    [SerializeField] private float alturaExtraAlSaltar = 0.5f;
    [SerializeField] private float velocidadSuavizadoSalto = 0.1f;
    
    [Header("═══ RESPUESTA A CAÍDAS ═══")]
    [SerializeField] private float alturaExtraAlCaer = -0.3f;
    [SerializeField] private float velocidadSuavizadoCaida = 0.15f;
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private bool debug = false;
    
    private ControlesJugador controlJugador;
    private float alturaExtra = 0f;
    private bool estabaSaltando = false;

    void Start()
    {
        if (jugador == null)
        {
            jugador = FindObjectOfType<ControlesJugador>()?.transform;
            if (jugador == null)
            {
                Debug.LogError("❌ ERROR: No se encontró el jugador");
                enabled = false;
                return;
            }
        }
        
        controlJugador = jugador.GetComponent<ControlesJugador>();
        
        if (controlJugador == null)
        {
            Debug.LogError("❌ ERROR: El jugador no tiene script ControlesJugador");
            enabled = false;
            return;
        }
        
        // Posicionar cámara inicial
        transform.position = jugador.position + offsetBase;
        
        if (debug) Debug.Log("✓ Cámara inicializada - SIGUE DIRECCIÓN DEL JUGADOR");
    }

    void LateUpdate()
    {
        if (jugador == null || controlJugador == null) return;
        
        // Obtener información del jugador
        Vector2 velocidadJugador = controlJugador.GetVelocidad();
        int direccionJugador = controlJugador.GetDireccion();
        bool enSuelo = controlJugador.GetEnSuelo();
        
        // ═══ RESPUESTA A SALTOS ═══
        if (velocidadJugador.y > 0.5f && !enSuelo && !estabaSaltando)
        {
            estabaSaltando = true;
            alturaExtra = alturaExtraAlSaltar;
            
            if (debug) Debug.Log("📈 Cámara responde a SALTO");
        }
        
        // ═══ RESPUESTA A CAÍDAS ═══
        if (velocidadJugador.y < -1f && !enSuelo && estabaSaltando)
        {
            alturaExtra = Mathf.Lerp(alturaExtra, alturaExtraAlCaer, velocidadSuavizadoCaida);
            
            if (debug) Debug.Log("📉 Cámara responde a CAÍDA");
        }
        
        // ═══ RESETEAR CUANDO TOCA SUELO ═══
        if (enSuelo && estabaSaltando)
        {
            estabaSaltando = false;
            alturaExtra = Mathf.Lerp(alturaExtra, 0f, velocidadSuavizadoSalto);
        }
        
        // ═══ CALCULAR POSICIÓN OBJETIVO CON ANTICIPACIÓN ═══
        Vector3 posicionObjetivo = jugador.position + offsetBase;
        
        // ✅ ANTICIPAR EN LA DIRECCIÓN QUE MIRA EL JUGADOR
        posicionObjetivo.x += anticipacionHorizontal * direccionJugador;
        posicionObjetivo.y += alturaExtra;
        
        // ═══ SUAVIZADO EN X E Y ═══
        Vector3 nuevaPosicion = transform.position;
        
        // Seguimiento suave en X
        nuevaPosicion.x = Mathf.Lerp(transform.position.x, posicionObjetivo.x, velocidadSeguimientoX * Time.deltaTime);
        
        // Seguimiento suave en Y
        nuevaPosicion.y = Mathf.Lerp(transform.position.y, posicionObjetivo.y, velocidadSeguimientoY * Time.deltaTime);
        
        // Z siempre debe ser -10
        nuevaPosicion.z = -10f;
        
        transform.position = nuevaPosicion;
    }

    void OnDrawGizmos()
    {
        if (jugador == null) return;
        
        // Visualizar posición objetivo
        Vector3 posObjetivo = jugador.position + offsetBase;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(posObjetivo, Vector3.one * 0.3f);
    }
}
