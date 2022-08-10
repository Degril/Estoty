using UnityEngine;

namespace Gravity
{
    public class GravityAttractor : MonoBehaviour
    {
        private void Awake()
        {
            PolarTransform.SetPlanet(transform);
        }
    }
}
