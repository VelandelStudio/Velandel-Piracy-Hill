using UnityEngine;
using emotitron.Network.NST;

/// <summary>
/// Rotates and object if it has as NetworkSyncTransform on it, and if it has authority. Used for demonstration only.
/// </summary>
public class Rotator : NSTComponent, INstPostUpdate
{
	public float speed = 20;
	public float timePassed;

	// Runs on the NST Update, so that this doesn't disable.
	public void OnNstPostUpdate()
	{
		timePassed += Time.deltaTime;
		// Only objects with authority should be moving things.
		if (this != null && na != null && na.IsMine)
			transform.localEulerAngles = new Vector3(0, 0, timePassed * speed % 360);
		else
			Destroy(this);
	}
}
