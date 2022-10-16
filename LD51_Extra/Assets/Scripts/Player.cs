using System.Linq;
using OldManAndTheSea.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Ship _ship = null;

        [SerializeField] private Cannon _cannon = null;
        
        private Vector2 _moveAxis = Vector2.zero;
        private Vector2 _aimAxis = Vector2.zero;

        private void Update()
        {
            UpdateMove();
            UpdateAim();
        }

        private void UpdateMove()
        {
            _ship.Move(_moveAxis);
        }

        private void UpdateAim()
        {
            _cannon.Aim(_aimAxis);
        }

        public void Move(Vector2 move)
        {
            _moveAxis = move;
        }

        public void Aim(Vector2 aim)
        {
            _aimAxis = aim;
        }

        public void Fire()
        {
            _cannon.Fire();
        }

        #if DEBUG
        public void SinkRandomShip()
        {
            var allShips = FindObjectsOfType<Ship>();
            var ships = allShips.Where(x => x.IsAlive && !x.IsPlayer).ToArray();
            var ship = ships[Random.Range(0, ships.Length)];
            ship.Sink();
        }
        #endif // DEBUG
    }
}