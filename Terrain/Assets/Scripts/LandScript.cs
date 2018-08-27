using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandScript : MonoBehaviour
{
    public float sizeOfTerrain = 64;
    public int noOfDivisions = 128;
    public float maximumHeight = 20;

    private float minHeightOfLand = 20;
    private float maxHeightOfLand = -20;

    public Material material;

    // Use this for initialization
    void Start()
    {
        MeshFilter terrainMesh = gameObject.AddComponent<MeshFilter>();
        terrainMesh.mesh = generateTerrain();

        MeshRenderer landRenderer = gameObject.AddComponent<MeshRenderer>();
        landRenderer.material = material;

        gameObject.AddComponent<MeshCollider>();

    }

    private Mesh generateTerrain()
    {

        Mesh mesh = new Mesh();
        mesh.name = "Terrain";

        //There will be n+1 vertices for there to be n divisions
        Vector3[] vertices = new Vector3[(noOfDivisions + 1) * (noOfDivisions + 1)];
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
                colors[index] = Color.green;
            }
        }


        //Make triangles
        index = 0;
        for (int i = 0; i < noOfDivisions; i++)
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

        //Set values for corners
        vertices[0].y = Random.Range(0, maximumHeight);
        vertices[noOfDivisions].y = Random.Range(-maximumHeight, 0);
        vertices[vertices.Length - 1].y = Random.Range(0, maximumHeight);
        vertices[vertices.Length - 1 - noOfDivisions].y = Random.Range(-maximumHeight, 0);


        int noOfSteps = (int)Mathf.Log(noOfDivisions, 2);
        int noOfSquares = 1;
        int squareSize = noOfDivisions;
        float height = maximumHeight;

        for (int i = 0; i < noOfSteps; i++)
        {

            int row = 0;
            for (int j = 0; j < noOfSquares; j++)
            {

                int col = 0;
                for (int k = 0; k < noOfSquares; k++)
                {
                    runDiamondSquare(vertices, row, col, squareSize, height);
                    col += squareSize;
                }

                row += squareSize;
            }

            noOfSquares *= 2;
            squareSize /= 2;
            height *= 0.5f;
        }

        for (int i = 0; i < noOfDivisions; i++)
        {
            updateMinAndMaxHeight(vertices[i].y);
        }

        //Set colors
        //for (int i = 0; i < noOfDivisions+1; i++){
        //    for (int j = 0; j < noOfDivisions+1; j++)
        //    {
        //        index = i * (noOfDivisions + 1) + j;

        //        float vertexHeight = vertices[index].y;
        //        if (vertexHeight > maximumHeight - getHeightOfLand() / 12){
        //            colors[index] = new Color32(255, 255, 255, 1);
        //        }
        //        else if (vertexHeight > maximumHeight - getHeightOfLand() / 9){
        //            if (Random.Range(0, 10) >= 5)
        //            {
        //                colors[index] = new Color32(255, 255, 255, 1);
        //            }
        //            else
        //            {
        //                colors[index] = new Color32(105, 74, 16, 1);
        //            }
        //        }
        //        else if (vertexHeight > maximumHeight - getHeightOfLand() / 6){
        //            colors[index] = new Color32(105, 74, 16, 1);
        //        }
        //        else if (vertexHeight > maximumHeight - getHeightOfLand() / 4){
        //            if (Random.Range(0, 10) >= 5){
        //                colors[index] = new Color32(105, 74, 16, 1);
        //            } else{
        //                colors[index] = new Color32(0, 153, 76, 1);
        //            }
        //        }
        //        else if (vertexHeight > maximumHeight - getHeightOfLand() / 2) { 
        //            colors[index] = new Color32(0, 153, 76, 1);
        //        }else if (vertexHeight > 0){
        //            if (Random.Range(0, 10) >= 5)
        //            {
        //                colors[index] = new Color32(0, 153, 76, 1);
        //            }
        //            else
        //            {
        //                colors[index] = new Color32(255, 214, 159, 1);
        //            }
        //        }else{
        //            colors[index] = new Color32(255, 214, 159, 1);
        //        }
        //    }
        //}

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;

    }

    private void runDiamondSquare(Vector3[] vertices, int row, int col, int squareSize, float offset)
    {

        //Find square corners
        int halfSize = squareSize / 2;
        int topLeft = row * (noOfDivisions + 1) + col;
        int topRight = topLeft + squareSize;
        int bottomLeft = (row + squareSize) * (noOfDivisions + 1) + col;
        int bottomRight = bottomLeft + squareSize;

        /* Perform diamond step - Get midpoint and set its height to be the average of the heights of the square corners plus a random value */
        int midPoint = (row + halfSize) * (noOfDivisions + 1) + col + halfSize;
        vertices[midPoint].y = (vertices[topLeft].y + vertices[bottomLeft].y + vertices[topRight].y + vertices[bottomRight].y) * 0.25f + Random.Range(-offset, offset);

        /* Perform square step - Find midpoints of the square's edges and set their heights to be the average of the adjacent corners and midpoint plus a random
        value*/

        //TopMid
        vertices[topLeft + halfSize].y = (vertices[midPoint].y + vertices[topLeft].y + vertices[topRight].y) / 3 + Random.Range(-offset, offset);
        //LeftMid
        vertices[midPoint - halfSize].y = (vertices[midPoint].y + vertices[topLeft].y + vertices[bottomLeft].y) / 3 + Random.Range(-offset, offset);
        //RightMid
        vertices[midPoint + halfSize].y = (vertices[midPoint].y + vertices[topRight].y + vertices[bottomRight].y) / 3 + Random.Range(-offset, offset);
        //BottomMid
        vertices[bottomLeft + halfSize].y = (vertices[midPoint].y + vertices[bottomLeft].y + vertices[bottomRight].y) / 3 + Random.Range(-offset, offset);

    }

    private void updateMinAndMaxHeight(float y)
    {
        if (y < minHeightOfLand)
        {
            minHeightOfLand = y;
        }
        else if (y > maxHeightOfLand)
        {
            maxHeightOfLand = y;
        }
    }


    public float getMinHeightOfLand()
    {
        return minHeightOfLand;
    }

 
    public float getTotalHeightOfLand()
    {
        return maxHeightOfLand - minHeightOfLand;
    }
}
