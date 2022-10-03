using UnityEngine;

namespace OldManAndTheSea.World
{
    [CreateAssetMenu(fileName = "WorldManagerSettings", menuName = "OldManAndTheSea/World/New WorldManagerSettings", order = 0)]
    public class WorldManagerSettings : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float _seaToSkyRatio = 0.75f;
        public float SeaToSkyRatio => _seaToSkyRatio;

        [SerializeField] private float _seaDistanceFromCamera = 3000f;
        public float SeaDistanceFromCamera => _seaDistanceFromCamera;

        [SerializeField] private float _seaNearWidth = 100f;
        public float SeaNearWidth => _seaNearWidth;

        [SerializeField] private float _seaFarWidth = 3000f;
        public float SeaFarWidth => _seaFarWidth;

        [SerializeField] private float _seaDistanceLogBase = 10f;
        public float SeaDistanceLogBase => _seaDistanceLogBase;

        public float SeaNearToFarWidthRatio => SeaNearWidth / SeaFarWidth;

        public float LogDistanceFromCamera(float distance) => Mathf.Log(distance, SeaDistanceLogBase);
        public float LogSeaDistanceFromCamera => LogDistanceFromCamera(SeaDistanceFromCamera);

        public float PowSeaDistance(float log) => Mathf.Pow(_seaDistanceLogBase, log);
    }
}