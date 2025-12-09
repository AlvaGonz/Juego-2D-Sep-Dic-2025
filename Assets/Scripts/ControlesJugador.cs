using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ControlesJugador : MonoBehaviour
{
    [Header("═══ MOVIMIENTO ═══")]
    [SerializeField] private float velocidadMovimiento = 8f;
    [SerializeField] private float fuerzaSalto = 6f;
    [SerializeField] private float velocidadMaximaHorizontal = 8f;
    
    [Header("═══ FÍSICA ═══")]
    [SerializeField] private float gravedad = 9.81f;
    [SerializeField] private float radioDeteccionSuelo = 0.3f;
    [SerializeField] private LayerMask capaSuelo;
    
    [Header("═══ REFERENCIA AL SUELO ═══")]
    [SerializeField] private Transform puntoDeteccionSuelo;
    
    [Header("═══ AUDIO ═══")]
    [SerializeField] private AudioClip sonidoSalto;
    [SerializeField] private AudioClip sonidoMuerte;
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private bool debug = true;
    
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    
    // INPUT SYSTEM
    private InputAction actionMovimiento;
    private InputAction actionSalto;
    
    private float velocidadActual = 0f;
    private bool enSuelo = false;
    private bool puedesSaltar = true;
    private bool estasMuerto = false;
    private int direccionActual = 1; // 1 = derecha, -1 = izquierda

    void OnEnable()
    {
        // Crear acciones de input
        var keyboard = Keyboard.current;
        
        if (keyboard == null)
        {
            if (debug) Debug.LogError("❌ ERROR: No se detectó teclado");
            return;
        }
        
        // Acción de movimiento: A/D o Flechas izquierda/derecha
        actionMovimiento = new InputAction(type: InputActionType.Value);
        actionMovimiento.AddCompositeBinding("1DAxis")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow");
        actionMovimiento.Enable();
        
        // Acción de salto: Espacio
        actionSalto = new InputAction(type: InputActionType.Button);
        actionSalto.AddBinding("<Keyboard>/space");
        actionSalto.Enable();
        
        if (debug) Debug.Log("✓ Input System configurado");
    }

    void OnDisable()
    {
        actionMovimiento?.Disable();
        actionSalto?.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (puntoDeteccionSuelo == null)
        {
            Debug.LogError("❌ ERROR: Punto de detección de suelo no asignado");
        }
        
        if (debug) Debug.Log("✓ Controles del jugador inicializados");
    }

    void Update()
    {
        if (estasMuerto) return;
        
        // Detección de entrada (Input System)
        float inputHorizontal = actionMovimiento.ReadValue<float>();
        
        // ✅ FIX SALTO: Usar IsPressed()
        bool pulsandoSalto = actionSalto.IsPressed();
        
        // Movimiento
        Movimiento(inputHorizontal);
        
        // Salto SOLO si está en suelo y presiona espacio
        if (pulsandoSalto && puedesSaltar && enSuelo)
        {
            Saltar();
            puedesSaltar = false;
        }
        
        // Actualizar animaciones
        ActualizarAnimaciones(inputHorizontal);
    }

    void FixedUpdate()
    {
        if (estasMuerto) return;
        
        // ✅ DETECTAR SUELO ANTES DE TODO
        DetectarSuelo();
        
        // Aplicar gravedad adicional (para caída más rápida)
        if (!enSuelo && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.down * gravedad * 0.5f * Time.fixedDeltaTime;
        }
        
        // Limitar velocidad horizontal
        if (rb.linearVelocity.x > velocidadMaximaHorizontal)
        {
            rb.linearVelocity = new Vector2(velocidadMaximaHorizontal, rb.linearVelocity.y);
        }
        else if (rb.linearVelocity.x < -velocidadMaximaHorizontal)
        {
            rb.linearVelocity = new Vector2(-velocidadMaximaHorizontal, rb.linearVelocity.y);
        }
    }

    private void Movimiento(float input)
    {
        // Calcular velocidad horizontal deseada
        float velocidadDeseada = input * velocidadMovimiento;
        
        // Aplicar movimiento suave
        velocidadActual = Mathf.Lerp(velocidadActual, velocidadDeseada, 0.15f);
        
        // Aplicar al Rigidbody
        rb.linearVelocity = new Vector2(velocidadActual, rb.linearVelocity.y);
        
        // Flip del sprite
        if (input > 0 && direccionActual == -1)
        {
            direccionActual = 1;
            spriteRenderer.flipX = false;
        }
        else if (input < 0 && direccionActual == 1)
        {
            direccionActual = -1;
            spriteRenderer.flipX = true;
        }
    }

    private void Saltar()
    {
        if (debug) Debug.Log("⬆️ ¡Salto ejecutado!");
        
        // Aplicar fuerza de salto
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity += Vector2.up * fuerzaSalto;
        
        // Reproducir sonido
        if (sonidoSalto != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoSalto);
        }
    }

    private void DetectarSuelo()
    {
        if (puntoDeteccionSuelo == null) return;
        
        // ✅ VERIFICAR VISUALMENTE EN EDITOR
        // Raycast hacia abajo desde el punto de detección
        Collider2D[] colisiones = Physics2D.OverlapCircleAll(
            puntoDeteccionSuelo.position,
            radioDeteccionSuelo,
            capaSuelo
        );
        
        bool estabaSuelo = enSuelo;
        enSuelo = colisiones.Length > 0;
        
        // Debug visual
        if (debug && enSuelo != estabaSuelo)
        {
            Debug.Log(enSuelo ? "✅ En suelo" : "⬆️ En aire");
        }
        
        // Permitir salto nuevamente cuando toca el suelo
        if (enSuelo)
        {
            puedesSaltar = true;
        }
    }

    private void ActualizarAnimaciones(float input)
    {
        if (animator == null) return;
        
        // Parámetro Speed
        float speedAbsoluta = Mathf.Abs(input);
        animator.SetFloat("speed", speedAbsoluta);
        
        // Parámetro isGrounded
        animator.SetBool("isGrounded", enSuelo);
        
        // Parámetro isFalling
        bool esCayendo = !enSuelo && rb.linearVelocity.y < -0.5f;
        animator.SetBool("isFalling", esCayendo);
        
        // Parámetro isJumping
        if (rb.linearVelocity.y > 0.5f && !enSuelo)
        {
            animator.SetBool("isJumping", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
        }
    }

    public void Morir()
    {
        if (estasMuerto) return;
        
        estasMuerto = true;
        
        if (debug) Debug.Log("💀 ¡MUERTE!");
        
        // Reproducir sonido de muerte
        if (sonidoMuerte != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
        }
        
        // Reproducir animación de muerte
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
        
        // Reiniciar nivel después de 1 segundo
        Invoke("ReiniciarNivel", 1f);
    }

    public void ReiniciarNivel()
    {
        if (debug) Debug.Log("🔄 Reiniciando nivel...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Getters para la cámara
    public int GetDireccion()
    {
        return direccionActual;
    }

    public bool GetEnSuelo()
    {
        return enSuelo;
    }

    public Vector2 GetVelocidad()
    {
        return rb.linearVelocity;
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar zona de detección de suelo en editor
        if (puntoDeteccionSuelo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(puntoDeteccionSuelo.position, radioDeteccionSuelo);
        }
    }
}
