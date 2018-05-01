using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities.SmartVars;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] Transform VoxelBody;
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.RotateAround(VoxelBody.position, Vector3.up, Input.GetAxis("Mouse X"));
    }
}