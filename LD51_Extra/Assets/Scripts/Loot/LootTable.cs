using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "OldManAndTheSea/Loot/New LootTable", order = 0)]
    public class LootTable : SerializedScriptableObject
    {
        [SerializeField, MinMaxSlider(1,10)] private Vector2Int _lootCountRange = Vector2Int.one;

        [SerializeField, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout, KeyLabel = "Loot Type")]
        private Dictionary<Loot.Type, LootTypeSettings> _lootTypeSettings = new Dictionary<Loot.Type, LootTypeSettings>();

        [Serializable]
        private class LootTypeSettings
        {
            [SerializeField] private Transform _prefab = null;
            public Transform Prefab => _prefab;
            
            [SerializeField] private float _odds = 1f;
            public float Odds => _odds;
            
            [SerializeField, MinMaxSlider(1f,50f)] private Vector2 _amount = Vector2.one;
            public Vector2 Amount => _amount;
        }

        public class LootData
        {
            public Loot.Type Type { get; private set; } = Loot.Type.GOLD;
            public float Amount { get; private set; } = 1f;

            public LootData(Loot.Type type, float amount)
            {
                Type = type;
                Amount = amount;
            }
        }

        public Transform GetLootTypePrefab(Loot.Type lootType)
        {
            if (_lootTypeSettings.ContainsKey(lootType))
            {
                return _lootTypeSettings[lootType].Prefab;
            }

            return null;
        }

        public IEnumerable<LootData> GenerateLoot()
        {
            var loot = new List<LootData>();

            var count = Random.Range(_lootCountRange.x, _lootCountRange.y + 1);
            for (var i = 0; i < count; ++i)
            {
                Loot.Type lootType = GetRandomLootType();
                Vector2 amountRange = _lootTypeSettings[lootType].Amount;
                var amount = Random.Range(amountRange.x, amountRange.y);
                
                loot.Add(new LootData(lootType, amount));
            }

            return loot;
        }

        private Loot.Type GetRandomLootType()
        {
            var lootType = Loot.Type.GOLD;
            
            var total = _lootTypeSettings.Sum(x => x.Value.Odds);
            var random = Random.Range(0f, total);
            var counter = 0f;
            _lootTypeSettings.ForEach(x =>
                {
                    var lootOdds = x.Value.Odds;
                    if (random >= counter && random < lootOdds)
                    {
                        lootType = x.Key;
                    }
                    counter += lootOdds;
                }
            );
            return lootType;
            
            // var loot = _lootTypeSettings.First(x =>
            // {
            //     var lootOdds = x.Value.Odds;
            //     if (random >= counter && random < lootOdds)
            //     {
            //         return true;
            //     }
            //     counter += lootOdds;
            //     return false;
            // });
            // return loot.Key;
        }
    }
}