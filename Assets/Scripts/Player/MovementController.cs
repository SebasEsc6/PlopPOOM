using System.Collections;
using UnityEngine;

public class MovementController : MonoBehaviour, IPoolable
{
    public float speedMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private float rayDistance;
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private GameObject jumpParticles;
    [SerializeField] private LayerMask groundLayer; // This should only include Default layer
    [SerializeField] private float speedIncreaseAmount;

    [Header("Jump Anti-Spam")]
    [SerializeField] private float jumpCooldown = 0.15f;   // min time between jumps
    [SerializeField] private float coyoteTime = 0.08f;   // optional: grace after leaving ground
    [SerializeField] private Transform groundCheck;      // un empty en los pies
    [SerializeField] private float groundCheckRadius = .12f;
    private float _lastJumpTime = -999f;
    private float _lastGroundedTime = -999f;
    private bool _doubleJumpAvailable;

    public bool canJump;
    public float moveDirection;
    public float currentSpeed;
    public float timeSpeedUp;

    private Rigidbody2D rb;
    private Animator _animator;
    private bool isSpeedUpActive = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = gameObject.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        MoveHandler();
        ValidationJump();

        if (canJump) _lastGroundedTime = Time.time;
    }

    public void OnSpawnedFromPool()
    {
        _doubleJumpAvailable = false;
        _lastJumpTime = -999f;
        _lastGroundedTime = -999f;

        moveDirection = 0f;
        if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
    }

    public void OnDespawnedToPool()
    {
        moveDirection = 0f;
        if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
    }

    private void MoveHandler()
    {
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);

        if (moveDirection < 0)
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
        }
        else if (moveDirection > 0)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        }
        _animator.SetFloat("MoveSpeed", moveDirection);
    }

    public void SwitchVelocity(bool isSlow)
    {
        if (!isSlow)
        {
            currentSpeed = speedMovement;
        }
        else
        {
            currentSpeed = speedMovement / 2;
        }
        
        if (isSpeedUpActive)
        {
            StartCoroutine(IncreaseSpeed());
        }
    }

    public void Jump()
    {
        // Block if we are still in cooldown
        if (Time.time < _lastJumpTime + jumpCooldown) return;

        // Ground or coyote window?
        bool canGroundJump = canJump || (Time.time - _lastGroundedTime <= coyoteTime);

        if (canGroundJump)
        {
            // Ground jump
            _doubleJumpAvailable = true; // allow one air jump after a grounded jump
            DoJump();
            return;
        }

        if (_doubleJumpAvailable)
        {
            _doubleJumpAvailable = false;
            DoJump();
            return;
        }
    }

    private void DoJump()
    {
        // Perform the actual jump and start cooldown.
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _lastJumpTime = Time.time;
        _animator.SetTrigger("Jump");
        StartCoroutine(TurnParticles());
    }

    IEnumerator TurnParticles()
    {
        jumpParticles.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        jumpParticles.SetActive(false);
    }

    public IEnumerator IncreaseSpeed()
    {
        currentSpeed = speedMovement * speedIncreaseAmount;
        yield return new WaitForSeconds(timeSpeedUp);
        isSpeedUpActive = false;
        currentSpeed = speedMovement;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpeedUp"))
        {
            isSpeedUpActive = true;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Ammo"))
        {
            StatsController stats = GetComponent<StatsController>();
            if (stats != null)
            {
                stats.Reload();
                Destroy(other.gameObject);
            }
        }
        
        if (other.CompareTag("Fall"))   
        {
            StatsController stats = GetComponent<StatsController>();
            if (stats != null)
            {
                stats.DieOnce();
            }
        }
    }

    private void ValidationJump()
    {
        bool touchingGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool movingUp = rb.linearVelocity.y > 0.01f;
        canJump = touchingGround && !movingUp;
        if (canJump) _lastGroundedTime = Time.time;

        Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckRadius, canJump ? Color.green : Color.red);
    }
}
