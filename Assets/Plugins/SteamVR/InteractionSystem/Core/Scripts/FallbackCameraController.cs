//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Controls for the non-VR debug camera
//
//=============================================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Camera ) )]
	public class FallbackCameraController : MonoBehaviour
	{
		public float speed = 4.0f;
		public float shiftSpeed = 16.0f;
		public bool showInstructions = true;

		private Vector3 startEulerAngles;
		private Vector3 startMousePosition;
		private float realTime;

		//-------------------------------------------------
		void OnEnable()
		{
			realTime = Time.realtimeSinceStartup;
		}


		//-------------------------------------------------
		void Update()
		{
			float forward = 0.0f;
			if ( Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
			{
				forward += 1.0f;
			}
			if ( Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed )
			{
				forward -= 1.0f;
			}

            float up = 0.0f;
            if (Keyboard.current.eKey.isPressed)
            {
                up += 1.0f;
            }
            if (Keyboard.current.qKey.isPressed)
            {
                up -= 1.0f;
            }

            float right = 0.0f;
			if ( Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed )
			{
				right += 1.0f;
			}
			if ( Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed )
			{
				right -= 1.0f;
			}

			float currentSpeed = speed;
			if ( Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
			{
				currentSpeed = shiftSpeed;
			}

			float realTimeNow = Time.realtimeSinceStartup;
			float deltaRealTime = realTimeNow - realTime;
			realTime = realTimeNow;

			Vector3 delta = new Vector3( right, up, forward ) * currentSpeed * deltaRealTime;

			// BEGIN CHANGE
			// transform.position += transform.TransformDirection( delta );
			Vector3 newDir = transform.TransformDirection(delta);
			if (Physics.Raycast(transform.position, newDir, out var hit, newDir.magnitude, LayerMask.GetMask("PaintBooth")))
				transform.position = hit.point + hit.normal * 0.01f;
			else
				transform.position += newDir;
			// END CHANGE

			Vector3 mousePosition = Mouse.current.position.ReadValue();

			if ( Mouse.current.rightButton.wasPressedThisFrame /* right mouse */)
			{
				startMousePosition = mousePosition;
				startEulerAngles = transform.localEulerAngles;
			}

			if ( Mouse.current.rightButton.isPressed /* right mouse */)
			{
				Vector3 offset = mousePosition - startMousePosition;
				transform.localEulerAngles = startEulerAngles + new Vector3( -offset.y * 360.0f / Screen.height, offset.x * 360.0f / Screen.width, 0.0f );
			}
		}


		//-------------------------------------------------
		void OnGUI()
		{
			if ( showInstructions )
			{
				GUI.Label( new Rect( 10.0f, 10.0f, 600.0f, 400.0f ),
				"WASD EQ/Arrow Keys to translate the camera\n" +
					"Right mouse click to rotate the camera\n" +
					"Left mouse click for standard interactions.\n" );
			}
		}
	}
}
