// Script to generate the Sun by modeling it as a point light source that will
// move over the landscape for COMP30019 Project 01.
//
// Written by Wenzhen Yue, September 2018.

using UnityEngine;
using System.Collections;

public class PointLight : MonoBehaviour {

    public Color color = Color.white;
    public float sunSpeed = 3;
    public Material material;

    private float timeElapsed = 0;
    private GameObject sun;

    void Start() {

        // Make a sphere around the point light source to make it look like 
        // the sun.
        sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sun.GetComponent<MeshRenderer>().material = material;
        sun.transform.parent = this.transform;
    }

    void Update() {

        timeElapsed += Time.deltaTime;

        // Move the sun over the landscape.
        this.transform.position = new Vector3(75 * Mathf.Cos(timeElapsed * sunSpeed / 75),
                                              75 * Mathf.Sin(timeElapsed * sunSpeed / 75),
                                              0.0f);
    }

    public Vector3 GetWorldPosition()
    {
        return this.transform.position;
    }


}
