using UnityEngine;
using System.Collections;

namespace _halftheory {
    [RequireComponent(typeof(Camera))]
    public class MouseLook : MonoBehaviour {

        public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
        public RotationAxes axes = RotationAxes.MouseXAndY;
        public float sensitivityX = 5F;
        public float sensitivityY = 5F;

        public float minimumX = -360F;
        public float maximumX = 360F;

        public float minimumY = -60F;
        public float maximumY = 60F;

        float rotationY = 0F;

        public float zoomSpeed = 2f;

        private Vector3 localEulerAngles = Vector3.zero;
        private Vector3 localPosition = Vector3.zero;

        void Start () {
            // Make the rigid body not change rotation
            if (GetComponent<Rigidbody>()) {
                GetComponent<Rigidbody>().freezeRotation = true;
            }
            // store original values for future resets
            localEulerAngles = transform.localEulerAngles;
            localPosition = transform.localPosition;
        }

        void Update () {
            //Zoom in and out with Mouse Wheel
            transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);

            //Look around with Left Mouse
            if (Input.GetMouseButton(0)) {
                if (axes == RotationAxes.MouseXAndY) {
                    float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
                    rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                    rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
                    transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
                }
                else if (axes == RotationAxes.MouseX) {
                    transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
                }
                else {
                    rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                    rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
                    transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
                }
            }
            // reset with right mouse
            else if (Input.GetMouseButton(1)) {
                transform.localEulerAngles = localEulerAngles;
                transform.localPosition = localPosition;
            }
        }

    }
}