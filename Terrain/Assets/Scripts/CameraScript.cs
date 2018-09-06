// Script to control camera based on keyboard/ mouse input for COMP30019
// Project 01.
//
// Written by Brendan Leung, September 2018.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public int MouseSensitivity = 10;
    public int CameraSpeed = 10;

    // Initial camera rotation values.
    private float horizontal = 20.0f;
    private float vertical = 30.0f;

    // Initial camera position values.
    private float zPosition = 0.0f;
    private float yPosition = 30.0f;
    private float xPosition = 0.0f;

    private const float sizeOfTerrain = 64;

    void Start () {
      
        // Set initial camera rotation and position.
        this.transform.localRotation = Quaternion.Euler(vertical, horizontal, 0.0f);
        this.transform.localPosition = new Vector3(xPosition, yPosition, zPosition);
    }

    void FixedUpdate () {

        Ray forward = new Ray(transform.position, transform.forward);
        Ray right = new Ray(transform.position, transform.right);
        Ray back = new Ray(transform.position, -transform.forward);
        Ray left = new Ray(transform.position, -transform.right);

        // Update horizontal and vertical rotation values.
        horizontal += MouseSensitivity * Input.GetAxis("Mouse X");
        vertical -= MouseSensitivity * Input.GetAxis("Mouse Y");

        // Restrict vertical rotation values.
        vertical = Mathf.Clamp(vertical, -90.0f, 90.0f);
        // Rotate camera.
        transform.eulerAngles = new Vector3(vertical, horizontal, 0.0f);

        // Move camera.
        if (Input.GetKey("w") && !Physics.SphereCast(forward, 0.5f, 1.0f))
            transform.Translate(Vector3.forward * Time.deltaTime * CameraSpeed);
        if (Input.GetKey("s") && !Physics.SphereCast(back, 0.5f, 1.0f))
            transform.Translate(Vector3.back * Time.deltaTime * CameraSpeed);
        if (Input.GetKey("a") && !Physics.SphereCast(left, 0.5f, 1.0f))
            transform.Translate(Vector3.left * Time.deltaTime * CameraSpeed);
        if (Input.GetKey("d") && !Physics.SphereCast(right, 0.5f, 1.0f))
            transform.Translate(Vector3.right * Time.deltaTime * CameraSpeed);

        // Restrict camera boundaries.
        zPosition = Mathf.Clamp(transform.position.z, -sizeOfTerrain/2, sizeOfTerrain/2);
        yPosition = Mathf.Clamp(transform.position.y, 0.0f, 1000.0f);
        xPosition = Mathf.Clamp(transform.position.x, -sizeOfTerrain/2, sizeOfTerrain/2);

        this.transform.localPosition = new Vector3(xPosition, yPosition, zPosition);
    }
}
