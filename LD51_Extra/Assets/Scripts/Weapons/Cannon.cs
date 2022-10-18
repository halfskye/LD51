using Sirenix.OdinInspector;
using UnityEngine;

namespace OldManAndTheSea.Weapons
{
    public class Cannon : MonoBehaviour
    {
        [SerializeField] private Transform _visual = null;
        [SerializeField] private Transform _pivot = null;
        [SerializeField] private Transform _firePoint = null;
        
        [SerializeField, MinMaxSlider(-90f, 90f)] private Vector2 _horizontalExtents = Vector2.zero;
        [SerializeField, MinMaxSlider(-90f, 90f)] private Vector2 _verticalExtents = Vector2.zero;

        [SerializeField] private float _horizontalSpeed = 1f;
        [SerializeField] private float _verticalSpeed = 1f;

        [SerializeField] private Transform _cannonballPrefab = null;
        [SerializeField] private float _cannonForce = 10f;

        [SerializeField] private ProjectileLauncher _projectileLauncher = null;
        
        private Vector2 _aimAxis = Vector2.zero;
        private Vector2 _activeEulerXY = Vector2.zero;

        private Ship _owner = null;

        private TargetObject _target = null;

        public void SetTarget(TargetObject target)
        {
            _target = target;
            _projectileLauncher.SetTarget(_target);
        }
        
        private void Awake()
        {
            _owner = this.GetComponentInParent<Ship>();
        }

        private void Update()
        {
            UpdateAim();
        }
        
        private void UpdateAim()
        {
            Aim(_aimAxis);
        }

        public void Aim(Vector2 aim)
        {
            _aimAxis = aim;
            
            var newX = _activeEulerXY.x + _horizontalSpeed * aim.x * Time.deltaTime;
            newX = Mathf.Clamp(newX, _horizontalExtents.x, _horizontalExtents.y);
            var deltaX = newX - _activeEulerXY.x;
            _activeEulerXY.x = newX;
            
            var newY = _activeEulerXY.y + _verticalSpeed * aim.y * Time.deltaTime;
            newY = Mathf.Clamp(newY, -_verticalExtents.y, -_verticalExtents.x);
            var deltaY = newY - _activeEulerXY.y;
            _activeEulerXY.y = newY;

            var pivot = _pivot.position;
            this.transform.RotateAround(pivot, this.transform.right, deltaY);
            this.transform.RotateAround(pivot, _pivot.up, deltaX);
        }

        public void Fire_Start()
        {
            _projectileLauncher.OnFireStart();
        }
        public void Fire_Hold()
        {
            _projectileLauncher.OnFireHold();
        }
        public void Fire_Stop()
        {
            _projectileLauncher.OnFireStop();
        }
        
        // public void Fire()
        // {   
        //     // _projectileLauncher.Fire(_firePoint.forward * _cannonForce);
        //     
        //     // var cannonballXform = PoolBoss.SpawnInPool(_cannonballPrefab, _firePoint.position, _firePoint.rotation);
        //     // var cannonball = cannonballXform.GetComponent<Cannonball>();
        //     // var fireForce = _firePoint.forward * _cannonForce;
        //     // cannonball.Fire(_owner, fireForce);
        // }
    }
}