using Blobcreate.ProjectileToolkit;
using Blobcreate.Universal;
using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Weapons
{
	public class ProjectileLauncher : MonoBehaviour
	{
		public Transform launchPoint;
		public Rigidbody bulletPrefab;
		public LayerMask groundMask;
		public float torqueForce = 5f;
		public float smallA = -0.1f;
		public float bigA = -0.01f;
		public float lerpSpeed = 5f;
		public float reloadTime = 1f;
		public TrajectoryPredictor trajectory;
		public bool drawLine;

		public float fireForce = 10f;
		public float previewLength = 5f;
		public int previewSegments = 16;
		
		float currentA;
		float currentTorque;
		bool isReloading;
		float reloadTimer;

		private TargetObject _target = null;
		public void SetTarget(TargetObject target) { _target = target; }

		void Start()
		{
			currentA = smallA;
		}

		void LateUpdate()
		{
			if (isReloading)
			{
				reloadTimer += Time.deltaTime;

				if (reloadTimer > reloadTime)
				{
					reloadTimer = 0f;
					isReloading = false;
				}
			}

			if (drawLine)
			{
				if (!isReloading)
				{
					// var target = TargetManager.Instance.GetTarget(launchPoint.position, launchPoint.forward);
					if (!_target.IsNullOrDestroyed())
					{
						// Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
						// 	out var hit, 300f, groundMask);
						RenderLaunch(launchPoint.position, _target.GetPosition());
						trajectory.enabled = true;
					}
					else
					{
						trajectory.enabled = false;
					}
				}
				else
				{
					trajectory.enabled = false;
				}
			}
		}

		// private Vector3 PredictTarget()
		// {
		// }

		public void RenderLaunch(Vector3 origin, Vector3 target)
		{
			var v = Projectile.VelocityByA(origin, target, currentA);
			trajectory.Render(origin, v, previewLength, previewSegments);
		}

		public void Fire(Vector3 target)
		{
			var b = Instantiate(bulletPrefab, launchPoint.position, launchPoint.rotation);
			b.GetComponent<ProjectileBehaviour>().Launch(target);

			// Magic happens!
			var f = Projectile.VelocityByA(b.position, target, currentA);
			b.AddForce(f, ForceMode.VelocityChange);

			// Add some torque, not necessary, but interesting.
			var t = Vector3.Lerp(torqueForce * Random.onUnitSphere,
				torqueForce * (target - launchPoint.position).normalized, currentTorque);
			b.AddTorque(t, ForceMode.VelocityChange);
		}

		public void OnFireStart()
		{
			currentA = smallA;
			currentTorque = 0f;
		}

		public void OnFireHold()
		{
			currentA = Mathf.Lerp(currentA, bigA, lerpSpeed * Time.deltaTime);
			currentTorque = Mathf.Lerp(currentTorque, 1f, lerpSpeed * Time.deltaTime);
		}

		public void OnFireStop()
		{
			if (isReloading)
				return;

			// Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 300f, groundMask);
			// var target = TargetManager.Instance.GetTarget(launchPoint.position, launchPoint.forward);
			var targetPosition = _target.IsNullOrDestroyed() ? GetBasicVelocity() : _target.GetPosition();
			Fire(targetPosition);
			isReloading = true;
			currentA = smallA;
			currentTorque = 0f;
		}

		private Vector3 GetBasicVelocity()
		{
			return launchPoint.forward; // * fireForce;
		}
	}
}