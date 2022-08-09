using System;
using UnityEngine;

namespace Gravity
{
    public class GravityAttractor : MonoBehaviour
    {
        private void Awake()
        {
            PolarTransform.SetPlanet(transform);
        }

        public void Attract(Transform objectRigidbody, bool isGrounded, bool insideObject, float verticalVelocity)
        {
            var targetDirection = (objectRigidbody.transform.position - transform.position).normalized;
            var objectUp = objectRigidbody.transform.up;

            objectRigidbody.rotation = Quaternion.FromToRotation(objectUp, targetDirection) * objectRigidbody.rotation;
            // if (!isGrounded && verticalVelocity == 0)
            // {
            //     Debug.Log("gravity");
            //     objectRigidbody.transform.position += (-targetDirection * ((Physics.gravity.magnitude) * Time.deltaTime));
            // }
            // else if (insideObject || verticalVelocity != 0)
            {
                if (verticalVelocity == 0)
                    verticalVelocity = 0.2f;
                //objectRigidbody.GetComponent<CharacterController>().Move(targetDirection * ((Physics.gravity.magnitude) * Time.deltaTime * verticalVelocity));
                
            }
        }
    }
}
