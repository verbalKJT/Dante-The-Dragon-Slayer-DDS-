using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// VERY primitive animator example.
    /// 애니메이션을 바꾸거나 파티클 시스템을 보여주는 용도
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Animator _anim;
        
        [Header("Settings")] [SerializeField, Range(1f, 3f)]
        private float _maxIdleSpeed = 2;

        [SerializeField] private float _maxTilt = 5;
        [SerializeField] private float _tiltSpeed = 20;

        [Header("Particles")] [SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;

        [Header("Audio Clips")] [SerializeField]
        private AudioClip[] _footsteps;

        private AudioSource _source;
        private IPlayerController _player;
        private bool _grounded;
        private ParticleSystem.MinMaxGradient _currentGradient;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _player = GetComponentInParent<IPlayerController>();
        }

        private void OnEnable()
        {
            _player.Jumped += OnJumped;
            _player.GroundedChanged += OnGroundedChanged; 

            //_moveParticles.Play();
        }

        private void OnDisable()
        {
            _player.Jumped -= OnJumped;
            _player.GroundedChanged -= OnGroundedChanged;
           // _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null) return;

            DetectGroundColor();

            HandleSpriteFlip();

            HandleIdleSpeed();

            HandleCharacterTilt();
        }

        private void HandleSpriteFlip()
        {
            if (_player.FrameInput.x == 0f) return;
            float facingDirection = _player.FrameInput.x > 0f ? 1f : -1f;
                
            float newScaleX = Mathf.Abs(transform.localScale.x) * facingDirection;
    
            transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
        }

        // 이동 애니메이션 및 파티클
        private void HandleIdleSpeed()
        {
            float currentSpeed = Mathf.Abs(_player.FrameVelocity.x);
    
            // 현재 속도를 최고 속도로 나누어 0.0 ~ 1.0 사이의 부드러운 소수점 비율로 만듭니다.
            float speedPercentage = currentSpeed / _player._stats.MaxSpeed;
    
            // 이미 물리 엔진이 부드럽게 가감속을 처리했으므로, dampTime(0.1f) 없이 직접 넘겨도 매우 부드럽습니다.
            _anim.SetFloat(MoveAnimKey, speedPercentage);
            //_moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
        }

        private void HandleCharacterTilt()
        {
            var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) : Quaternion.identity;
            _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, runningTilt * Vector2.up, _tiltSpeed * Time.deltaTime, 0f);
        }

        private void OnJumped()
        {
            _anim.SetTrigger(JumpKey);
           // _anim.ResetTrigger(GroundedKey);
            
            if (_grounded) // Avoid coyote
            {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                _jumpParticles.Play();
            }
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;
            if (grounded)
            {
                DetectGroundColor();
                SetColor(_landParticles);
                //_source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
                //_moveParticles.Play();

                //_landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
                //_landParticles.Play();
            }
            else
            {
                //_moveParticles.Stop();
            }
        }
        private void DetectGroundColor()
        {
            var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

            if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
            var color = r.color;
            _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
            SetColor(_moveParticles);
        }

        private void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        private static readonly int MoveAnimKey = Animator.StringToHash("IsMoving");
        private static readonly int JumpKey = Animator.StringToHash("FirstJump");
    }
}