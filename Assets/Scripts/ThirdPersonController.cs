using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using UnityEngine.UI;
#endif
using UnityEngine.AI;
namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 2.0f;
        private int _playerNavMeshArea;

        [Header("Noise / Sonido")]
        [SerializeField] private float sprintNoiseRadius = 12f;
        [SerializeField] private float sprintNoiseInterval = 0.4f;
        private float _noiseTimer = 0f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        public float health;
        
        [SerializeField] private Slider healthSlider;
        private NavMeshAgent _agent;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float stamina;
        [SerializeField] private float staminaDrainSprint = 15f;
        [SerializeField] private float staminaRecoverIdle = 25f;
        [SerializeField] private float staminaRecoverWalk = 10f;
        
        [Header("Health Regen")]
        [SerializeField] private float healthRegenDelay = 3f;
        [SerializeField] private float healthRegenRate = 5f; // vida por segundo

        private float _lastDamageTime;
        [Header("Crouch Settings")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private Vector3 standingCenter = new Vector3(0, 1f, 0);
        [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
        
        [Header("UI")]
        [SerializeField] private Slider staminaSlider;

        [Header("Flashlight")]
        [SerializeField] private Light flashlight;
        [SerializeField] private KeyCode flashlightKey = KeyCode.F;

        private bool flashlightOn = false;
        private bool _isCrouching = false;

        // Para evitar doble toggle con mando
        private bool _flashlightButtonHeld = false;
        private bool _crouchButtonHeld = false;

        public float SprintSpeed = 5.335f;

        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip Hurt;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDCrouch;
        private AudioSource _audioSource;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {    
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.updatePosition = false;
                _agent.updateRotation = false;
                _agent.Warp(transform.position);
            }
            _lastDamageTime = Time.time;
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            health = maxHealth;
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = health;
            }

            stamina = maxStamina;
            if (staminaSlider != null)
            {
                staminaSlider.maxValue = maxStamina;
                staminaSlider.value = stamina;
            }

            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#endif

            AssignAnimationIDs();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);
            HandleHealthRegen();
            HandleFlashlight();
            HandleCrouch();
            JumpAndGravity();
            GroundedCheck();
            Move();
            UpdateStaminaUI();
            UpdateHealthUI();
            HandleStamina();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDCrouch = Animator.StringToHash("Crouch");
        }

        // ── HEALTH ────────────────────────────────────────────────────────────

        public void Respawn(Vector3 position)
        {
            StartCoroutine(DoRespawn(position));
        }

        private System.Collections.IEnumerator DoRespawn(Vector3 position)
        {
            _controller.enabled = false;
            yield return null;
            transform.position = position;
            _controller.enabled = true;
            _verticalVelocity = 0f;
            health = maxHealth;
            stamina = maxStamina;
        }

        public void TakeDamage(float amount)
        {
            health = Mathf.Clamp(health - amount, 0f, maxHealth);
            _lastDamageTime = Time.time; // reinicia temporizador de regeneración
            _audioSource.PlayOneShot(Hurt);

            if (health <= 0f)
                OnDeath();
        }
        private void HandleHealthRegen()
        {
            if (health <= 0f || health >= maxHealth)
                return;

            if (Time.time - _lastDamageTime >= healthRegenDelay)
            {
                health += healthRegenRate * Time.deltaTime;
                health = Mathf.Clamp(health, 0f, maxHealth);
            }
        }
        public void Heal(float amount)
        {
            health = Mathf.Clamp(health + amount, 0f, maxHealth);
        }

        private void OnDeath()
        {
            GameManager.Instance.Death();
            EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
            foreach (EnemyAI enemy in enemies)
                enemy.ResetToStart();
        }

        private void UpdateHealthUI()
        {
            if (healthSlider != null)
                healthSlider.value = health;
        }

        // ── FLASHLIGHT ────────────────────────────────────────────────────────
        // Teclado: F  |  Mando: Y (Xbox) / Triángulo (PS) = JoystickButton3

        private void HandleFlashlight()
        {
            bool pressed = Input.GetKeyDown(flashlightKey)
                        || Input.GetKeyDown(KeyCode.JoystickButton3);

            if (pressed)
            {
                flashlightOn = !flashlightOn;
                if (flashlight != null)
                    flashlight.enabled = flashlightOn;
            }
        }

        // ── STAMINA ───────────────────────────────────────────────────────────

        private void HandleStamina()
        {
            bool isMoving = _input.move != Vector2.zero;
            bool isSprinting = _input.sprint && isMoving && !_isCrouching;

            if (isSprinting)
                stamina = Mathf.Clamp(stamina - staminaDrainSprint * Time.deltaTime, 0f, maxStamina);
            else if (!isMoving)
                stamina = Mathf.Clamp(stamina + staminaRecoverIdle * Time.deltaTime, 0f, maxStamina);
            else
                stamina = Mathf.Clamp(stamina + staminaRecoverWalk * Time.deltaTime, 0f, maxStamina);
        }

        // ── CROUCH ────────────────────────────────────────────────────────────
        // Teclado: C  |  Mando: B (Xbox) / Círculo (PS) = JoystickButton1

        private void HandleCrouch()
        {
            bool pressed = Input.GetKeyDown(KeyCode.C)
                        || Input.GetKeyDown(KeyCode.JoystickButton1);

            if (pressed)
                _isCrouching = !_isCrouching;

            if (_isCrouching)
            {
                _controller.height = crouchHeight;
                _controller.center = crouchCenter;
            }
            else
            {
                _controller.height = standingHeight;
                _controller.center = standingCenter;
            }

            if (_hasAnimator)
                _animator.SetBool(_animIDCrouch, _isCrouching);
        }

        // ── GROUNDED ──────────────────────────────────────────────────────────

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, Grounded);
        }

        // ── CAMERA ────────────────────────────────────────────────────────────

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = Mathf.Clamp(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f);
        }

        // ── STAMINA UI ────────────────────────────────────────────────────────

        private void UpdateStaminaUI()
        {
            if (staminaSlider != null)
                staminaSlider.value = stamina;
        }

        // ── MOVE ──────────────────────────────────────────────────────────────

        private void Move()
        {
            if (!_controller.enabled) return;

            bool isMoving = _input.move != Vector2.zero;
            bool isCrouching = _isCrouching;
            bool isSprinting = _input.sprint && stamina > 0f && isMoving && !isCrouching;

            float targetSpeed = isSprinting ? SprintSpeed : MoveSpeed;
            if (isCrouching) targetSpeed *= 0.5f;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                    ref _rotationVelocity, RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            Vector3 movement = targetDirection.normalized * (_speed * Time.deltaTime) +
                               new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

            Vector3 nextPosition = transform.position + movement;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(nextPosition, out hit, 0.5f, NavMesh.AllAreas))
                _controller.Move(movement);
            else
                _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }

            if (isSprinting)
            {
                _noiseTimer -= Time.deltaTime;
                if (_noiseTimer <= 0f)
                {
                    _noiseTimer = sprintNoiseInterval;
                    EmitNoise(sprintNoiseRadius);
                }
            }
            else
            {
                _noiseTimer = 0f;
            }
        }

        private void EmitNoise(float radius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
                hit.GetComponent<EnemyAI>()?.HearSound(transform.position);
        }

        // ── JUMP & GRAVITY ────────────────────────────────────────────────────

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                    _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f && !_isCrouching)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator)
                        _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                    _fallTimeoutDelta -= Time.deltaTime;
                else
                {
                    if (_hasAnimator)
                        _animator.SetBool(_animIDFreeFall, true);
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        // ── UTILS ─────────────────────────────────────────────────────────────

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}