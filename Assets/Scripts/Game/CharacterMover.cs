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

        public void MoveTo(Vector3 targetPosition, Action onMoveEnded)
        {
            var posY = transform.position.GetPolarPositionY();
            transform.LookAt(targetPosition.SetPolarY(posY));
            _targetPosition = targetPosition;
            _isMove = true;
            _onMoveEnded = onMoveEnded;
        }
            
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
        private int _animIDMotionSpeed;
        
        
        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _terminalVelocity = 53.0f;
        
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;
        
        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        
        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;
        
        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool InsideObject = false;
        
        [Tooltip("The radius of the area check. Should match the radius of the CharacterController")]
        public float AreaHeight = 1f;
        
        
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float AreaRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;
        
        public float VerticalVelocity;
        
        private void Start()
        {
            AssignAnimationIDs();
            _hasAnimator = TryGetComponent(out _animator);
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
        
        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, _targetPosition);
        }

        private void Rotate()
        {
            
        }

        private void Move()
        {
            if (_isMove && ((transform.position.ToPolar() - _targetPosition.ToPolar()).sqrMagnitude < 3f))
            {
                _isMove = false;
                _onMoveEnded?.Invoke();
            }
            var targetSpeed = _isMove ? MoveSpeed : 0;
            
            // accelerate or decelerate to target speed
            if (_currentSpeed < targetSpeed - 0.1f ||
                _currentSpeed > targetSpeed + 0.1f)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                //_currentSpeed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }
            
            //if (_isMove)
            {
                var targetDirection = (transform.position - PolarTransform.GetPolarPositionZero()).normalized;
                var objectUp = transform.up;

                var deltaUp = targetDirection * (Time.deltaTime * VerticalVelocity);
                var deltaForward = ((_targetPosition - transform.position) * (_currentSpeed * Time.deltaTime));
                //GetComponent<CharacterController>().Move(deltaUp + deltaForward);
                if (Grounded && VerticalVelocity < 0)
                    deltaUp = Vector3.zero;
                
                transform.position += deltaUp + deltaForward;
                //GetComponent<Rigidbody>().AddForce((_targetPosition - transform.position) * (_currentSpeed * Time.deltaTime), ForceMode.VelocityChange);
                //transform.position += ((_targetPosition - transform.position) * (_currentSpeed * Time.deltaTime));
                //transform.transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _currentSpeed * Time.deltaTime);
            }

            
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
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }
            

                // stop our velocity dropping infinitely when grounded
                if (VerticalVelocity < 0.0f)
                {
                    VerticalVelocity = -2f;
                }

                // Jump
                if (_isJump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _isJump = false;
            }
            
            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (VerticalVelocity < _terminalVelocity)
            {
                VerticalVelocity += Gravity * Time.deltaTime;
            }
            
            // if (_isJump)
            // {
            //     transform.position += transform.up * (VerticalVelocity * Time.deltaTime * JumpHeight);
            // }
        }
        
        private void GroundedCheck()
        {
            // set sphere position, with offset
            var spherePosition = transform.position - transform.up * GroundedOffset;
            
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            var centerPosition = transform.position;

            InsideObject = Physics.CheckCapsule(centerPosition - transform.up * (AreaHeight * 0.5f)
                ,spherePosition + transform.up * (AreaHeight * 0.5f), AreaRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }
    }
}