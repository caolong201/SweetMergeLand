using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace MyGameNamespace {

    public class CameraMove : MonoBehaviour {

        public float movementSpeed = 10.0f; // Speed of camera movement
        public float rotationSpeed = 100.0f; // Speed of camera rotation
        public float verticalSpeed = 5.0f; // Speed of vertical camera movement

        void Start() {
            // Hide the mouse cursor when the game is playing
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update () {
            // Move camera based on keyboard input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 translation = new Vector3(horizontalInput, 0, verticalInput) * movementSpeed * Time.deltaTime;
            transform.Translate(translation, Space.Self);

            // Rotate camera based on mouse input
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.left, mouseY, Space.Self);

            // Move camera up or down based on keyboard input
            if (Input.GetKey(KeyCode.Q)) {
                transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.E)) {
                transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime);
            }
        }
    }
}