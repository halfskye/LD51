using OldManAndTheSea.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OldManAndTheSea
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private Player _player = null;

        //@TEMP/@DEBUG:
        #if DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                _player.SinkRandomShip();
            }
        }
        #endif // DEBUG

        public void Move(InputAction.CallbackContext context)
        {
            DebugLog("Move!");

            var move = context.ReadValue<Vector2>();
            _player.Move(move);
        }
        
        public void Aim(InputAction.CallbackContext context)
        {
            DebugLog("Aim!");
            
            var aim = context.ReadValue<Vector2>();
            aim.y = -aim.y;
            _player.Aim(aim);
        }
        
        public void Fire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                DebugLog("Fire!");

                _player.Fire();
            }
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.PLAYER_INPUT, message, this);
        }
    }
}