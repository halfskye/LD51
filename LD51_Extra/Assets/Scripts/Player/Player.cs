using System.Collections.Generic;
using System.Linq;
using OldManAndTheSea.Utilities;
using OldManAndTheSea.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea
{
    public class Player : SingletonMonoBehaviour<Player>
    {
        public static string TAG_NAME = "Player";
        
        [SerializeField] private Ship _ship = null;
        public Vector3 Position => _ship.transform.position;

        [SerializeField] private Cannon _cannon = null;

        private Dictionary<Loot.Type, float> _loot = new Dictionary<Loot.Type, float>();

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

        private void AddLoot(Loot.Type type, float amount)
        {
            if (_loot.ContainsKey(type))
            {
                _loot[type] += amount;
            }
            else
            {
                _loot.Add(type, amount);
            }
        }
        
        public void AddLoot(Loot loot)
        {
            AddLoot(loot.LootType, loot.Amount);
        }

        #if DEBUG
        public void SinkRandomShip()
        {
            var allShips = FindObjectsOfType<Ship>();
            var ships = allShips.Where(x => x.IsAlive && !x.IsPlayer).ToArray();
            var ship = ships[Random.Range(0, ships.Length)];
            ship.Sink();
        }

        public void ResetShip()
        {
            _ship.Reset();
        }
        #endif // DEBUG
    }
}