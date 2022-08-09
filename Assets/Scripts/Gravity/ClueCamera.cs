using System;
using UnityEditor;
using UnityEngine;

namespace Gravity
{
    public class ClueCamera : MonoBehaviour
    {
        public Transform LookAt { get; set; }

        private float _angleRotation;
        
        public float sensitivity = 3; // чувствительность мышки
        public float limit = 80; // ограничение вращения по Y
        public float zoom = 0.25f; // чувствительность при увеличении, колесиком мышки
        public float zoomMax = 10; // макс. увеличение
        public float zoomMin = 3; // мин. увеличение
        private float X, Y;
        public Vector3 offset;

        private void Update()
        {
            if (LookAt)
            {
                var parent = transform.parent;
                parent.position = LookAt.transform.position;
                
                if(Input.GetAxis("Mouse ScrollWheel") > 0) offset.z += zoom;
                else if(Input.GetAxis("Mouse ScrollWheel") < 0) offset.z -= zoom;
                offset.z = Mathf.Clamp(offset.z, -Mathf.Abs(zoomMax), -Mathf.Abs(zoomMin));

                _angleRotation = Input.GetAxis("Mouse X") * sensitivity;
                
                var targetDirection = (parent.transform.position - PolarTransform.GetPolarPositionZero()).normalized;
                var objectUp = parent.transform.up;

                parent.rotation = Quaternion.FromToRotation(objectUp, targetDirection) * parent.rotation * Quaternion.Euler(0,_angleRotation, 0);
                //parent.rotation *= Quaternion.FromToRotation(parent.up, targetDirection);// * Quaternion.Euler(0,_angleRotation, 0);
                 //Y += Input.GetAxis("Mouse Y") * sensitivity;
                 //Y = Mathf.Clamp (Y, -limit, limit);
                //parent.localEulerAngles = new Vector3(-Y, X, 0);
                //transform.position = transform.localRotation * offset + LookAt.position;
                //transform.LookAt(LookAt);
            }
        }
    }
}