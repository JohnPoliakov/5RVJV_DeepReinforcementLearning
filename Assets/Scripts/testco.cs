using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testco : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
            StartCoroutine(count(0));
        
    }

    IEnumerator count(int i)
    {
        
        Debug.Log(i);
        yield return new WaitForSeconds(1);
        StartCoroutine(count(i + 1));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
