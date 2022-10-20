using System;
using DarkTonic.PoolBoss;
using OldManAndTheSea.Utilities;
using OldManAndTheSea.World;
using UnityEngine;

namespace OldManAndTheSea
{
    public class Loot : MonoBehaviour
    {
        public enum Type
        {
            GOLD = 0,
            WOOD = 1,
            FOOD = 2,
        }
        [SerializeField] private Type _type = Type.GOLD;
        public Type LootType => _type;

        [SerializeField] private float _amount = 1f;
        public float Amount => _amount;

        [SerializeField] private float _riseSpeed = 1f;
        private float _depth = 0f;

        private enum State
        {
            RISING = 0,
            IDLE = 1,
        }
        private State _state = State.IDLE;

        public void Initialize(float amount, float depth)
        {
            _state = State.RISING;
            _amount = amount;
            _depth = depth;
        }
        
        private void Update()
        {
            if (_state == State.RISING)
            {
                UpdateState_Rising();
            }
            else if (_state == State.IDLE)
            {
                UpdateState_Idle();
            }
        }

        private void UpdateState_Rising()
        {
            var thisTransform = this.transform;
            var position = thisTransform.position;
            var rise = _riseSpeed * Time.deltaTime;
            position += WorldManager.Instance.Data.Sea_Up * rise;
            thisTransform.position = position;
            
            var waterHeight = WorldManager.Instance.GetWaterHeightAtPosition(position);
            if (position.y >= waterHeight)
            {
                _state = State.IDLE;
            }
        }

        private void UpdateState_Idle()
        {
            CastToWaterHeight();
        }
        
        private void CastToWaterHeight()
        {
            var position = this.transform.position;
            var waterHeight = WorldManager.Instance.GetWaterHeightAtPosition(position);
            this.transform.position = new Vector3(position.x, waterHeight, position.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            DebugLog($"Loot collision - {other.name} - {other.tag}");
            
            if (other.CompareTag(Player.TAG_NAME))
            {
                var player = other.GetComponentInParent<Player>();
                player.AddLoot(this);

                Despawn();
            }
        }

        private void Despawn()
        {
            PoolBoss.Despawn(this.transform);
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.WEAPONS, message, this);
        }
    }
}