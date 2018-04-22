using UnityEngine;
using System.Collections;

public class DAX_PP_Gravity_L1 : MonoBehaviour 
{
	public float MagnitifyFactor = 0.5f;  
	public bool IsWorldSpace = false;
	public float MagnitifyTimePart = 0.5f;
	public float MagnitifyDistance = 2.0f;
	//Transform TRANSF;
	ParticleSystem PS;

	// Use this for initialization
	void Start () 
	{
		this.PS = this.GetComponent<ParticleSystem>();
		//this.TRANSF = this.transform;
        if (this.PS.main.simulationSpace == ParticleSystemSimulationSpace.World) { this.IsWorldSpace = true; } else { this.IsWorldSpace = false; };
	}
	float PDist( Vector3 inner, Vector3 outer, out Vector3 Vec )
	{
		Vec = (outer - inner);
		return Vec.magnitude;
	}
	// Update is called once per frame
	void Update () 
	{

		ParticleSystem.Particle[] P = new ParticleSystem.Particle[ this.PS.particleCount ];		
		int PC = this.PS.GetParticles( P ); 
		//Vector3 PPos;
		for (int i=0;i<PC;i++)
		{
			if ( (P[i].remainingLifetime / P[i].startLifetime) > MagnitifyTimePart ) { continue; };
			
			Vector3 outFact = new Vector3( 0.0f, 0.0f, 0.0f );
			
			for (int j=0;j<PC;j++)
			{
				if (i==j){ continue;};
				Vector3 vec = new Vector3( 0.0f, 0.0f, 0.0f );
				float dist = PDist( P[i].position, P[j].position, out vec );	
				if ( dist < MagnitifyDistance )
				{			
					outFact += Mathf.Pow( dist, 2 ) * vec / PC;
				}
			}
			
			/*if (IsWorldSpace)
			{
				PPos = this.TRANSF.localPosition - P[i].position;
			}else
			{
				PPos = -P[i].position;
			}*/
			P[i].position = P[i].position + (outFact * Time.deltaTime * MagnitifyFactor);
		}
		this.PS.SetParticles( P, PC);
		
		P = null;
	}
}
