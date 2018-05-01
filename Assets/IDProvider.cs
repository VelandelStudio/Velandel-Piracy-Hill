using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IDProvider  {

    private static int currentID = 0;

    public static void BuildIDFor(GameObject obj, bool isMine)
    {
        if (isMine)
        {
            currentID++;
            obj.name += currentID.ToString();
        }
    }
}
