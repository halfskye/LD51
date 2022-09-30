using UnityEngine;
using System;
using Opertoon.Panoply;
#if (ENABLE_INPUT_SYSTEM)
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif
using UnityEngine.UI;

/**
 * The PanoplyController class handles user input globally.
 * Copyright © Erik Loyer
 * erik@opertoon.com
 * Part of the Panoply engine
 */

namespace Opertoon.Panoply {

	public enum AxisGesturePhase
	{
		None,
		Began,
		InProgress,
		Ended
	}

	public enum PanoplyGesture {
		SwipeUp,
		SwipeRight,
		SwipeDown,
		SwipeLeft
	}

	public class PanoplyController: MonoBehaviour {

		public Text text;

		public int swipeTouchCount = 1;						// How many touches are required to trigger a swipe
	    public float gestureTriggerDist = 25.0f;			// Distance a gesture must traverse before triggering a new step
	    public float gestureRate = 5.0f;					// How quickly gestures are executed
	    public bool passiveInput = true;					// Whether passive input is accepted
		public bool keyboardInput = true;                   // Whether keyboard input is accepted
		public bool ignoreGesturesOutsideOfPanels = false;
		public bool ignoreStepCount = false;
		public DeviceType deviceType;

	    [HideInInspector]
	    public float horizontalTilt;
	    
	    [HideInInspector]
	    public float verticalTilt;
	    
	    [HideInInspector]
	    public PanoplyGesture lastGesture;
	    
	    Vector2 gestureStart;
	    bool ignoreCurrentGesture = false;

		[HideInInspector]
		public Vector2 gyroCenter = new Vector2 (0, 30);
		[HideInInspector]
		public Vector2 gyroRange = new Vector2 (335, 10);
		public Vector2 accelerometerRange = new Vector2 (.2f, .5f);
		public float smoothing = .5f;
	    
	    Vector2 acceleration = new Vector2(0.0f,0.0f);
		Vector2 lastAxisPosition;

		[HideInInspector]
		public Vector2 screenPosition;
	    
	    Panel[] panels;
	    
	    PanoplyEventManager eventManager;

		private Vector2 _accelerometerMin = new Vector2 (-.3f, -.25f);

#if (ENABLE_INPUT_SYSTEM)
		private InputActionAsset _inputActions;
		private InputAction _screenPositionAction;
#endif
		private bool _gotInput;
		private int _priorTargetStep;
		private bool _screenContactIsHappening;
		private Vector2 _tilt;

	    public void Start() {
	    
	    	panels = FindObjectsOfType( typeof( Panel ) ) as Panel[];
	    	
	    	eventManager = GetComponent<PanoplyEventManager>();

#if (ENABLE_INPUT_SYSTEM)
			_inputActions = GetComponent<PlayerInput> ().actions;
			_screenPositionAction = _inputActions.FindAction("Panoply/ScreenPosition");
			if (PanoplyCore.passiveInputType == PassiveInputType.Accelerometer) {
				InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
			}
#endif

			deviceType = SystemInfo.deviceType;

	    }

		private Quaternion GyroToUnity(Quaternion q)
        {
			return new Quaternion(q.x, q.y, -q.z, -q.w);
        }
	       
	    /**
	     * Returns the current value of the specified input.
	     *
	     * @param	input		The input type to query.
	     */
	    public void UpdatePassiveInput() {

			if (passiveInput) {
				switch (PanoplyCore.passiveInputType) {

				case PassiveInputType.Gyroscope:
#if (ENABLE_INPUT_SYSTEM)
					Quaternion attitude = UnityEngine.InputSystem.AttitudeSensor.current.attitude.ReadValue();
					float angleDiff = (attitude.eulerAngles.y - gyroCenter.x + 180) % 360 - 180;
					_tilt.x = -(Mathf.InverseLerp (-gyroRange.x, gyroRange.x, angleDiff) * 2 - 1);
					angleDiff = (attitude.eulerAngles.x - (360 - gyroCenter.y) + 180) % 360 - 180;
					_tilt.y = Mathf.InverseLerp (-gyroRange.y, gyroRange.y, angleDiff) * 2 - 1;
#elif (ENABLE_LEGACY_INPUT_MANAGER)
					Quaternion attitude = GyroToUnity(Input.gyro.attitude);
					float angleDiff = (attitude.eulerAngles.y - gyroCenter.x + 180) % 360 - 180;
					_tilt.x = Mathf.InverseLerp (-gyroRange.x, gyroRange.x, angleDiff) * 2 - 1;
					angleDiff = (attitude.eulerAngles.x - gyroCenter.y + 180) % 360 - 180;
					_tilt.y = -(Mathf.InverseLerp (-gyroRange.y, gyroRange.y, angleDiff) * 2 - 1);
#endif
					verticalTilt = Mathf.Lerp(verticalTilt, _tilt.y, Time.deltaTime * (1.0f / smoothing));
					horizontalTilt = Mathf.Lerp(horizontalTilt, _tilt.x, Time.deltaTime * (1.0f / smoothing));
					break;

				case PassiveInputType.Accelerometer:
					Vector3 inputAcceleration = new Vector3();
#if (ENABLE_INPUT_SYSTEM)
					inputAcceleration = UnityEngine.InputSystem.Accelerometer.current.acceleration.ReadValue();
#elif (ENABLE_LEGACY_INPUT_MANAGER)
					inputAcceleration = Input.acceleration;
#endif
					if (inputAcceleration.x < _accelerometerMin.x)
					{
						_accelerometerMin.x = Mathf.Min(_accelerometerMin.x, inputAcceleration.x);
					}
					else if (inputAcceleration.x > (_accelerometerMin.x + accelerometerRange.x))
					{
						_accelerometerMin.x = inputAcceleration.x - accelerometerRange.x;
					}
					if (inputAcceleration.y < _accelerometerMin.y)
					{
						_accelerometerMin.y = Mathf.Min(_accelerometerMin.y, inputAcceleration.y);
					}
					else if (inputAcceleration.y > (_accelerometerMin.y + accelerometerRange.y))
					{
						_accelerometerMin.y = inputAcceleration.y - accelerometerRange.y;
					}
					acceleration.x = -((Mathf.InverseLerp(_accelerometerMin.x + accelerometerRange.x, _accelerometerMin.x, inputAcceleration.x) * 2.0f) - 1.0f);
					acceleration.y = (Mathf.InverseLerp(_accelerometerMin.y, _accelerometerMin.y + accelerometerRange.y, inputAcceleration.y) * 2.0f) - 1.0f;
					if (inputAcceleration.z < 0)
					{
						acceleration *= -1.0f;
					}
					verticalTilt = Mathf.Lerp (verticalTilt, acceleration.y, Time.deltaTime * (1.0f / smoothing));
					horizontalTilt = Mathf.Lerp (horizontalTilt, acceleration.x, Time.deltaTime * (1.0f / smoothing));
					break;

				case PassiveInputType.Mouse:
#if (ENABLE_INPUT_SYSTEM)
					horizontalTilt = (Mathf.Clamp ((screenPosition.x / Screen.width), 0.0f, 1.0f) - 0.5f) * 2.0f;
					verticalTilt = (Mathf.Clamp ((screenPosition.y / Screen.height), 0.0f, 1.0f) - 0.5f) * 2.0f;
#elif (ENABLE_LEGACY_INPUT_MANAGER)
					horizontalTilt = (Mathf.Clamp ((Input.mousePosition.x / Screen.width), 0.0f, 1.0f) - 0.5f) * 2.0f;
					verticalTilt = (Mathf.Clamp ((Input.mousePosition.y / Screen.height), 0.0f, 1.0f) - 0.5f) * 2.0f;
#endif
					break;

				}
	    	} else {
	    		horizontalTilt = 0.0f;
	    		verticalTilt = 0.0f;
	    	}
	    	
	    }  
	    
	    /**
	     * Returns true if the specified point falls within the bounds of any
	     * frame that is set to intercept interactions.
	     *
	     * @param point					The point to test.
	     * @param includeEmptySpace		Should empty space also be considered an interaction intercept?
	     * @return						True if the point is contained by an intercepting frame.
	     */
	    bool PointCannotBeUsedForNavigation( Vector2 point ) {
	    
	    	bool pointInAnyInterceptingFrame = false;
			bool pointInAnyNonInterceptingFrame = false;
	    	int i = 0;
	    	int n = 0;
	    	
	    	n = panels.Length;
	    	Panel panel = null;
	    	for( i = 0; i < n; i++ ) {
	    		panel = panels[ i ];
				if ((panel.camera.enabled && panel.camera.pixelRect.Contains( point )) || PanoplyCore.scene.disableNavigation) {
					if (panel.interceptInteraction) {
						pointInAnyInterceptingFrame = true;
					} else {
						pointInAnyNonInterceptingFrame = true;
                    }
	    		}
	    	}

			if (ignoreGesturesOutsideOfPanels) {
				return pointInAnyInterceptingFrame || !pointInAnyNonInterceptingFrame;
            } else {
				return pointInAnyInterceptingFrame;
            }
		}

#if (ENABLE_INPUT_SYSTEM)
		public void OnScreenPosition (InputAction.CallbackContext context)
		{
			screenPosition = context.ReadValue<Vector2> ();
		}

		public void OnScreenContact(InputAction.CallbackContext context)
        {
			if (!context.canceled) {
				HandleScreenContactBegin ();
            } else {
				HandleScreenContactEnd ();
            }
        }
#endif

		private void HandleScreenContactBegin()
        {
#if (ENABLE_INPUT_SYSTEM)
			screenPosition = gestureStart = _screenPositionAction.ReadValue<Vector2> ();
#endif

			// if the click is contained by a frame set to intercept interactions, or if the menu is open, then ignore this gesture
			if (!PointCannotBeUsedForNavigation (screenPosition) && !PanoplyCore.menu.isOpen) {
				_gotInput = true;
				ignoreCurrentGesture = false;
				_screenContactIsHappening = true;

			} else if (PanoplyCore.menu.isOpen) {
				if (!PanoplyCore.menu.ScreenPointIsInsideMenu (screenPosition)) {
					PanoplyCore.menu.SetMenuOpen (false);
				}
				ignoreCurrentGesture = true;
			}
		}

		private void HandleScreenContactInProgress ()
		{
			if (!ignoreCurrentGesture) {
				Vector2 gestureDelta = Vector2.zero;
				float gestureDeltaMultiplier = 1.0f;

				_gotInput = true;

				// in case the down phase gets skipped
				if (gestureStart.x == -1 || gestureStart == Vector2.zero) {
					gestureStart = screenPosition;
				}

				gestureDelta = screenPosition - gestureStart;

				if (Mathf.Abs (gestureDelta.x) > Mathf.Abs (gestureDelta.y)) {
					if (gestureDelta.x > 0) {
						gestureDeltaMultiplier = -1.0f;
						lastGesture = PanoplyGesture.SwipeRight;
					} else {
						lastGesture = PanoplyGesture.SwipeLeft;
					}
				} else {
					if (gestureDelta.y < 0) {
						gestureDeltaMultiplier = -1.0f;
						lastGesture = PanoplyGesture.SwipeDown;
					} else {
						lastGesture = PanoplyGesture.SwipeUp;
					}
				}

				PanoplyCore.interpolatedStep = Mathf.Lerp (PanoplyCore.interpolatedStep, PanoplyCore.targetStep + Mathf.Clamp (gestureDelta.magnitude * gestureDeltaMultiplier, -gestureTriggerDist, gestureTriggerDist) / gestureTriggerDist, Time.deltaTime * gestureRate);
			}
		}

		private void HandleScreenContactEnd()
        {
			_screenContactIsHappening = false;

			if (!ignoreCurrentGesture) {

				_gotInput = true;

				_priorTargetStep = PanoplyCore.targetStep;
				if ((PanoplyCore.interpolatedStep - PanoplyCore.targetStep) > .07f) {
					if (ignoreStepCount) {
						PanoplyCore.targetStep++;
					} else {
						PanoplyCore.targetStep = Mathf.Min (PanoplyCore.targetStep + 1, PanoplyCore.scene.stepCount - 1);
					}
					if (PanoplyCore.targetStep != _priorTargetStep) {
						eventManager.HandleTargetStepChanged (_priorTargetStep, PanoplyCore.targetStep);
					}

				} else if ((PanoplyCore.targetStep - PanoplyCore.interpolatedStep) > .07f) {
					if (ignoreStepCount) {
						PanoplyCore.targetStep--;
					} else {
						PanoplyCore.targetStep = Mathf.Max (PanoplyCore.targetStep - 1, 0);
					}
					if (PanoplyCore.targetStep != _priorTargetStep) {
						eventManager.HandleTargetStepChanged (_priorTargetStep, PanoplyCore.targetStep);
					}
				}

				gestureStart = new Vector2 (-1.0f, -1.0f);

			}
		}

		public void Update() {
	    
	    	Vector2 gestureDelta = Vector2.zero;
	    	_gotInput = false;
	    	Vector2 averagedPosition = Vector2.zero;
			Vector2 axisPosition = Vector2.zero;
	    	
	    	UpdatePassiveInput();

#if (ENABLE_INPUT_SYSTEM)

		if (_screenContactIsHappening) {
			HandleScreenContactInProgress();
		}

#elif (ENABLE_LEGACY_INPUT_MANAGER)

	    	Touch touch = new Touch();
	    	float gestureDeltaMultiplier = 1.0f;
			switch (deviceType) {

			case DeviceType.Console:
			case DeviceType.Unknown:
				axisPosition.x = Input.GetAxis ("Horizontal");
				axisPosition.y = Input.GetAxis ("Vertical");
				AxisGesturePhase gesturePhase = AxisGesturePhase.None;

				if (axisPosition.x != 0 || axisPosition.y != 0) {
					if (lastAxisPosition.x == 0 && lastAxisPosition.y == 0) {
						gesturePhase = AxisGesturePhase.Began;
					} else {
						gesturePhase = AxisGesturePhase.InProgress;
					}
				} else {
					if (lastAxisPosition.x != 0 || lastAxisPosition.y != 0) {
						gesturePhase = AxisGesturePhase.Ended;
					}
				}

				switch (gesturePhase) {

				case AxisGesturePhase.Began:
					_gotInput = true;
					gestureStart = axisPosition;
					ignoreCurrentGesture = false;
					break;

				case AxisGesturePhase.InProgress:
					if (!ignoreCurrentGesture) {

						_gotInput = true;
						gestureDelta = axisPosition - gestureStart;

						if (Mathf.Abs (gestureDelta.x) > Mathf.Abs (gestureDelta.y)) {
							if (gestureDelta.x > 0) {
								gestureDeltaMultiplier = -1.0f;
								lastGesture = PanoplyGesture.SwipeRight;
							} else {
								lastGesture = PanoplyGesture.SwipeLeft;
							}
						} else {
							if (gestureDelta.y < 0) {
								gestureDeltaMultiplier = -1.0f;
								lastGesture = PanoplyGesture.SwipeDown;
							} else {
								lastGesture = PanoplyGesture.SwipeUp;
							}
						}

						PanoplyCore.interpolatedStep = Mathf.Lerp (PanoplyCore.interpolatedStep, PanoplyCore.targetStep + Mathf.Clamp (gestureDelta.magnitude * gestureDeltaMultiplier, -.5f, .5f) / .5f, Time.deltaTime * gestureRate);
					}
					break;

				case AxisGesturePhase.Ended:
					if (!ignoreCurrentGesture) {
						_gotInput = true;
						_priorTargetStep = PanoplyCore.targetStep;
						if ((PanoplyCore.interpolatedStep - PanoplyCore.targetStep) > .07f) {
							if (ignoreStepCount) {
								PanoplyCore.targetStep++;
							} else {
								PanoplyCore.targetStep = Mathf.Min (PanoplyCore.targetStep + 1, PanoplyCore.scene.stepCount - 1);
							}
							if (PanoplyCore.targetStep != _priorTargetStep) {
								eventManager.HandleTargetStepChanged (_priorTargetStep, PanoplyCore.targetStep);
							}

						} else if ((PanoplyCore.targetStep - PanoplyCore.interpolatedStep) > .07f) {
							if (ignoreStepCount) {
								PanoplyCore.targetStep--;
							} else {
								PanoplyCore.targetStep = Mathf.Max (PanoplyCore.targetStep - 1, 0);
							}
							if (PanoplyCore.targetStep != _priorTargetStep) {
								eventManager.HandleTargetStepChanged (_priorTargetStep, PanoplyCore.targetStep);
							}
						}

						gestureStart = new Vector2 (-1.0f, -1.0f);
					}
					break;
				}

				lastAxisPosition = axisPosition;
				break;

			case DeviceType.Desktop:			
				screenPosition = (Vector2)Input.mousePosition;

				// if the mouse button was clicked, then record its location
				if (Input.GetMouseButtonDown (0)) {
					HandleScreenContactBegin();

					// if the mouse button was released, then calculate our new step index
				} else if (Input.GetMouseButtonUp (0)) {
					HandleScreenContactEnd();

					// if the mouse is currently being dragged, recalculate the current gesture progress based on the distance the mouse has moved
				} else if (Input.GetMouseButton (0)) {
					HandleScreenContactInProgress();
				}
				if (keyboardInput) {
					if (Input.GetKeyDown ("left")) {
						PanoplyCore.DecrementStep (ignoreStepCount);
					} else if (Input.GetKeyDown ("right")) {
						PanoplyCore.IncrementStep (ignoreStepCount);
					} else if (Input.GetKeyDown ("up")) {
						if (Application.isEditor) {
							PanoplyCore.GoToFirstStep ();
						}
					} else if (Input.GetKeyDown ("down")) {
						if (Application.isEditor) {
							PanoplyCore.GoToLastStep ();
						}
					}
				}
				break;

			case DeviceType.Handheld:
				if (Input.touchCount == swipeTouchCount) {

					UnityEngine.TouchPhase dominantPhase = UnityEngine.TouchPhase.Canceled;

					screenPosition = Vector2.zero;
					for (int i = 0; i < swipeTouchCount; i++) {
						touch = Input.touches [i];
						screenPosition += Input.touches [i].position;
						if (dominantPhase == UnityEngine.TouchPhase.Canceled) {
							if (touch.phase == UnityEngine.TouchPhase.Began) {
								dominantPhase = touch.phase;
							} else if (touch.phase == UnityEngine.TouchPhase.Ended) {
								dominantPhase = touch.phase;
							}
						} else if (dominantPhase == UnityEngine.TouchPhase.Ended) {
							if (touch.phase == UnityEngine.TouchPhase.Began) {
								dominantPhase = touch.phase;
							}
						}
					}
					screenPosition /= swipeTouchCount;

					// if the touch is starting, then record its location
					if (dominantPhase == UnityEngine.TouchPhase.Began) {
						HandleScreenContactBegin();

					// if the touch is ending, then calculate our new step index
					} else if (dominantPhase == UnityEngine.TouchPhase.Ended) {
						HandleScreenContactEnd();

						// otherwise, recalculate the current gesture progress based on distance the touch has moved
					} else {
						HandleScreenContactInProgress();
					}
				}
				break;

			}
			
#endif


			if (!ignoreStepCount && PanoplyCore.scene != null) {
				PanoplyCore.interpolatedStep = Mathf.Clamp(PanoplyCore.interpolatedStep, 0, PanoplyCore.scene.stepCount - 1);
			}
	    	
	    	// No input; head for target step
	    	if ( !_gotInput ) {
	    		PanoplyCore.interpolatedStep = Mathf.Lerp( PanoplyCore.interpolatedStep,  ( float )PanoplyCore.targetStep, Time.deltaTime * gestureRate);
	    	}
	    
	    }
	}
}
