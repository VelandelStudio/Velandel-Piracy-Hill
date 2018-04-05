using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// IDestructibleElement interface
/// This interface is used for elements that can be destructed by cannonballs
/// </summary>
public interface IDestructibleElement
{

    void OnElementHit();
    void OnElementTouchWater();
}