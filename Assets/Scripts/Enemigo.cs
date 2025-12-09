using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("═══ MOVIMIENTO ═══")]
    [SerializeField] private float velocidadPatrulla = 2f;
    [SerializeField] private float distanciaPatrulla = 5f;
    [SerializeField] private float alturaDeteccionSalto = 0.5f;
    
    [Header("═══ LÍMITES DE MOVIMIENTO ═══")]
    [SerializeField] private float limiteIzquierdo = -10f;
    [SerializeField] private float limiteDerecho = 10f;
    
    [Header("═══ REBOTE AL SALTAR ═══")]
    [SerializeField] private float fuerzaRebote = 8f; // ✅ REBOTE MÁS FUERTE
    [SerializeField] private float velocidadReboteHorizontal = 5f; // Empuje horizontal al rebotar
    
    [Header("═══ AUDIO ═══")]
    [SerializeField] private AudioClip sonidoMuerte;
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private bool debug = true;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    private Vector3 posicionInicial;
    private int direccion = 1;
    private bool estaMuerto = false;
    private float distanciaRecorrida = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        posicionInicial = transform.position;
        
        if (debug) Debug.Log("👹 Enemigo creado en patrulla");
    }

    void FixedUpdate()
    {
        if (estaMuerto) return;
        
        Patrullar();
    }

    private void Patrullar()
    {
        // Movimiento
        rb.linearVelocity = new Vector2(velocidadPatrulla * direccion, rb.linearVelocity.y);
        
        distanciaRecorrida = Mathf.Abs(transform.position.x - posicionInicial.x);
        
        // ✅ VERIFICAR LÍMITES DUROS
        if (transform.position.x <= limiteIzquierdo || transform.position.x >= limiteDerecho)
        {
            CambiarDireccion();
        }
        // Cambiar dirección al llegar al límite de patrulla
        else if (distanciaRecorrida >= distanciaPatrulla)
        {
            CambiarDireccion();
        }
    }

    private void CambiarDireccion()
    {
        direccion *= -1;
        spriteRenderer.flipX = (direccion == -1);
        
        if (debug) Debug.Log($"🔄 Enemigo cambió dirección. Ahora: {(direccion == 1 ? "→" : "←")}");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaMuerto) return;
        
        if (collision.gameObject.CompareTag("Jugador"))
        {
            ControlesJugador controlJugador = collision.gameObject.GetComponent<ControlesJugador>();
            if (controlJugador == null) return;
            
            // Verificar si el jugador saltó en la cabeza
            float alturaCabeza = transform.position.y + alturaDeteccionSalto;
            float posicionColisionY = collision.relativeVelocity.y;
            
            if (collision.transform.position.y > alturaCabeza && posicionColisionY < -0.5f)
            {
                if (debug) Debug.Log("☠️ ¡Enemigo saltado en la cabeza!");
                
                // ✅ REBOTE MEJORADO
                Rigidbody2D rbJugador = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rbJugador != null)
                {
                    // Resetear velocidad
                    rbJugador.linearVelocity = Vector2.zero;
                    
                    // Rebote vertical fuerte
                    rbJugador.linearVelocity += Vector2.up * fuerzaRebote;
                    
                    // Empuje horizontal en la dirección que mira el jugador
                    int direccionJugador = controlJugador.GetDireccion();
                    rbJugador.linearVelocity += new Vector2(velocidadReboteHorizontal * direccionJugador, 0);
                    
                    if (debug) Debug.Log($"⬆️ Rebote: Fuerza={fuerzaRebote}, Dirección={direccionJugador}");
                }
                
                // Matar al enemigo
                MatarEnemigo();
                
                // Agregar puntos
                GameManager.instance.AgregarPuntos(50);
            }
            else
            {
                if (debug) Debug.Log("💥 ¡Tocado por enemigo!");
                controlJugador.Morir();
            }
        }
    }

    public void MatarEnemigo()
    {
        estaMuerto = true;
        
        // Reproducir sonido
        if (sonidoMuerte != null)
        {
            AudioSource.PlayClipAtPoint(sonidoMuerte, transform.position);
        }
        
        // Animar muerte
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
        
        // Destruir después de animación
        Destroy(gameObject, 0.5f);
    }
}
