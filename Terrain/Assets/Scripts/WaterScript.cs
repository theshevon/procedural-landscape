// Script to generate a plane that will represent the ocean surface for 
// COMP30019 Project 01.
//
// Written by Shevon Mendis, September 2018.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScript : MonoBehaviour
{
    public Material material;
    public PointLight pointLight;

    private const float sizeOfSurface = 64;
    private const int noOfDivisions = 128;

    MeshRenderer waterRenderer;

    void Start()
    {
        MeshFilter waterMesh = gameObject.AddComponent<MeshFilter>();
        waterMesh.mesh = GenerateWater();

        waterRenderer = gameObject.AddComponent<MeshRenderer>();
        waterRenderer.material = material;

        // Add a mesh collider for collision detection.
        gameObject.AddComponent<MeshCollider>();
    }

    void Update()
    {
        // Pass updated point light positions to shader.
        waterRenderer.material.SetColor("_PointLightColor", this.pointLight.color);
        waterRenderer.material.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
    }

    private Mesh GenerateWater()
    {

        Mesh mesh = new Mesh
        {
            name = "Water"
        };

        // For there to be n divisions, there must be n+1 vertices per side.
        Vector3[] vertices = new Vector3[(noOfDivisions + 1) * (noOfDivisions + 1)];
        Vector2[] UVs = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[noOfDivisions * noOfDivisions * 6];

        // Create a flat plane.
        float sizeOfDivision = sizeOfSurface / noOfDivisions;
        int index;
        for (int i = 0; i < noOfDivisions + 1; i++){
            for (int j = 0; j < noOfDivisions + 1; j++){

                index = i * (noOfDivisions + 1) + j;

                // Set vertex position, uv, colour.
                vertices[index] = new Vector3(-(sizeOfSurface * 0.5f) + (j * sizeOfDivision), 0, (sizeOfSurface * 0.5f) - (i * sizeOfDivision));
                UVs[index] = new Vector2((float)i / noOfDivisions, (float)j / noOfDivisions);
                colors[index] = new Color32(46, 154, 248, 1);
            }
        }

        // Make triangles out of the vertices.
        index = 0;
        for (int i = 0; i < noOfDivisions; i++){
            for (int j = 0; j < noOfDivisions; j++){

                triangles[index++] = i * (noOfDivisions + 1) + j;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j;

                triangles[index++] = i * (noOfDivisions + 1) + j;
                triangles[index++] = i * (noOfDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j + 1;
            }
        }

        // Store the arrays in their mesh counter-parts.
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;

    }
}