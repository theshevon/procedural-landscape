using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScript : MonoBehaviour
{
    public Material material;
    private float sizeOfTerrain = LandScript.sizeOfTerrain;
    private int noOfDivisions = LandScript.noOfDivisions/2;
    private float maxWaveHeight = 1.0f;
    private MeshFilter terrainMesh;
    private float totalTime = 0;
    private float timeToWait = 0.5f;
    private float timeWaited = float.MaxValue;
  
    // Use this for initialization
    void Start(){
        
        terrainMesh = gameObject.AddComponent<MeshFilter>();
        terrainMesh.mesh = generateWater();

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material.shader = Shader.Find("Unlit/WaterShader");
    }

    private Mesh generateWater()
    {

        Mesh mesh = new Mesh();
        mesh.name = "Water";

        //There will be n+1 vertices for there to be n divisions
        Vector3[] vertices = new Vector3[(noOfDivisions + 1) * (noOfDivisions + 1)]; // + 4];
        Vector2[] UVs = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];

        int[] triangles = new int[noOfDivisions * noOfDivisions * 6];

        float sizeOfDivision = sizeOfTerrain / noOfDivisions;

        int index;
        for (int i = 0; i < noOfDivisions + 1; i++)
        {
            for (int j = 0; j < noOfDivisions + 1; j++)
            {
                index = i * (noOfDivisions + 1) + j;

                //Set vertices
                vertices[index] = new Vector3(-(sizeOfTerrain * 0.5f) + (j * sizeOfDivision), 0.0f, (sizeOfTerrain * 0.5f) - (i * sizeOfDivision));
                UVs[index] = new Vector2((float)i / noOfDivisions, (float)j / noOfDivisions);
                colors[index] = new Color32(46, 154, 248, 1);
            }
        }


        //Make triangles
        index = 0;
        for (int i = 0; i < noOfDivisions ; i++)
        {
            for (int j = 0; j < noOfDivisions; j++)
            {
                triangles[index++] = i * (noOfDivisions + 1) + j;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j;

                triangles[index++] = i * (noOfDivisions + 1) + j;
                triangles[index++] = i * (noOfDivisions + 1) + j + 1;
                triangles[index++] = (i + 1) * (noOfDivisions + 1) + j + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;

    }

    //void Update()
    //{
    //    float delta = Time.deltaTime;
    //    totalTime += delta;
    //    timeWaited += delta;

    //    if (timeWaited >= timeToWait){

    //        timeWaited = 0;

    //        Vector3[] vertices = terrainMesh.mesh.vertices;

    //        int noOfSteps = (int)Mathf.Log(noOfDivisions, 2);
    //        int noOfSquares = 1;
    //        int squareSize = noOfDivisions;
    //        float height = maxWaveHeight;

    //        for (int i = 0; i < noOfSteps; i++)
    //        {

    //            int row = 0;
    //            for (int j = 0; j < noOfSquares; j++)
    //            {

    //                int col = 0;
    //                for (int k = 0; k < noOfSquares; k++)
    //                {
    //                    runDiamondSquare(vertices, row, col, squareSize, height);
    //                    col += squareSize;
    //                }

    //                row += squareSize;
    //            }

    //            noOfSquares *= 2;
    //            squareSize /= 2;
    //            height *= 0.5f;
    //        }

    //        terrainMesh.mesh.vertices = vertices;
    //    }

    //}

    //private void runDiamondSquare(Vector3[] vertices, int row, int col, int squareSize, float offset)
    //{

    //    //Find square corners
    //    int halfSize = squareSize / 2;
    //    int topLeft = row * (noOfDivisions + 1) + col;
    //    int topRight = topLeft + squareSize;
    //    int bottomLeft = (row + squareSize) * (noOfDivisions + 1) + col;
    //    int bottomRight = bottomLeft + squareSize;

    //    /* Perform diamond step - Get midpoint and set its height to be the average of the heights of the square corners plus a random value */
    //    int midPoint = (row + halfSize) * (noOfDivisions + 1) + col + halfSize;
    //    vertices[midPoint].y = (vertices[topLeft].y + vertices[bottomLeft].y + vertices[topRight].y + vertices[bottomRight].y) * 0.25f + Random.Range(-offset, offset);

    //    /* Perform square step - Find midpoints of the square's edges and set their heights to be the average of the adjacent corners and midpoint plus a random
    //    value*/

    //    //TopMid
    //    vertices[topLeft + halfSize].y = (vertices[midPoint].y + vertices[topLeft].y + vertices[topRight].y) / 3 + Random.Range(-offset, offset);
    //    //LeftMid
    //    vertices[midPoint - halfSize].y = (vertices[midPoint].y + vertices[topLeft].y + vertices[bottomLeft].y) / 3 + Random.Range(-offset, offset);
    //    //RightMid
    //    vertices[midPoint + halfSize].y = (vertices[midPoint].y + vertices[topRight].y + vertices[bottomRight].y) / 3 + Random.Range(-offset, offset);
    //    //BottomMid
    //    vertices[bottomLeft + halfSize].y = (vertices[midPoint].y + vertices[bottomLeft].y + vertices[bottomRight].y) / 3 + Random.Range(-offset, offset);

    //}


}

