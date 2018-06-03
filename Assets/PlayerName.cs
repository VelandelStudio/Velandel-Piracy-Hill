using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour {

    void OnInstantiate(PhotonMessageInfo info)
    {
        var pView = transform.root.GetComponent<PhotonView>();
        GetComponent<Text>().text = pView.owner.NickName;
    }
}
