using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public int MouseSensitivity;
    public int CameraSpeed;

    // initial camera rotation values
    private float horizontal = 20.0f;
    private float vertical = 30.0f;

    //initial camera position values
    private float zPosition = -5.0f;
    private float yPosition = 30.0f;
    private float xPosition = 0.0f;

    private float sizeOfTerrain;

    // Use this for initialization
    void Start () {
        // get size of terrain from TerrainScript
        sizeOfTerrain = 64;
        // set initial camera rotation
        this.transform.localRotation = Quaternion.Euler(vertical, horizontal, 0.0f);
        // set initial camera position
        this.transform.localPosition = new Vector3(xPosition, yPosition, zPosition);
    }

    // Update is called once per frame
    void FixedUpdate () {
        Ray forward = new Ray(transform.position, transform.forward);
        Ray right = new Ray(transform.position, transform.right);
        Ray back = new Ray(transform.position, -transform.forward);
        Ray left = new Ray(transform.position, -transform.right);
        // update horizontal rotation value
        horizontal += MouseSensitivity * Input.GetAxis("Mouse X");
        // update vertical rotation value
        vertical -= MouseSensitivity * Input.GetAxis("Mouse Y");
        // restrict vertical rotation values
        vertical = Mathf.Clamp(vertical, -90.0f, 90.0f);
        // rotate camera
        transform.eulerAngles = new Vector3(vertical, horizontal, 0.0f);

        // move camera
        if (Input.GetKey("w") && !Physics.SphereCast(forward, 0.5f, 1.0f))
            transform.Translate(Vector3.forward * Time.deltaTime * CameraSpeed);
        if (Input.GetKey("s") && !Physics.SphereCast(back, 0.5f, 1.0f))
            transform.Translate(Vector3.back * Time.deltaTime * CameraSpeed);
        if (Input.GetKey("a") && !Physics.SphereCast(left, 0.5f, 1.0f))
            transform.Translate(Vector3.left * Time.deltaTime * CameraSpeed);
        if (Input.GetKey("d") && !Physics.SphereCast(right, 0.5f, 1.0f))
            transform.Translate(Vector3.right * Time.deltaTime * CameraSpeed);

        // restrict camera boundaries
        zPosition = Mathf.Clamp(transform.position.z, -sizeOfTerrain/2, sizeOfTerrain/2);
        yPosition = Mathf.Clamp(transform.position.y, 0.0f, 1000.0f);
        xPosition = Mathf.Clamp(transform.position.x, -sizeOfTerrain/2, sizeOfTerrain/2);

        this.transform.localPosition = new Vector3(xPosition, yPosition, zPosition);

    }
}
