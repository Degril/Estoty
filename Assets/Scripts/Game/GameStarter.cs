using System.Linq;
using Game;
using Gravity;
using Sphere;
using UnityEngine;

[RequireComponent(typeof(HexaSphere))]
public class GameStarter : MonoBehaviour
{
    [SerializeField] private CharacterMover _mainCharacterPrefab;
    [SerializeField] private ClueCamera _camera;
    
    private HexaSphere _hexaSphere;
    private CharacterMover _mainCharacter;
    private void Start()
    {
        _hexaSphere = GetComponent<HexaSphere>();
        _hexaSphere.GenerateCells();
        
        CreatePlayer();
        _hexaSphere.OnCellShown.AddListener(MoveCharacterToCell);
        _hexaSphere.OnClickableSphere.AddListener(OpenCell);
    }

    private void OpenCell(Cell cell, Vector3 middlePosition)
    {
        MoveCharacterToCell(cell, middlePosition);
    }

    private void OnDestroy()
    {
        _hexaSphere.OnCellShown.RemoveListener(MoveCharacterToCell);
        _hexaSphere.OnClickableSphere.RemoveListener(OpenCell);
    }

    private void CreatePlayer()
    {
        var firstCell = _hexaSphere.Cells.First();
        firstCell.Show();
        
        _mainCharacter = Instantiate(_mainCharacterPrefab, firstCell.transform.position+ firstCell.transform.up * 3, Quaternion.identity, null);
        _camera.LookAt = _mainCharacter.transform;
    }


    private void MoveCharacterToCell(Cell cell, Vector3 middlePosition)
    {
        _mainCharacter.MoveTo(middlePosition.SetPolarY(_mainCharacter.transform.position.GetPolarPositionY())
            , () => OnMovedToNextPoint(cell.transform.position));
    }

    private void OnMovedToNextPoint(Vector3 position)
    {
        if(position.GetPolarPositionY() > _mainCharacter.transform.position.GetPolarPositionY() - 0.5f)
            _mainCharacter.DoJump();
        _mainCharacter.MoveTo(position.SetPolarY(_mainCharacter.transform.position.GetPolarPositionY()), null);
    }
}
