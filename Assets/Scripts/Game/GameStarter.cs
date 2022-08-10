using System.Linq;
using Gravity;
using Sphere;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(HexaSphere))]
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private CharacterMover _mainCharacterPrefab;
        [SerializeField] private ClueCamera _camera;
    
        private HexaSphere _hexaSphere;
        private CharacterMover _mainCharacter;
        private CharacterMovementController _characterMovementController;
        private void Start()
        {
            _hexaSphere = GetComponent<HexaSphere>();
            _hexaSphere.GenerateCells();
            CreatePlayer();
            _characterMovementController = new CharacterMovementController(_mainCharacter);
        
            _hexaSphere.OnCellShown.AddListener(MoveCharacterToCell);
            _hexaSphere.OnSphereClicked.AddListener(OpenCell);
        }

        private void MoveCharacterToCell(Cell cell, Vector3 nextPosition)
        {
            _characterMovementController.MoveCharacterToMiddleOfCells(cell.transform, nextPosition);
        }

        private void OpenCell(Cell cell, Vector3 middlePosition)
        {
            MoveCharacterToCell(cell, middlePosition);
        }

        private void OnDestroy()
        {
            _hexaSphere.OnCellShown.RemoveListener(MoveCharacterToCell);
            _hexaSphere.OnSphereClicked.RemoveListener(OpenCell);
        }

        private void CreatePlayer()
        {
            var firstCell = _hexaSphere.Cells.First();
            firstCell.Show();
        
            _mainCharacter = Instantiate(_mainCharacterPrefab, firstCell.transform.position+ firstCell.transform.up * 3, Quaternion.identity, null);
            _camera.LookAtObject = _mainCharacter.transform;
        }
    }
}
