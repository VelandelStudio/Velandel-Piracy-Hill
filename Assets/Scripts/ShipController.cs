﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipController : NetworkBehaviour
{
    public Transform barrePlace;
    [SyncVar]
    public NetworkIdentity userId;

    /// <summary>
    /// 
    /// </summary>
    protected void FixedUpdate()
    {
        if (!userId || !userId.isLocalPlayer)
        {
            return;
        }

        float x = Input.GetAxis("Horizontal") * Time.deltaTime * 20.0f;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * 10.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
    }
}