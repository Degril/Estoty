using UnityEngine;

namespace Gravity
{
    public class ClueCamera : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 3;

        private float _angleRotation;
        
        public Transform LookAtObject { get; set; }

        private void Update()
        {
            if (!LookAtObject) return;
            
            MoveAndLookToCharacter();
        }

        private void MoveAndLookToCharacter()
        {
            var parent = transform.parent;
            parent.position = LookAtObject.transform.position;

            _angleRotation = Input.GetAxis("Mouse X") * sensitivity;
                
            var targetDirection = (parent.transform.position - PolarTransform.GetPolarPositionZero()).normalized;
            var objectUp = parent.transform.up;

            parent.rotation = Quaternion.FromToRotation(objectUp, targetDirection) * parent.rotation * Quaternion.Euler(0,_angleRotation, 0);
            
        }
    }
}