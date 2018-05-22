using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] Transform VoxelBody;
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.RotateAround(VoxelBody.position, Vector3.up, Input.GetAxis("Mouse X"));
    }
}