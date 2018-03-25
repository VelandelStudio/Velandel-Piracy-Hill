using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/** ActivableMechanism, public abstract class
 * @extends : Mechanism
 * This class represents mechanism that shoudl be activated by the player. These mechanisms can not activate themselves.
 **/
public abstract class ActivableMechanism : Mechanism
{
    private bool _activating;
    private GameObject _entityActivator;
    private Collider _entityActivatorCol;
    /** AttributeMechanismObject, protected override void Method
	 * Launched by the base.Start, we instantiate as a child GameObject an ActivableMechanism that has a ActivableMechanismDetector.
	 * We notify the ActivableMechanismDetector that we are the Mechanism associated to it.
	 **/
    protected override void AttributeMechanismObject()
    {
        mechanismObject = (GameObject)Resources.Load("Mechanisms/ActivableMechanism");
        mechanismObject = Instantiate(mechanismObject, transform);
        mechanismObject.GetComponent<ActivableMechanismDetector>().SetInetrractableParent(this);
    }

    /** DisplayTextOfInterractable, public override void 
	 * We tell to the player which button to press to activate the mechanism
	 **/
    public override void DisplayTextOfInterractable()
    {
        //Debug.Log("Press " + InputsProperties.Activate.ToString() + " to activate.");
    }

    /** CancelTextOfInterractable, public override void 
	 * Does nothing currently but will handle the Behaviour of the GUI.
	 **/
    public override void CancelTextOfInterractable(Collider other) { }

    /** ActivateInterractable, public override abstract void 
	 * The behaviour of the ActivableMechanism will be set in the child script.
	 **/
    public override void OnActivation(Collider other)
    {
        _entityActivator = other.gameObject;
        _entityActivatorCol = other;
        _activating = true;
    }

    protected virtual void Update()
    {
        if (_activating)
        {
            Vector3 lookDir = transform.position - _entityActivator.transform.position;
            lookDir.y = 0;
            Quaternion q = Quaternion.LookRotation(lookDir);
            _entityActivator.transform.rotation = Quaternion.RotateTowards(_entityActivator.transform.rotation, q, Time.deltaTime * 500f);
            //Camera.main.GetComponent<CameraController>().CameraControlled = false;

            if (Vector3.Angle(_entityActivator.transform.forward, lookDir) < 10)
            {
                //Camera.main.GetComponent<CameraController>().CameraControlled = true;
                _activating = false;
                ActivateInterractable(_entityActivatorCol);
            }
        }
    }
    /** ActivateInterractable, public override abstract void 
	 * The behaviour of the ActivableMechanism will be set in the child script.
	 **/
    public override abstract void ActivateInterractable(Collider other);
}
