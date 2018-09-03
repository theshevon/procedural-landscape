// Script to procedurally generate a terrain for COMP30019 Project 01.
//
// Procedural land Generation using Diamond Square Algorithm referenced from
// code written by Ather Omar:
// (https://www.youtube.com/watch?v=1HV8GbFnCik&t=915s)
//
// Adapted and modified by Shevon Mendis, September 2018.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LandScript : MonoBehaviour{

    public Material material;
    public PointLight pointLight;

    // the land must have width and height of the order 2^n + 1
    // hence both the size of the land and no of divisions must be of the 
    // order 2^n
    private const float sizeOfland = 64; 
    private const int noOfDivisions = 128; 

    private const float maximumHeight = 16;
    private float maxY = -float.MaxValue;
    private float minY = float.MaxValue;

    MeshRenderer landRenderer;

    void Start()
    {
        MeshFilter landMesh = gameObject.AddComponent<MeshFilter>();
        landMesh.mesh = GenerateLand();

        landRenderer = gameObject.AddComponent<MeshRenderer>();
        landRenderer.material = material;

        // add a mesh collider for collision detection
        gameObject.AddComponent<MeshCollider>();
    }

    void Update()
    {
        // Pass updated light positions to shader
        landRenderer.material.SetColor("_PointLightColor", this.pointLight.color);
        landRenderer.material.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
    }

    /// <summary>Generates a mesh for the land.</summary>
    /// <returns>The land mesh.</returns>
    private Mesh GenerateLand()
    {

        Mesh mesh = new Mesh
        {
            name = "Land"
        };

        // the size of the vertices array is (noOfDivisions + 1)^2 to satisfy
        // the (2^n + 1) height-width condition needed to run the diamond square algorithm
        Vector3[] vertices = new Vector3[(noOfDivisions + 1) * (noOfDivisions + 1)];

        Vector2[] UVs = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[noOfDivisions * noOfDivisions * 6];

        // start by creating a plane of the required size
        MakePlane(vertices, UVs, triangles, sizeOfland, noOfDivisions);

        // vary the heights of the vertices
        VaryVertexHeights(vertices);

        // update vertex height details about the land
        UpdateMinAndMaxHeight(vertices);
        
        // ensure that at least a third of the land generated will be below water
        float threshold = -GetHeightOfLand() / 3.0f;
        float heightToMove = threshold - minY;
        OffsetVertices(vertices, heightToMove);
        minY += heightToMove;
        maxY += heightToMove;

        SetColors(colors, vertices);

        // store the arrays in their mesh counter-parts
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary> Varies the y values of the vertices that make up the land </summary>
    /// <param name="vertices">Vertices array.</param>
    private void VaryVertexHeights(Vector3[] vertices)
    {
        // set initial values for the corners of the plane
        vertices[0].y = Random.Range(0, maximumHeight);
        vertices[noOfDivisions].y = Random.Range(-maximumHeight, 0);
        vertices[vertices.Length - 1].y = Random.Range(0, maximumHeight);
        vertices[vertices.Length - 1 - noOfDivisions].y = Random.Range(-maximumHeight, 0);

        // for each square in the array, there will be a diamond and a square 
        // step. Hence the total number of times the diamon-square algorithm
        // will run == max number of squares in the graph, which is equal
        // to log2(no of divisions)
        int noOfRuns = (int)Mathf.Log(noOfDivisions, 2);

        // initially there is only one square, with side length == no of divisions
        int noOfSquares = 1;
        int squareSize = noOfDivisions;
        float height = maximumHeight;

        // run the diamond square algorithm on the array
        for (int i = 0; i < noOfRuns; i++)
        {

            int row = 0;
            for (int j = 0; j < noOfSquares; j++)
            {

                int col = 0;
                for (int k = 0; k < noOfSquares; k++)
                {
                    RunDiamondSquare(vertices, row, col, squareSize, height);
                    col += squareSize;
                }

                row += squareSize;
            }

            // With each run, the number of sqaures doubles as the size
            // of the current square halves
            noOfSquares *= 2;
            squareSize /= 2;

            // vary the height offset 
            height *= 0.5f;
        }
    }

    /// <summary>Makes a plane out of vertices.</summary>
    /// <param name="vertices">Vertices array.</param>
    /// <param name="UVs">UVs array.</param>
    /// <param name="triangles">Triangles array.</param>
    private void MakePlane(Vector3[] vertices, Vector2[] UVs, int[] triangles, float size, int nDivisions){

        float sizeOfDivision = size / nDivisions;
        int index;

        // create a plane
        for (int i = 0; i < nDivisions + 1; i++)
        {
            for (int j = 0; j < nDivisions + 1; j++)
            {

                index = i * (nDivisions + 1) + j;

                // set vertex
                vertices[index] = new Vector3(-(size * 0.5f) + (j * sizeOfDivision), 0.0f, (size * 0.5f) - (i * sizeOfDivision));

                // set uv
                UVs[index] = new Vector2((float)i / nDivisions, (float)j / nDivisions);
            }
        }

        // make triangles out of the vertices
        index = 0;
        for (int i = 0; i < nDivisions; i++)
        {
            for (int j = 0; j < nDivisions; j++)
            {

                // left side triangle of a sqaure unit in plane
                triangles[index++] = i * (nDivisions + 1) + j;
                triangles[index++] = (i + 1) * (nDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (nDivisions + 1) + j;

                // right side triangle of a square in plane
                triangles[index++] = i * (nDivisions + 1) + j;
                triangles[index++] = i * (nDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (nDivisions + 1) + j + 1;
            }
        }

    }

    /// <summary> Runs the diamond square algorithm on an array of vertices. </summary>
    /// <param name="vertices">Vertices array.</param>
    /// <param name="row">Row number.</param>
    /// <param name="col">Column number.</param>
    /// <param name="squareSize">Size of square.</param>
    /// <param name="offset">Height offset.</param>
    private void RunDiamondSquare(Vector3[] vertices, int row, int col, int squareSize, float offset)
    {

        int halfSize = squareSize / 2;

        // find the corners of the square
        int topLeft = row * (noOfDivisions + 1) + col;
        int topRight = topLeft + squareSize;
        int bottomLeft = (row + squareSize) * (noOfDivisions + 1) + col;
        int bottomRight = bottomLeft + squareSize;

        // perform the diamond step - get the midpoint of the square and set its height to be the average of the square's corner vertex heights plus a random value
        int midPoint = (row + halfSize) * (noOfDivisions + 1) + col + halfSize;
        vertices[midPoint].y = (vertices[topLeft].y + vertices[bottomLeft].y + vertices[topRight].y + vertices[bottomRight].y) * 0.25f + Random.Range(-offset, offset);

        // perform a diamond step - set the midpoints of each of the 4 diamonds to be the average of their corner vertex heights plus a random value
        vertices[topLeft + halfSize].y = (vertices[midPoint].y + vertices[topLeft].y + vertices[topRight].y) / 3 + Random.Range(-offset, offset); 
        vertices[midPoint - halfSize].y = (vertices[midPoint].y + vertices[topLeft].y + vertices[bottomLeft].y) / 3 + Random.Range(-offset, offset);
        vertices[midPoint + halfSize].y = (vertices[midPoint].y + vertices[topRight].y + vertices[bottomRight].y) / 3 + Random.Range(-offset, offset);
        vertices[bottomLeft + halfSize].y = (vertices[midPoint].y + vertices[bottomLeft].y + vertices[bottomRight].y) / 3 + Random.Range(-offset, offset);
    }

    /// <summary> Updates the values of the maximum and minimum vertex heights generated </summary>
    /// <param name="vertices">Vertices array.</param>
    private void UpdateMinAndMaxHeight(Vector3[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++){

            float y = vertices[i].y;

            if (y < minY)
            {
                minY = y;
            }

            if (y > maxY)
            {
                maxY = y;
            }
        }
    }

    /// <summary> Returns the actual height of the land generated </summary>
    /// <returns>The height of land.</returns>
    private float GetHeightOfLand(){
        return maxY - minY;
    }

    /// <summary> Moves all the vertices vertically up or down as required.</summary>
    /// <param name="vertices">Vertices array.</param>
    /// <param name="height">Height to offset by.</param>
    private void OffsetVertices(Vector3[] vertices, float height){
        for (int i = 0; i < vertices.Length; i++){
            vertices[i] += new Vector3(0, height, 0);
        }
    }

    /// <summary> Sets the colors for each vertex. </summary>
    /// <param name="colors">Colors array.</param>
    /// <param name="vertices">Vertices array.</param>
    private void SetColors(Color[] colors, Vector3[] vertices){

        for (int i = 0; i < vertices.Length; i++){

            float vertexHeight = vertices[i].y;
            float heightAboveLand = maximumHeight; 

            // highest vertices will be white (to resemble snow)
            if (vertexHeight > 0.8f * heightAboveLand)
            {
                colors[i] = new Color32(255, 255, 255, 1);
            }

            // vary vertex colours from white to brown
            else if (vertexHeight > 0.7f * heightAboveLand)
            {
                colors[i] = Random.Range(0, 10) >= 5 ? (Color)new Color32(255, 255, 255, 1) : (Color)new Color32(26, 13, 0, 1);
            }

            // upper-mid vertices will be brown to show Earth
            else if (vertexHeight > 0.5f * heightAboveLand)
            {
                colors[i] = new Color32(26, 13, 0, 1);
            }

            // vary vertex colours from brown to green
            else if (vertexHeight > 0.4f * heightAboveLand)
            {
                colors[i] = Random.Range(0, 10) >= 5 ? (Color)new Color32(26, 13, 0, 1) : (Color)new Color32(0, 153, 76, 1);
            }

            // most of the vertices will be green to show vegetation
            else if (vertexHeight > 0.1f * heightAboveLand)
            {
                colors[i] = new Color32(0, 153, 76, 1);
            }

            // vary from green to beige
            else if (vertexHeight > 0)
            {
                colors[i] = Random.Range(0, 10) >= 5 ? (Color)new Color32(0, 153, 76, 1) : (Color)new Color32(255, 214, 159, 1);
            }

            // vertices below water will be sand coloured (beige)
            else
            {
                colors[i] = new Color32(255, 214, 159, 1);
            }
        }

    }
}
