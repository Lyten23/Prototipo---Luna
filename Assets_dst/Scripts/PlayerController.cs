using System;
using System.Collections;
using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Header("Movement Player")]
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        [Header("Dash")] 
        private bool _canDash;
        private bool _isDashing;
        [SerializeField] private SpriteRenderer sprite;
        [Header("WallSlide/WallJump")] 
        private bool _isWallSliding;
        private float _wallSlidingSpeed = 2f;
        //Wall Jump 
        public float wallJumDuration;
        public Vector2 wallJumpForce;
        private bool wallJumping;
        #region Interface

        public bool IsDashing => _isDashing;
        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Dashing; 
        public event Action Jumped;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            if (_isDashing) return;
            GatherInput();
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump"),
                JumpHeld = Input.GetButton("Jump"),
                Dashing = Input.GetButtonDown("Dash"),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
                
            };
            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }
            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
            if (_frameInput.JumpDown && wallJumping)
            {
                
            }
            if (_frameInput.Dashing)
            {
                StartCoroutine(Dash());
                Dashing?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            if (_isDashing) return;
            CheckCollisions();
            HandleJump();
            HandleDirection();
            HandleGravity();
            ApplyMovement();
        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;
            var vector2 = sprite.flipX ? Vector2.left : Vector2.right;
            // Detectamos la colisión con el suelo, techo y paredes
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool walledHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,vector2, _stats.GrounderDistance, _stats.Wall);
            // Chocamos con el Techo
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
            // Chocamos con una pared (Slide)
            else
            {
                _isWallSliding = false;
            }
            // Caemos al suelo
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Dejamos el suelo
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }
            if (!_grounded && walledHit)
            {
                _isWallSliding = true;
                _frameVelocity.y=Mathf.Clamp(_rb.velocity.y, -_wallSlidingSpeed, float.MaxValue);
            }else if (_isWallSliding)
            {
                wallJumping = true;
                Invoke("StopWallJumping",wallJumDuration);
            }
            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion
        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion
        #region WallJump

        void StopWallJumping()
        {
            wallJumping = false;
        }
        #endregion
        #region Dash

        private IEnumerator Dash()
        {
            _canDash = false;
            _isDashing = true;
            float originalGravity = _rb.gravityScale;
            _rb.gravityScale = 0f;
            if (sprite.flipX)
            {
                _rb.velocity = new Vector2(-sprite.transform.localScale.x * _stats.dashingPower, 0f);
            }
            else
            {
                _rb.velocity = new Vector2(sprite.transform.localScale.x * _stats.dashingPower, 0f);
            }
            yield return new WaitForSeconds(_stats.dashingTime);
            _rb.gravityScale = originalGravity;
            _isDashing = false;
            yield return new WaitForSeconds(_stats.dashingCooldown);
            _canDash = true;
        }
        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                float inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool Dashing;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Dashing;
        public event Action Jumped;
        public bool IsDashing { get; }
        public Vector2 FrameInput { get; }
    }
}