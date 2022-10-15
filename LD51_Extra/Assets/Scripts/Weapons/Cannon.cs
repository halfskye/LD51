﻿using DarkTonic.PoolBoss;
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
        
        private Vector2 _activeEulerXY = Vector2.zero;

        public void Aim(Vector2 aim)
        {
            var newX = _activeEulerXY.x + _horizontalSpeed * aim.x * Time.deltaTime;
            newX = Mathf.Clamp(newX, _horizontalExtents.x, _horizontalExtents.y);
            var deltaX = newX - _activeEulerXY.x;
            _activeEulerXY.x = newX;
            
            var newY = _activeEulerXY.y + _verticalSpeed * aim.y * Time.deltaTime;
            newY = Mathf.Clamp(newY, -_verticalExtents.y, -_verticalExtents.x);
            var deltaY = newY - _activeEulerXY.y;
            _activeEulerXY.y = newY;

            var pivot = _pivot.position;
            _visual.RotateAround(pivot, _visual.right, deltaY);
            _visual.RotateAround(pivot, _pivot.up, deltaX);
        }
        
        public void Fire()
        {
            var cannonball = PoolBoss.SpawnInPool(_cannonballPrefab, _firePoint.position, _firePoint.rotation);
            var fireForce = _firePoint.forward * _cannonForce;
            var rigidBody = cannonball.GetComponent<Rigidbody>();
            rigidBody.AddForce(fireForce, ForceMode.Impulse);
        }
    }
}