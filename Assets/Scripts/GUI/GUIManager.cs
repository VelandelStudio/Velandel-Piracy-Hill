using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour {

    [SerializeField] private GameObject ship0;
    [SerializeField] private GameObject ship1;
    [HideInInspector] public GUIHandler localPlayerObj;

    public void JoinTeamAction(int i)
    {
        GameObject ship = i == 0 ? ship0 : ship1;
        localPlayerObj.GetComponent<GUIHandler>().CmdAttributeTeam(ship);
    }
}
