using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public GameObject cellPrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject obstaclePrefab;

    private List<Vector3> usedPositions = new List<Vector3>();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid(){
    Vector3 startPosition = new Vector3(0, 0, 0);
    Instantiate(startPrefab, startPosition, Quaternion.identity);
    usedPositions.Add(startPosition);

    Vector3 endPosition = new Vector3((width - 1) * cellPrefab.transform.localScale.x, 0, (height - 1) * cellPrefab.transform.localScale.z);
    Instantiate(endPrefab, endPosition, Quaternion.identity);
    usedPositions.Add(endPosition);

    PlaceSpecialCell(obstaclePrefab);
    PlaceSpecialCell(obstaclePrefab);
    PlaceSpecialCell(obstaclePrefab);
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

    void PlaceSpecialCell(GameObject prefab)
    {
        Vector3 position = GetRandomPosition();
        Instantiate(prefab, position, Quaternion.identity);
        usedPositions.Add(position);
    }

    Vector3 GetRandomPosition()
    {
        int x = Random.Range(0, width) * (int)cellPrefab.transform.localScale.x;
        int z = Random.Range(0, height) * (int)cellPrefab.transform.localScale.z;
        Vector3 position = new Vector3(x, 0, z);
        // S'assure que la position n'a pas déjà été choisie
        while (usedPositions.Contains(position))
        {
            x = Random.Range(0, width) * (int)cellPrefab.transform.localScale.x;
            z = Random.Range(0, height) * (int)cellPrefab.transform.localScale.z;
            position = new Vector3(x, 0, z);
        }
        return position;
    }
}
