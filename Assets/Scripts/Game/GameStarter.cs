using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Gravity;
using Sphere;
using UnityEngine;

[RequireComponent(typeof(Hexasphere))]
public class GameStarter : MonoBehaviour
{
    [SerializeField] private Gravity.Gravity _mainCharacterPrefab;
    [SerializeField] private ClueCamera _camera;
    private Hexasphere _hexasphere;
    private Gravity.Gravity _mainCharacter;
    private void Start()
    {
        _hexasphere = GetComponent<Hexasphere>();
        _hexasphere.GenerateCells();
        CreatePlayer();
        _hexasphere.OnCellShown.AddListener(MoveCharacterToCell);
        _hexasphere.OnClickableSphere.AddListener(OpenCell);
    }

    private void OpenCell(Cell cell, Vector3 middlePosition)
    {
        MoveCharacterToCell(cell, middlePosition);
    }

    private void OnDestroy()
    {
        _hexasphere.OnCellShown.RemoveListener(MoveCharacterToCell);
        _hexasphere.OnClickableSphere.RemoveListener(OpenCell);
    }

    private void CreatePlayer()
    {
        var firstCell = _hexasphere.Cells.First();
        firstCell.Show();
        _mainCharacter = Instantiate(_mainCharacterPrefab, firstCell.transform.position+ firstCell.transform.up * 3, Quaternion.identity, null);
        _mainCharacter.GetComponent<Gravity.Gravity>().Init(_hexasphere.GravityAttractor);
        _camera.LookAt = _mainCharacter.transform;
    }


    private void MoveCharacterToCell(Cell cell, Vector3 middlePosition)
    {
        _mainCharacter.CharacterMover.MoveTo(middlePosition.SetPolarY(_mainCharacter.transform.position.GetPolarPositionY())
            , () => OnMovedToNextCell(cell));
    }

    private void OnMovedToNextCell(Cell cell)
    {
        if(cell.transform.position.GetPolarPositionY() > _mainCharacter.transform.position.GetPolarPositionY() - 0.5f)
            _mainCharacter.CharacterMover.DoJump();
        _mainCharacter.CharacterMover.MoveTo(cell.transform.position.SetPolarY(_mainCharacter.transform.position.GetPolarPositionY()), null);
    }
}
