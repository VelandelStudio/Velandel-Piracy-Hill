using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/** ActivableMechanismDetector, public class
 * This script is associated to a gameObject that represents the behaviour of an Activable Mechanism before its activation.
 * We handle here what happens when the player is close enough the objet or when he is too far.
 **/
public class ActivableMechanismDetector : MonoBehaviour
{
    protected Mechanism interractable;

    /** Start private void Method
	 * On Start, we Get the SphereCollider associated and resize it to fit with the max bound size.
	 **/
    private void Start()
    {
        interractable = GetComponentInParent<Mechanism>();
        SphereCollider col = GetComponent<SphereCollider>();
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        MeshFilter[] filters = interractable.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter f in filters)
        {
            bounds.Encapsulate(f.sharedMesh.bounds);
        }

        float maxBound = Mathf.Max(bounds.max.x, Mathf.Max(bounds.max.y, bounds.max.z));
        float maxLossyScale = 0f;

        if (maxBound == bounds.max.x)
        {
            maxLossyScale = interractable.transform.localScale.x;
        }

        if (maxBound == bounds.max.y)
        {
            maxLossyScale = interractable.transform.localScale.y;
        }

        if (maxBound == bounds.max.z)
        {
            maxLossyScale = interractable.transform.localScale.z;
        }
        col.radius = maxBound + (2 * maxLossyScale);
        col.center = bounds.center;
    }

    /** OnTriggerEnter, private void
	 * @param Collider
	 * When a player enters inside the Trigger, if the mechanism is Activable, we call the DisplayTextOfInterractable method inside the Mechanism
	 **/
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NetworkIdentity>())
        {
            NetworkIdentity otherID = other.GetComponent<NetworkIdentity>();
            if (otherID.isLocalPlayer)
            {
                interractable.DisplayTextOfInterractable();
                //otherID.GetComponent<MechanismHandler>().CmdNotifyInsideMechanism(GetComponentInParent<NetworkIdentity>());
            }
        }
    }

    /** OnTriggerExit, private void
	 * @param Collider
	 * When a player goes outside the Trigger, if the mechanism is Activable, we call the CancelTextOfInterractable method inside the Mechanism
	 **/
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<NetworkIdentity>())
        {
            NetworkIdentity otherID = other.GetComponent<NetworkIdentity>();
            if (otherID.isLocalPlayer)
            {
                interractable.CancelTextOfInterractable();
               // otherID.GetComponent<MechanismHandler>().CmdNotifyOutsideMechanism(GetComponentInParent<NetworkIdentity>());
            }
        }
    }
}