using UnityEngine;

/** IInterractable Interface
 * This Interface was created to make a gameObject interractable.
 * Currently, the Interfac contains 3 methods and 1 property.
 * The property is used to tell if the interractable is Activable or not.
 * The methods are used to tell what happens when you activate the Interractable and how does it acts with the GUI Elements.
 **/
public interface IInterractable {
	
	bool IsActivable{get;}

    void ActivateInterractable(Collider other);
    void DisplayTextOfInterractable();
    void CancelTextOfInterractable();
}
