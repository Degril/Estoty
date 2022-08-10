using Gravity;
using Sphere;
using UnityEngine;

namespace Game
{
    public class CharacterMovementController
    {
        private readonly CharacterMover _mainCharacter;

        public CharacterMovementController(CharacterMover mainCharacter)
        {
            _mainCharacter = mainCharacter;
        }

        public void MoveCharacterToMiddleOfCells(Transform nextObjectTransform, Vector3 nextPosition)
        {
            _mainCharacter.MoveTo(nextPosition.SetPolarY(_mainCharacter.transform.position.GetPolarPositionY())
                , () => MoveToCell(nextObjectTransform.position));
        }

        private void MoveToCell(Vector3 position)
        {
            if(position.GetPolarPositionY() > _mainCharacter.transform.position.GetPolarPositionY() - 0.5f)
                _mainCharacter.DoJump();
            _mainCharacter.MoveTo(position.SetPolarY(_mainCharacter.transform.position.GetPolarPositionY()), null);
        }
    }
}