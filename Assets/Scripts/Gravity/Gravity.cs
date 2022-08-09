using System;
using Game;
using UnityEngine;

namespace Gravity
{
    [RequireComponent(typeof(CharacterMover))]
    public class Gravity : MonoBehaviour
    {
        private GravityAttractor _gravityAttractor;
        private CharacterMover _characterMover;
        public CharacterMover CharacterMover => _characterMover;
        
        public void Init(GravityAttractor attractor)
        {
            _gravityAttractor = attractor;
            _characterMover = GetComponent<CharacterMover>();
        }

        public void LateUpdate()
        {
            if (_characterMover.Grounded)
            {
                
            }//else 
           // if (_rigidbody)
            {
                _gravityAttractor.Attract(transform, _characterMover.Grounded, _characterMover.InsideObject, _characterMover.VerticalVelocity);
            }
            
        }
    }
}