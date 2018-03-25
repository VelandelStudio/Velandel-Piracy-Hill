using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/** Mechanism, public abstract class, 
 * @implements : IInterractable
 * This class is the mother class of all Mechanism in the game. Mechanisms are GamObjects that interracts with the player
 * They can be activated by the player or can active themselves in different conditions.
 * A mechanism always works with a GameObject (mechanismObject) that has a ActivableMechanismDetector attached to it.
 **/
public abstract class Mechanism : NetworkBehaviour, IInterractable
{
    protected GameObject mechanismObject;
    protected bool isActivable = true;
    public virtual bool IsActivable
    {
        get { return isActivable; }
        protected set { }
    }

    /** Awake protected virtual void
	 * Launches the Abstract method AttributeMechanismObject. We ensure that child scripts will always attribut a ActivableMechanismDetector object to this script.
	 **/
    protected virtual void Awake()
    {
        AttributeMechanismObject();
    }

    /** AttributeMechanismObject, protected abstract void
	 * This method should  be override in child scripts 
	 * We use it to instantiate a childGameObject to the Mechanism wich handles the behaviour to activate it.
	 **/
    protected abstract void AttributeMechanismObject();

    /** ActivateInterractable, public abstract void
     * @param : Collider
     * This method should  be override in child scripts 
	 * This method is used to tell to the mechanism that is it activated by a ActivableMechanismDetector.
	 * You should override here the behaviour of your mechanism when it is activated.
	 **/
    public abstract void ActivateInterractable(Collider other);

    /** DisplayTextOfInterractable, public virtual void
	 * Used to display elements on the screen when we are close enough to the mechanism
	 **/
    public virtual void DisplayTextOfInterractable() { }


    public virtual void CancelTextOfInterractable() { }
    /** CancelTextOfInterractable, public virtual void
	 * Used to disable elements on the screen when we are not close enough to the mechanism
	 **/
    public virtual void CancelTextOfInterractable(Collider other) { }



    public abstract void OnActivation(Collider other);
}
