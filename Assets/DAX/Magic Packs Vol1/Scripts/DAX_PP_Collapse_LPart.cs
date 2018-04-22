using UnityEngine;
using System.Collections;

public class DAX_PP_Collapse_LPart : MonoBehaviour 
{
	public float MagnitifyFactor = 0.5f;  
	public bool IsWorldSpace = false;
	public float MagnitifyTimePart = 0.5f;
	Transform TRANSF;
	ParticleSystem PS;

	// Use this for initialization
	void Start () 
	{
		this.PS = this.GetComponent<ParticleSystem>();
		this.TRANSF = this.transform;
		if ( this.PS.main.simulationSpace==ParticleSystemSimulationSpace.World) { this.IsWorldSpace = true;} else { this.IsWorldSpace = false; };
	}
	
	// Update is called once per frame
	void Update () 
	{

		ParticleSystem.Particle[] P = new ParticleSystem.Particle[ this.PS.particleCount ];		
		int PC = this.PS.GetParticles( P ); 
		Vector3 PPos;
		for (int i=0;i<PC;i++)
		{
			if ( (P[i].remainingLifetime / P[i].startLifetime) < MagnitifyTimePart ) { continue; };
			
			if (IsWorldSpace)
			{
				PPos = this.TRANSF.localPosition - P[i].position;
			}else
			{
				PPos = -P[i].position;
			}
			P[i].position = P[i].position + (PPos * Time.deltaTime * MagnitifyFactor);
		}
		this.PS.SetParticles( P, PC);
		
		P = null;
	}
}
