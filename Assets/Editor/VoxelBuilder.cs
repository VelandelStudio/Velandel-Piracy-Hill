using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VoxelBuilder : MonoBehaviour
{

    GameObject objSelected;

    private void Update()
    {
        objSelected = Selection.activeGameObject;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            GameObject newObj = SpawnObject();
            newObj.transform.localPosition += Vector3.forward;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            GameObject newObj = SpawnObject();
            newObj.transform.localPosition -= Vector3.forward;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GameObject newObj = SpawnObject();
            newObj.transform.localPosition += Vector3.right;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GameObject newObj = SpawnObject();
            newObj.transform.localPosition -= Vector3.right;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject newObj = SpawnObject();
            newObj.transform.localPosition += Vector3.up;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            GameObject newObj = SpawnObject();
            newObj.transform.localPosition -= Vector3.up;
        }
    }

    private GameObject SpawnObject()
    {
        GameObject newObj = Instantiate(objSelected, objSelected.transform.position, objSelected.transform.rotation);
        newObj.name = objSelected.name;

        if (objSelected.transform.parent != null)
        {
            newObj.transform.SetParent(objSelected.transform.parent);
        }
        Selection.SetActiveObjectWithContext(newObj, null);

        return newObj;
    }

    private void DestroyMiddle ()
    {
        Transform[] sb = GetComponentsInChildren<Transform>();
        for (int i = 0; i < sb.Length; i++)
        {
            /*if(sb[i].gameObject.name == "PontsRight")
            {
                continue;
            }*/

            if (sb[i].transform.localPosition.x == 0)
            {
                DestroyImmediate(sb[i].gameObject);
            }
        }
    }
}