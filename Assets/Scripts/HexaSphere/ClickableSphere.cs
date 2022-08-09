using System;
using Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sphere
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ClickableSphere : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Color _selectedColor;
        
        private Cell _neighbourCell;
        private Vector3 _connectedPosition;
        private MeshRenderer _renderer;
        private Color _startColor;

        public event Action<Cell, Vector3> OnCLickable;
        
        public void Init(Cell neighbourCell, Vector3 connectedPosition)
        {
            _neighbourCell = neighbourCell;
            _connectedPosition = connectedPosition;
            _renderer = GetComponent<MeshRenderer>();
            _startColor = _renderer.material.color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _neighbourCell.Show();
            OnCLickable?.Invoke(_neighbourCell, _connectedPosition);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _renderer.material.color = _selectedColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _renderer.material.color = _startColor;
        }
    }
}