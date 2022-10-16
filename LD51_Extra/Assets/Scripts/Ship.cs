using DarkTonic.PoolBoss;
using OldManAndTheSea.Utilities;
using OldManAndTheSea.World;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea
{
    public class Ship : MonoBehaviour
    {
        [SerializeField] private bool _isPlayer = false;
        public bool IsPlayer => _isPlayer;

        private const string STARTING_STATE_TITLE = "Starting State";
        private const float COORDS_EXTENT_LOWER = -0.25f;
        private const float COORDS_EXTENT_UPPER = 1.25f;

        [TitleGroup("@STARTING_STATE_TITLE")]
        [SerializeField] private float _health = 20f;
        private float _activeHealth = 0f;
        [SerializeField, LabelText("Randomize by Range")] private bool _useRandomCoords = true;
        [HorizontalGroup("Coords")]
        [SerializeField, BoxGroup("Coords/x"), HideIf("@_useRandomCoords"), Range(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private float xCoord = 0.25f;
        [SerializeField, BoxGroup("Coords/y"), HideIf("@_useRandomCoords"), Range(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private float yCoord = 0.05f;
        [SerializeField, BoxGroup("Coords/x"), HideIf("@!_useRandomCoords"), MinMaxSlider(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private Vector2 xCoordRandom = Vector2.zero;
        [SerializeField, BoxGroup("Coords/y"), HideIf("@!_useRandomCoords"), MinMaxSlider(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private Vector2 yCoordRandom = Vector2.zero;
        
        private const float START_ANGLE_EXTENT = 180f;
        [SerializeField, MinMaxSlider(-START_ANGLE_EXTENT, START_ANGLE_EXTENT), LabelText("Starting Rotation Range")] private Vector2 _startRotationRange = Vector2.zero;
        // private float _startRotation = 0f;

        private const float RANDOM_ANGLE_EXTENT = 1f;
        private const string MOVEMENT_TITLE = "Movement";
        [TitleGroup("@MOVEMENT_TITLE")]
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField, MinMaxSlider(-RANDOM_ANGLE_EXTENT, RANDOM_ANGLE_EXTENT), LabelText("Active Rotation Range"), HideIf("@_oscillateRotation")] private Vector2 _randomRotationRange = Vector2.zero;
        private float _randomRotation = 0f;
        [SerializeField, HideIf("@_oscillateRotation")] private float _rotationSpeed = 10f;
        [SerializeField] private bool _oscillateRotation = false;
        private const float OSCILLATION_MAX_EXTENT = 1f;
        [SerializeField, HideIf("@!_oscillateRotation")] private Vector2 _oscillationExtents = new Vector2(-OSCILLATION_MAX_EXTENT, OSCILLATION_MAX_EXTENT);
        [SerializeField, HideIf("@!_oscillateRotation")] private float _oscillationSpeed = 0.1f;
        private float _currentOscillation = 0f;
        private float _oscillationDirection = 1f;
        [SerializeField] private float _sinkAcceleration = 0.001f;
        private float _sinkVelocity = 0f;
        [SerializeField] private float _sunkDepth = 0.5f;
        private float _activeDepth = 0f;
        
        private const string VISUALS_TITLE = "Visuals";
        [TitleGroup("@VISUALS_TITLE")]
        [SerializeField] private MeshRendererController _sailVisual = null;
        [SerializeField] private MeshRendererController _stopVisual = null;
        
        private const string LOOT_TITLE = "Loot";

        [TitleGroup("@LOOT_TITLE")]
        [SerializeField] private LootTable _lootTable;
        [SerializeField, MinMaxSlider(-1f,1f)] private Vector2 _lootDeviationRangeX = Vector2.zero;
        [SerializeField, MinMaxSlider(-1f,1f)] private Vector2 _lootDeviationRangeY = Vector2.zero;
        
        private bool _hasBecomeVisible = false;
        private const float MAX_INVISIBLE_TIME = 30f;
        private float _invisibleTimer = 0f;

        private Renderer[] _renderers = null;
        
        private enum State
        {
            Alive = 0,
            Sinking = 1,
            Sunk = 2,
        }
        private State _state = State.Alive;
        public bool IsAlive => _state == State.Alive;

        private void Awake()
        {
            _renderers = this.GetComponentsInChildren<Renderer>();
        }

        private void Start()
        {
            SetStartState();
        }

        private void SetStartState()
        {
            _state = State.Alive;
            _activeHealth = _health;
            
            _hasBecomeVisible = _isPlayer;
            
            var worldManager = WorldManager.Instance;
            var coords = Vector2.zero;

            //@TODO: Set position.
            if (_useRandomCoords)
            {
                var x = Mathf.Approximately(xCoordRandom.x, xCoordRandom.y)
                    ? xCoordRandom.x
                    : Random.Range(xCoordRandom.x, xCoordRandom.y);
                
                var y = Mathf.Approximately(yCoordRandom.x, yCoordRandom.y)
                    ? yCoordRandom.x
                    : Random.Range(yCoordRandom.x, yCoordRandom.y);

                DebugLog($"Coords: {x}, {y}");
                
                coords = new Vector2(x, y);
            }
            else
            {
                coords = new Vector2(xCoord, yCoord);
            }

            var position = worldManager.CoordinatesToWorldPoint(coords);
            this.transform.position = position;

            // var rotation = _isPlayer ? worldManager.Data.WestToEast_Normalized : worldManager.Data.EastToWest_Normalized;
            var startRotation = Random.Range(_startRotationRange.x, _startRotationRange.y);
            // this.transform.rotation = Quaternion.LookRotation(_startRotation, worldManager.Data.Sea_Up);
            var baseRotation = Quaternion.LookRotation(worldManager.Data.Sea_Forward, worldManager.Data.Sea_Up);
            this.transform.rotation = baseRotation * Quaternion.Euler(0f, startRotation, 0f);
            
            _randomRotation = Random.Range(_randomRotationRange.x, _randomRotationRange.y);
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Alive:
                    UpdateState_Alive();
                break;
                case State.Sinking:
                    UpdateState_Sinking();
                    break;
                case State.Sunk:
                    UpdateState_Sunk();
                    break;
                default:
                    break;
            }
        }

        private void UpdateState_Alive()
        {
            if (!_isPlayer)
            {
                var moveAxis = GetNPCMoveAxis();

                UpdateMovement(moveAxis);

                UpdateInvisibleSafetyCheck();
            }
        }

        private void UpdateState_Sinking()
        {
            _sinkVelocity += _sinkAcceleration * Time.deltaTime * Time.deltaTime;
            _activeDepth += _sinkVelocity;
            this.transform.position -= WorldManager.Instance.Data.Sea_Up * _sinkVelocity;

            if (_activeDepth >= _sunkDepth)
            {
                _state = State.Sunk;
            }
        }

        private void UpdateState_Sunk()
        {
            GenerateSunkGoods();
            
            Despawn();
        }

        private void GenerateSunkGoods()
        {
            var loot = _lootTable.GenerateLoot();
            loot.ForEach(lootData =>
            {
                GenerateLoot(lootData.Type, lootData.Amount);
            });
        }

        private void GenerateLoot(Loot.Type lootType, float amount)
        {
            var lootPrefab = _lootTable.GetLootTypePrefab(lootType);
            var position = this.transform.position;
            position += WorldManager.Instance.Data.Sea_Right * Random.Range(_lootDeviationRangeX.x, _lootDeviationRangeX.y);
            position += WorldManager.Instance.Data.Sea_Forward * Random.Range(_lootDeviationRangeY.x, _lootDeviationRangeY.y);
            var rotation = this.transform.rotation;
            var spawn = PoolBoss.SpawnInPool(lootPrefab, position, rotation);
            var loot = spawn.GetComponent<Loot>();
            loot.Initialize(amount, _sunkDepth);
        }

        #region Movement
        
        private Vector2 GetNPCMoveAxis()
        {
            var rotation = UpdateRotation();
            var speed = UpdateMovementSpeed();

            return new Vector2(rotation, speed);
        }
        
        private float UpdateRotation()
        {
            if (_oscillateRotation)
            {
                _currentOscillation += _oscillationSpeed * _oscillationDirection * Time.deltaTime;
                if (_currentOscillation <= _oscillationExtents.x || _currentOscillation >= _oscillationExtents.y)
                {
                    _currentOscillation = Mathf.Max(_oscillationExtents.x, math.min(_oscillationExtents.y, _currentOscillation));
                    _oscillationDirection = -_oscillationDirection;
                }

                return _currentOscillation;
            }
            else
            {
                return _randomRotation;
            }
        }

        private float UpdateMovementSpeed()
        {
            var rotateExtentScale = Mathf.Abs(_currentOscillation) / OSCILLATION_MAX_EXTENT;
            return 1f - rotateExtentScale;
        }

        private void UpdateMovement(Vector2 direction)
        {
            var shipTransform = this.transform;

            var rotationSpeed = direction.x * _rotationSpeed * Time.deltaTime;
            var rotateBy = Quaternion.Euler(0f, rotationSpeed, 0f);
            shipTransform.rotation *= rotateBy;

            var movementSpeed = Mathf.Max(0f, direction.y * _movementSpeed * Time.deltaTime);
            shipTransform.position += shipTransform.forward * movementSpeed;

            var isPastSeaEdge = false;
            if (shipTransform.position.z > WorldManager.Instance.Data.Sea_Middle_Top.z)
            {
                isPastSeaEdge = true;
                var delta = shipTransform.position.z - WorldManager.Instance.Data.Sea_Middle_Top.z;
                var crestDirection = WorldManager.Instance.Data.Sea_Up / 10f  + WorldManager.Instance.Data.Sea_Forward / 5f;
                shipTransform.position -= crestDirection * (delta * _movementSpeed * Time.deltaTime);
            }

            var isNearlyStopped = !isPastSeaEdge && direction.y < 0.6f && (_oscillationDirection * _currentOscillation >= 0f);
            _sailVisual.Turn(!isNearlyStopped || _stopVisual == null);
            if (_stopVisual != null)
            {
                _stopVisual.Turn(isNearlyStopped);
            }
        }

        public void Move(Vector2 move)
        {
            UpdateMovement(move);
        }
        
        #endregion Movement

        public void TakeDamage(float damage)
        {
            _activeHealth -= damage;
            if (_activeHealth <= 0f)
            {
                Sink();
            }
        }

        public void Sink()
        {
            _state = State.Sinking;
            _sinkVelocity = 0f;
            _activeDepth = 0f;
        }

        private void UpdateInvisibleSafetyCheck()
        {
            if (!_hasBecomeVisible)
            {
                _invisibleTimer += Time.deltaTime;
                if (_invisibleTimer > MAX_INVISIBLE_TIME)
                {
                    Despawn();
                }
            }
            else
            {
                _renderers.ForEach(x =>
                {
                    if (!GeometryUtility.TestPlanesAABB(WorldManager.Instance.Data.CameraFrustumPlanes, x.bounds))
                    {
                        Despawn();
                    }
                });
            }
        }

        public void OnBecameVisible()
        {
            _hasBecomeVisible = true;
        }

        private void OnBecameInvisible()
        {
            if (IsAlive)
            {
                Despawn();
            }
        }

        #region SPAWN POOL

        private void Despawn()
        {
            PoolBoss.Despawn(this.transform);
        }
        
        private void OnSpawned()
        {
            SetStartState();
        }

        private void OnDespawned()
        {
        }
        
        #endregion SPAWN POOL
        
        #region DEBUG
        
        private void LogPosition()
        {
            // var worldPosition = this.transform.position;
            // var screenPosition = Camera.main.WorldToScreenPoint(worldPosition, Camera.MonoOrStereoscopicEye.Mono);
            // var seaWorldPosition = WorldManager.Instance.ScreenToWorldPosition(screenPosition);
            // DebugLog($"Screen Position: {screenPosition}\n" +
            //          $"Sea World Position: {seaWorldPosition}\n" +
            //          $"World Position: {worldPosition}");
            
            var worldPosition = this.transform.position;
            DebugLog($"World Position: {worldPosition}");
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.SHIP, message, this);
        }
        
        #endregion DEBUG
    }
}