using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [field: SerializeField] public ScriptableStats _stats { get; private set; }
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;


        [SerializeField] private PlayerInput playerInput;
        private Vector2 _moveAction;
        private InputAction _jumpAction;
        
        #region Interface
        
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameVelocity => _frameVelocity;
        public Vector2 FrameInput => _moveAction;

        #endregion

        private float _time;

        private float _originX = 1f;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
            // 점프 입력 할당
            _jumpAction = playerInput.actions["Jump"];
        }

        private void OnEnable()
        {
            _jumpAction.started += JumpInput;
       
        }

        private void OnDisable()
        {
            _jumpAction.started -= JumpInput;
        }
        
        private void Update()
        {
            _time += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            HandleJump();
            HandleDirection();
            HandleGravity();
            // 실제 이동 처리
            ApplyMovement();
        }

        public void OnMove(InputValue value)
        {
            _moveAction = value.Get<Vector2>();
            
            if (_stats.SnapInput)
            {
                _moveAction.x = Mathf.Abs(_moveAction.x) < _stats.HorizontalDeadZoneThreshold
                    ? 0
                    : Mathf.Sign(_moveAction.x);
                _moveAction.y = Mathf.Abs(_moveAction.y) < _stats.VerticalDeadZoneThreshold
                    ? 0
                    : Mathf.Sign(_moveAction.y);
            }
        }

        #region Collisions

        // 체공 시작 시간 -> 캐릭터의 발이 땅에서 떨어진 시점의 시간
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling 땅인지 검사
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down,
                _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up,
                _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // 땅에 닿아있을 때
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }// 공중에 있을 때
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        // 점프 대기 -> 점프 키 입력 시 true
        private bool _jumpToConsume;
        // 선입력 -> 땅에 닿으면 true 점프 실행 후 즉시 false
        private bool _bufferedJumpUsable;
       
        // 코요테 시간 허용 -> 땅에 닿으면 true,코요테 타임으로 점프를 한 번 실행하고 나면 false
        private bool _coyoteUsable;
        // 점프 키를 누른 시점의 시간.
        private float _timeJumpWasPressed;
        
        
        // 선입력 유효성 검사
        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        // 코요테 유효성 검사
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void JumpInput(InputAction.CallbackContext context)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
            Debug.Log(_grounded + "  grounded: ");
        }
        
        private void HandleJump()
        {
            if (!_jumpToConsume && !HasBufferedJump) return;

            // 점프 했으니까 false
            _jumpToConsume = false;
            if (_grounded || CanUseCoyote)
            {
                ExecuteJump();
            }
          
        }

        private void ExecuteJump()
        {
            _frameVelocity.y = _stats.JumpPower;
            
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        // 이동
        private void HandleDirection()
        {
            if (_moveAction.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _moveAction.x * _stats.MaxSpeed,
                    _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            // 땅에 있다
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else // 점프중
            {
                // 공기 저항                // 하락 가속도
                var inAirGravity = _stats.FallAcceleration;
                // 하락 속도 보정
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed,
                    inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void ApplyMovement()
        {
            _rb.linearVelocity = _frameVelocity;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null)
                Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

 

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;

        public Vector2 FrameInput { get; }
        public Vector2 FrameVelocity { get; }
        public ScriptableStats _stats { get; }
    }
}