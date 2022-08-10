using System;
using System.Collections.Generic;
using Gravity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sphere
{
    [Serializable]
    public class Cell : MonoBehaviour
    {
        [SerializeField] private ClickableSphere _clickableSpherePrefab;
        public bool IsShowed { get; private set; }
        
        private Dictionary<Vector3, Cell> _neighbours = new();

        private Vector3 _centerPoint;
        private Vector3 _positionOnSphere;

        private static List<ClickableSphere> _clickableSpheres = new();

        private Vector3 startPosition;

        public event Action<Cell, Vector3> OnShow;
        public event Action<Cell, Vector3> OnSpherClicked;

        private const int RequiredResource1 = 200;
        private const int RequiredResource2 = 200;

        public void Show(Vector3? connectedPosition = null)
        {
            foreach (var clickableSphere in _clickableSpheres)
            {
                Destroy(clickableSphere.gameObject);
            }
            _clickableSpheres.Clear();
            startPosition = _positionOnSphere;
            transform.position = _positionOnSphere.SetPolarY(_positionOnSphere.GetPolarPositionY() + Random.Range(-0.5f,0.5f));
            IsShowed = true;
            GenerateNeighbourButtons();
            if(connectedPosition != null)
                OnShow?.Invoke(this, connectedPosition.Value);
        }

        private void GenerateNeighbourButtons()
        {
            foreach (var (connectionPosition, neighbour) in _neighbours)
            {
                if(neighbour.IsShowed)
                    continue;
                
                var sphereButton = Instantiate(_clickableSpherePrefab);
                sphereButton.transform.position = connectionPosition - startPosition + transform.position + transform.parent.position;
                sphereButton.Init(neighbour, connectionPosition);
                _clickableSpheres.Add(sphereButton);
                sphereButton.OnClicked += OnSpherClicked;
            }

        }
        
        public void Init(Dictionary<Vector3, Cell> neighbours, Vector3 centerPoint)
        {
            _centerPoint = centerPoint;
            _neighbours = neighbours;
            _positionOnSphere = transform.localPosition;
            transform.localPosition = Vector3.zero;
        }

        public void OnDrawGizmosSelected()
        {
            foreach (var keyValuePair in _neighbours)
            {
                Gizmos.DrawLine(transform.position, keyValuePair.Value.transform.position);
            }
        }
    }
}