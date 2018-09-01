// Original code for Procedural Terrain Generation using Diamond Square 
// Algorithm written by Ather Omar:
// (https://www.youtube.com/watch?v=1HV8GbFnCik&t=915s)
//
// Adapted and modified for COMP30019 Project 01 by Shevon Mendis - 868551

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandScript : MonoBehaviour{

    public Material material;
    public PointLight pointLight;

    // the terrain must have width and height of the order 2^n + 1
    // hence both the size of the terrain and no of divisions must be of the 
    // order 2^n
    private const float sizeOfTerrain = 64; 
    private const int noOfDivisions = 128; 

    private const float maximumHeight = 20;
    private float maxY = 20;
    private float minY = -20;

    MeshRenderer landRenderer;

    void Start()
    {
        MeshFilter terrainMesh = gameObject.AddComponent<MeshFilter>();
        terrainMesh.mesh = GenerateTerrain();

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

    private Mesh GenerateTerrain()
    {

        Mesh mesh = new Mesh();
        mesh.name = "Terrain";

        // the size of the vertices array is (noOfDivisions + 1)^2 to satisfy
        // the (2^n + 1) height-width condition needed to run the diamond square algorithm
        Vector3[] vertices = new Vector3[(noOfDivisions + 1) * (noOfDivisions + 1)];

        Vector2[] UVs = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[noOfDivisions * noOfDivisions * 6];

        float sizeOfDivision = sizeOfTerrain / noOfDivisions;
        int index;

        // Initially, create a plane
        for (int i = 0; i < noOfDivisions + 1; i++)
        {
            for (int j = 0; j < noOfDivisions + 1; j++)
            {

                index = i * (noOfDivisions + 1) + j;

                // set vertex
                vertices[index] = new Vector3(-(sizeOfTerrain * 0.5f) + (j * sizeOfDivision), 0.0f, (sizeOfTerrain * 0.5f) - (i * sizeOfDivision));

                // set uv
                UVs[index] = new Vector2((float)i / noOfDivisions,
                                         (float)j / noOfDivisions);

                // set colour
                colors[index] = Color.green;
            }
        }

        // make triangles out of the vertices
        index = 0;
        for (int i = 0; i < noOfDivisions; i++)
        {
            for (int j = 0; j < noOfDivisions; j++)
            {

                // left side triangle of sqaure unit in plane
                triangles[index++] = i * (noOfDivisions + 1) + j;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j;

                // right side triangle of square in plane
                triangles[index++] = i * (noOfDivisions + 1) + j;
                triangles[index++] = i * (noOfDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j + 1;
            }
        }

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

        // run the diamond square algorithm for each diamond and square in the array
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

        // update height details about the terrain
        for (int i = 0; i < noOfDivisions; i++)
        {
            UpdateMinAndMaxHeight(vertices[i].y);
        }

        // ensure that at least a third of the land generated will be below water
        float threshold = GetTotalHeightOfLand()*0.33f;
        if (minY > -threshold){
            float heightToMove = -(minY + threshold);
            OffsetVertices(vertices, heightToMove);
        }
        else if (maxY < threshold){
            float heightToMove = minY - threshold;
            OffsetVertices(vertices, heightToMove);
        }

        // set colors
        for (int i=0; i < colors.Length; i++){
            colors[i] = vertices[i].y < 0 ? new Color32(255, 214, 159, 1) : new Color32(105, 74, 16, 1);
        }

        // store the arrays in their mesh counter-parts
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;

    }

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


    /// updates the values of the maximum and minimum vertex heights generated
    private void UpdateMinAndMaxHeight(float y)
    {
        if (y < minY){
           minY = y;
        }
        else if (y >maxY){
           maxY = y;
        }
    }

    /// returns the actual height of the terrain generated
    private float GetTotalHeightOfLand(){
        return maxY - minY;
    }

    /// moves all the vertices vertically up or down as required
    private void OffsetVertices(Vector3[] vertices, float height){
        for (int i = 0; i < vertices.Length; i++){
            vertices[i] += new Vector3(0, height, 0);
        }
    }
}
