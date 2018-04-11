using UnityEngine;
using emotitron.Network.NST;
using emotitron.Utilities.GUIUtilities;

/// <summary>
/// Basic automatic transform mover for objects for network testing. Will only run if object has local authority.
/// </summary>
public class Mover : NSTComponent
{
	public enum TType { Position, Rotation, Scale }
	public enum Axis { X = 1, Y = 2, Z = 4 }

	public TType changeWhat = TType.Rotation;
	public Vector3 addVector = new Vector3(0, 0, 0);
	private Vector3 initialPos;
	private Vector3 initialRot;
	private Vector3 initialScl;

	[Help("Oscillate overrides the addVector and will instead lerp between the two range values.")]
	public bool oscillate;

	[EnumMask]
	public Axis oscillateAxis = Axis.X;

	public float oscillateStart;
	public float oscillateEnd;
	private float oscillateRange;
	private float oscillateHalfRange;

	public float oscillateRate;

	private Rigidbody rb;

	private void Start()
	{
		if (MasterNetAdapter.NetworkLibrary == NetworkLibrary.UNET && !nst.na.IsMine)
		{
			Destroy(this);
		}

		initialPos = transform.localPosition;
		initialRot = transform.localEulerAngles;
		initialScl = transform.localScale;

		rb = GetComponent<Rigidbody>();

		oscillateRange = oscillateEnd - oscillateStart;
		oscillateHalfRange = oscillateRange * .5f;

	}

	void Update ()
	{
		if (!nst.na.IsMine)
			return;

		if (oscillate)
		{
			float val = ((Mathf.Sin(Time.time * oscillateRate) + 1)) * oscillateHalfRange + oscillateStart;

			//Vector3 currentv3 =
			//(changeWhat == TType.Position) ? transform.localPosition :
			//(changeWhat == TType.Rotation) ? transform.localRotation.eulerAngles :
			//transform.localScale;

			Vector3 newv3 = new Vector3
				(
				((oscillateAxis & Axis.X) != 0) ? val : 0,
				((oscillateAxis & Axis.Y) != 0) ? val : 0,
				((oscillateAxis & Axis.Z) != 0) ? val : 0);

			if (changeWhat == TType.Rotation)
				transform.localRotation = Quaternion.Euler(initialRot + newv3);

			else if (changeWhat == TType.Position)
			{
				if (transform.parent == null)
					if (rb)
						rb.MovePosition(initialPos + newv3);
					else
						transform.position = initialPos + newv3;
				else
					transform.localPosition = initialPos + newv3;
			}

			else
				transform.localScale = initialScl + newv3; ;
		}

		else
		{
			if (changeWhat == TType.Rotation)
				transform.localRotation *= Quaternion.Euler(addVector * Time.deltaTime);

			else if (changeWhat == TType.Position)
				transform.localPosition += addVector * Time.deltaTime;

			else
				transform.localScale += addVector * Time.deltaTime;
		}

		



	}
}
