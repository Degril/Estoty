using UnityEngine;

namespace Gravity
{
    public static class PolarTransform
    {
        private static Transform _planetTransform;
        public static void SetPlanet(Transform transform)
        {
            _planetTransform = transform;
        }

        public static float GetPolarPositionY(this Vector3 position)
        {
            return Vector3.Distance(_planetTransform.position, position);
        }

        public static Vector3 SetPolarY(this Vector3 position, float posy)
        {
            return _planetTransform.position + (position - _planetTransform.position).normalized * posy;
        }

        public static Vector3 GetPolarPositionZero()
        {
            return _planetTransform.position;
        }
        public static Vector2 ToPolar(this Vector3 position)
        {
            Vector2 polarPosition;
 
            polarPosition.y = Mathf.Atan2(position.x,position.z);
 
            var xzLen = new Vector2(position.x,position.z).magnitude; 
            polarPosition.x = Mathf.Atan2(-position.y,xzLen);
 
            polarPosition *= Mathf.Rad2Deg;
 
            return polarPosition;
        }
 
 
        public static Vector3 PolarToCartesian(Vector2 polarPosition)
        {
            var origin = new Vector3(0,0,1);
            var rotation = Quaternion.Euler(polarPosition.x,polarPosition.y,0);
            var position = rotation * origin;
 
            return position;
        }
    }
}