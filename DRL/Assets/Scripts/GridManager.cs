using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public GameObject cellPrefab; // Prefab pour une cellule basique
    public GameObject startPrefab; // Prefab pour le point de départ
    public GameObject endPrefab; // Prefab pour le point d'arrivée
    public GameObject obstaclePrefab; // Prefab pour les obstacles

    private List<Vector3> usedPositions = new List<Vector3>(); // Liste pour stocker les positions déjà utilisées
    private Vector3 startPosition;
    private Vector3 endPosition;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        startPosition = PlaceSpecialCell(startPrefab);
        endPosition = PlaceEndCell(endPrefab);
        PlaceSpecialCell(obstaclePrefab); 
        PlaceSpecialCell(obstaclePrefab);
        PlaceSpecialCell(obstaclePrefab);
        PlaceSpecialCell(obstaclePrefab);

        for (int x = 0; x < width * cellPrefab.transform.localScale.x; x += (int)cellPrefab.transform.localScale.x)
        {
            for (int z = 0; z < height * cellPrefab.transform.localScale.z; z += (int)cellPrefab.transform.localScale.z)
            {
                Vector3 position = new Vector3(x, 0, z);
                if (!usedPositions.Contains(position))
                {
                    Instantiate(cellPrefab, position, Quaternion.identity);
                }
            }
        }
    }

    Vector3 PlaceSpecialCell(GameObject prefab)
    {
        Vector3 position = GetRandomPosition();
        Instantiate(prefab, position, Quaternion.identity);
        usedPositions.Add(position);
        return position;
    }

    Vector3 PlaceEndCell(GameObject prefab)
    {
        Vector3 position;
        do
        {
            position = GetRandomPosition();
        }
        while (ManhattanDistance(startPosition, position) < 5);
        Instantiate(prefab, position, Quaternion.identity);
        usedPositions.Add(position);
        return position;
    }

    Vector3 GetRandomPosition()
    {
        int x = Random.Range(0, width) * (int)cellPrefab.transform.localScale.x;
        int z = Random.Range(0, height) * (int)cellPrefab.transform.localScale.z;
        Vector3 position = new Vector3(x, 0, z);
        while (usedPositions.Contains(position))
        {
            x = Random.Range(0, width) * (int)cellPrefab.transform.localScale.x;
            z = Random.Range(0, height) * (int)cellPrefab.transform.localScale.z;
            position = new Vector3(x, 0, z);
        }
        return position;
    }

    int ManhattanDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Abs((int)(a.x - b.x)) + Mathf.Abs((int)(a.z - b.z));
    }
}
