using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speedMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private float rayDistance;
    [SerializeField] private bool canDoubleJump;
    public bool canJump;
    public float moveDirection;
    private float currentSpeed;
    private Animator _animator;


    [SerializeField] private LayerMask groundLayer; // This should only include Default layer

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = gameObject.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        MoveHandler();
        ValidationJump();
    }

    private void MoveHandler()
    {
        rb.velocity = new Vector2(moveDirection * currentSpeed, rb.velocity.y);
        if(moveDirection < 0)
        {
            transform.localScale = new Vector3(-.7f, transform.localScale.y, transform.localScale.z);
        }
        else if (moveDirection > 0)
        {
            transform.localScale = new Vector3(.7f, transform.localScale.y, transform.localScale.z);
        }
        _animator.SetFloat("Speed", moveDirection);
    }

    public void SwitchVelocity(bool isSlow)
    {
        if (!isSlow)
        {
            currentSpeed = speedMovement;
        }
        else
        {
            currentSpeed = speedMovement/2;
        }
    }

    public void Jump()
    {
        // Check if we can jump or double jump
        if (canJump)
        {
            canDoubleJump = true;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _animator.SetTrigger("Jump");
        }
        else if (!canJump && canDoubleJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canDoubleJump = false;
            _animator.SetTrigger("Jump");
        }
    }

    private void ValidationJump()
    {
        // Define the ray origin
        Vector3 rayOrigin = transform.position; // Try not offsetting first
        Vector2 direction = Vector2.down;
        
        // Debug ray to see in Scene view
        Debug.DrawRay(rayOrigin, direction * rayDistance, Color.red);

        // Only detect objects in groundLayer
        RaycastHit2D hitInfo = Physics2D.Raycast(rayOrigin, direction, rayDistance, groundLayer);

        // If the ray hits something in that layer
        canJump = (hitInfo.collider != null);
    }
}
