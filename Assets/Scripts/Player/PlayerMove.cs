using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerMove : MonoBehaviour
{
    [Header("컴포넌트들")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInput playerInput;
    
    private float moveSpeed = 2f;
    
    private float horizontalInput;
    private InputAction moveAction;
    
    // 플레이어 원래 크기
    [SerializeField] private Vector3 originalScale;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    
    [SerializeField] private PlayableDirector playableDirector;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }
        
        if (playerInput != null)
        {
            moveAction = playerInput.actions.FindAction("Move");
        }
    }
    
    void FixedUpdate()
    {
        Move();
    }
    
    void Move()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
    private void OnMove(InputValue value) 
    {
        horizontalInput = value.Get<float>(); // 좌우 클릭 여부를 알게 되었으니
        UpdateAnimationState();
        UpdateDir();
    }

    public void OnJump()
    {
        
    }

    void UpdateDir()
    {
        if (horizontalInput > 0f) // 좌우 회전 로직
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }
        else if (horizontalInput < 0f)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
    }
    void UpdateAnimationState()
    {
        if (animator == null)
            return;

        animator.SetBool(IsMovingHash, Mathf.Abs(horizontalInput) > 0.01f);
    }
}
