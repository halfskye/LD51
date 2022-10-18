using OldManAndTheSea.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OldManAndTheSea
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private Player _player = null;

        private bool _isFireHeld = false;
        
        private void Update()
        {
            if (_isFireHeld)
            {
                _player.Ship.ActiveCannon.Fire_Hold();
            }
            
            //@TEMP/@DEBUG:
            #if DEBUG
            if (Input.GetKeyDown(KeyCode.X))
            {
                _player.SinkRandomShip();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _player.ResetShip();
            }
            #endif // DEBUG
        }

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
            _player.Ship.ActiveCannon.Aim(aim);
        }
        
        public void Fire(InputAction.CallbackContext context)
        {
            DebugLog($"On: {context.ReadValueAsButton()} | Duration {context.duration} | Started: {context.started} | Performed: {context.performed} | Canceled: {context.canceled}");

            if (context.started)
            {
                _player.Ship.ActiveCannon.Fire_Start();
                _isFireHeld = true;
            }
            if (context.canceled)
            {   
                _player.Ship.ActiveCannon.Fire_Stop();
                _isFireHeld = false;
            }
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.PLAYER_INPUT, message, this);
        }
    }
}