using UnityEngine;
using System.Collections;

public class PointLight : MonoBehaviour {

    public Color color;
    public float sunSpeed;
    private float time;

    void Start() {
        time = 0;
    }

    private void Update() {
        time += Time.deltaTime;
        this.transform.position = new Vector3(100 * Mathf.Cos(Mathf.PI / 2.0f + time * sunSpeed / 100),
                                              100 * Mathf.Sin(Mathf.PI / 2.0f + time * sunSpeed / 100),
                                              0.0f);
    }

    public Vector3 GetWorldPosition()
    {
        return this.transform.position;
    }


}
