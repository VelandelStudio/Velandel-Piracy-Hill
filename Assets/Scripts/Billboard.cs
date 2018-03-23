using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simply class that freeze an element to the main camera in the game
/// </summary>
public class Billboard : MonoBehaviour {

    private void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
