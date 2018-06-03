using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour {

    void OnInstantiate(PhotonMessageInfo info)
    {
        var pView = GetComponentInParent<PhotonView>();
        if (pView.isMine)
        {
            gameObject.SetActive(false);
            return;
        }

        GetComponent<InputField>().text = pView.owner.NickName;
    }
}
