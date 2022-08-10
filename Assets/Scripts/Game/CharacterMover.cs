using System;
using Gravity;
using UnityEngine;

namespace Game
{
    public class CharacterMover : MonoBehaviour
    {
        private Vector3 _targetPosition;
        private Action _onMoveEnded;
        private float _currentSpeed;
            
        //animation
        private Animator _animator;
        private bool _hasAnimator;
        
        //commands
        private bool _isJump;
        private bool _isMove;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;

        // player
        private const float TerminalVelocity = 53.0f;
        
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float gravity = -15.0f;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;
        
        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float jumpHeight = 1.2f;
        
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;
        
        [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate = 10.0f;
        
        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool grounded = true;
        
        [Tooltip("Useful for rough ground")]
        public float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float groundedRadius = 0.28f;
        
        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool insideObject = false;
        
        [Tooltip("The radius of the area check. Should match the radius of the CharacterController")]
        public float areaHeight = 1f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float areaRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask groundLayers;
        
        public float verticalVelocity;
        
        private void Start()
        {
            AssignAnimationIDs();
            _hasAnimator = TryGetComponent(out _animator);
        }
        
        public void MoveTo(Vector3 targetPosition, Action onMoveEnded)
        {
            var posY = transform.position.GetPolarPositionY();
            transform.LookAt(targetPosition.SetPolarY(posY));
            _targetPosition = targetPosition;
            _isMove = true;
            _onMoveEnded = onMoveEnded;
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
        }
        
        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Rotate();
            Move();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(transform.position, _targetPosition);
        }

        private void Rotate()
        {
            var targetDirection = (transform.position - PolarTransform.GetPolarPositionZero()).normalized;
            var objectUp = transform.up;

            transform.rotation *= Quaternion.FromToRotation(objectUp, targetDirection);
        }

        private void Move()
        {
            if (_isMove && transform.position.PolarDistanceIsNear(_targetPosition))
            {
                _isMove = false;
                _onMoveEnded?.Invoke();
            }
            
            var targetSpeed = _isMove ? moveSpeed : 0;
            
            if (_currentSpeed < targetSpeed - 0.1f || _currentSpeed > targetSpeed + 0.1f)
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            }
            
            var targetDirection = (transform.position - PolarTransform.GetPolarPositionZero()).normalized;

            var deltaUp = targetDirection * (Time.deltaTime * verticalVelocity);
            var deltaForward = ((_targetPosition - transform.position) * (_currentSpeed * Time.deltaTime));
            if (grounded && verticalVelocity < 0)
                deltaUp = Vector3.zero;
                
            transform.position += deltaUp + deltaForward;

            
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed,_currentSpeed);
            }
        }


        [ContextMenu("123")]
        public void DoJump()
        {
            _isJump = true;
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                _fallTimeoutDelta = fallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }
            

                if (verticalVelocity < 0.0f)
                {
                    verticalVelocity = -2f;
                }

                if (_isJump && _jumpTimeoutDelta <= 0.0f)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = jumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                _isJump = false;
            }
            
            if (verticalVelocity < TerminalVelocity)
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }
        
        private void GroundedCheck()
        {
            var spherePosition = transform.position - transform.up * groundedOffset;
            
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            var centerPosition = transform.position;

            insideObject = Physics.CheckCapsule(centerPosition - transform.up * (areaHeight * 0.5f)
                ,spherePosition + transform.up * (areaHeight * 0.5f), areaRadius, groundLayers,
                QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, grounded);
            }
        }
    }
}