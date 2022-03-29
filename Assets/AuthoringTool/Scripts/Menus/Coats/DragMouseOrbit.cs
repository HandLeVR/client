 using UnityEngine;
 using UnityEngine.EventSystems;
 using UnityEngine.InputSystem;

 /// <summary>
 /// Allows to rotate the camera around the orbit of an object.
 /// 
 /// Source: https://stackoverflow.com/questions/34117591/c-sharp-with-unity-3d-how-do-i-make-a-camera-move-around-an-object-when-user-mo/48997101#48997101
 /// </summary>
 public class DragMouseOrbit : MonoBehaviour
 {
     public Transform target;
     public float distance = 2.0f;
     public float xSpeed = 20.0f;
     public float ySpeed = 20.0f;
     public float yMinLimit = -90f;
     public float yMaxLimit = 90f;
     public float distanceMin = 10f;
     public float distanceMax = 10f;
     public float smoothTime = 2f;
     float rotationYAxis = 0.0f;
     float rotationXAxis = 0.0f;
     float velocityX = 0.0f;
     float velocityY = 0.0f;
     
     void Start()
     {
         Vector3 angles = transform.eulerAngles;
         rotationYAxis = angles.y;
         rotationXAxis = angles.x;
         // Make the rigid body not change rotation
         if (GetComponent<Rigidbody>())
         {
             GetComponent<Rigidbody>().freezeRotation = true;
         }
     }
     
     void LateUpdate()
     {
         if (target)
         {
             float scrollDistance = 0;
             if (Mouse.current.leftButton.isPressed && EventSystem.current.currentSelectedGameObject &&
                 EventSystem.current.currentSelectedGameObject.CompareTag("CoatPreview"))
             {
                 velocityX += xSpeed * Mouse.current.delta.x.ReadValue() * distance * 0.02f;
                 velocityY += ySpeed * Mouse.current.delta.y.ReadValue() * 0.02f;
                 scrollDistance = Mouse.current.scroll.ReadValue().y * 0.001f;
             }

             rotationYAxis += velocityX;
             rotationXAxis -= velocityY;
             rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
             Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
             Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
             Quaternion rotation = toRotation;
 
             distance = Mathf.Clamp(distance - scrollDistance, distanceMin, distanceMax);
             Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
             Vector3 position = rotation * negDistance + target.position;
 
             transform.rotation = rotation;
             transform.position = position;
             velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
             velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
         }
     }
     
     public static float ClampAngle(float angle, float min, float max)
     {
         if (angle < -360F)
             angle += 360F;
         if (angle > 360F)
             angle -= 360F;
         return Mathf.Clamp(angle, min, max);
     }
 }