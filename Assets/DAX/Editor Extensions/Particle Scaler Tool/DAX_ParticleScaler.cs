// DAzBjax (2015), if you have same questions contact me at DAzBjax.Unity@mail.ru 
// 12.01.2016 - added ability to filter by child 

using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR 
using UnityEditor; 
#endif

#if UNITY_EDITOR 
[ExecuteInEditMode]
public class DAX_ParticleScaler : EditorWindow 
{
	const string MenuItemSTR = "Tools/Particle Scaler tool";
	const string TabCaptionSTR = "Ps Scale";
	
	float Scale = 1.0f;
	bool ScaleGameObjects = true; //scale game objects without PS component
	bool ScaleObjectHierarchy = true; //scale local space position for all game objects
	bool ScaleShapeModule = true; //scale shape module for PS
	bool MakeClones = true; //generate new objects
	
	
	Object[] findedObjects;
	bool[] exportedObjects;
	

	//const bool tOnlyROOT = true;//DO NOT EDIT THIS PARAM  | FINER PROGRAM 
	Vector2 scrollVector;
	
	byte[] pngBytesEYE = new byte[] {137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 19, 0, 0, 0, 19, 8, 6, 0, 0, 0, 114, 80, 54, 204, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 11, 19, 0, 0, 11, 19, 1, 0, 154, 156, 24, 0, 0, 0, 32, 99, 72, 82, 77, 0, 0, 122, 37, 0, 0, 128, 131, 0, 0, 249, 255, 0, 0, 128, 232, 0, 0, 82, 8, 0, 1, 21, 88, 0, 0, 58, 151, 0, 0, 23, 111, 215, 90, 31, 144, 0, 0, 2, 76, 73, 68, 65, 84, 120, 218, 228, 148, 203, 79, 19, 97, 20, 197, 207, 204, 180, 196, 118, 58, 51, 116, 35, 76, 37, 33, 64, 89, 72, 210, 198, 134, 146, 144, 82, 22, 117, 129, 16, 68, 23, 60, 130, 91, 82, 170, 136, 11, 18, 12, 113, 217, 53, 143, 45, 255, 129, 134, 242, 90, 104, 13, 77, 109, 40, 144, 144, 52, 145, 202, 163, 6, 215, 26, 211, 84, 87, 246, 49, 44, 128, 249, 174, 11, 165, 97, 120, 24, 22, 221, 121, 183, 223, 239, 158, 197, 119, 206, 185, 28, 17, 161, 82, 195, 163, 130, 83, 81, 49, 78, 177, 59, 175, 123, 107, 3, 48, 12, 192, 3, 64, 2, 80, 4, 176, 7, 224, 53, 128, 244, 141, 196, 24, 35, 23, 17, 91, 240, 122, 239, 181, 12, 244, 247, 193, 237, 186, 11, 155, 36, 162, 84, 212, 144, 249, 252, 5, 145, 197, 183, 216, 217, 217, 59, 52, 153, 132, 97, 142, 227, 50, 134, 101, 197, 238, 132, 98, 119, 66, 174, 110, 178, 112, 66, 205, 98, 71, 231, 67, 138, 197, 214, 233, 228, 228, 148, 174, 26, 198, 24, 37, 18, 91, 228, 243, 247, 18, 39, 212, 44, 201, 213, 77, 150, 51, 13, 40, 118, 39, 108, 114, 131, 200, 155, 213, 200, 104, 104, 146, 24, 99, 229, 197, 100, 114, 155, 6, 135, 130, 164, 222, 113, 211, 224, 80, 144, 146, 201, 109, 131, 112, 232, 217, 75, 18, 204, 106, 196, 38, 55, 136, 138, 221, 9, 72, 74, 163, 96, 170, 114, 76, 143, 4, 39, 12, 96, 42, 149, 166, 177, 241, 41, 90, 89, 141, 146, 166, 105, 180, 178, 26, 165, 177, 241, 41, 74, 165, 210, 6, 110, 36, 56, 65, 166, 42, 199, 180, 164, 52, 10, 16, 204, 106, 115, 187, 175, 135, 242, 249, 130, 1, 10, 135, 103, 104, 118, 110, 158, 116, 93, 39, 34, 34, 93, 215, 105, 118, 110, 158, 194, 225, 25, 3, 151, 207, 23, 168, 221, 215, 67, 130, 89, 109, 230, 69, 209, 26, 120, 208, 21, 128, 44, 75, 134, 191, 76, 239, 30, 192, 225, 168, 5, 207, 255, 73, 15, 207, 243, 112, 56, 106, 145, 222, 61, 48, 112, 178, 44, 161, 187, 235, 62, 68, 209, 26, 224, 53, 237, 104, 35, 30, 223, 64, 161, 80, 52, 64, 173, 30, 55, 178, 217, 28, 24, 99, 127, 93, 102, 200, 102, 115, 104, 245, 184, 13, 92, 161, 80, 68, 44, 190, 14, 77, 59, 74, 10, 54, 233, 246, 175, 175, 223, 190, 91, 114, 63, 126, 118, 60, 126, 212, 93, 134, 44, 150, 91, 72, 110, 110, 227, 248, 248, 4, 245, 245, 117, 136, 190, 255, 128, 244, 167, 125, 12, 244, 247, 161, 174, 78, 45, 115, 207, 95, 188, 194, 218, 218, 250, 140, 213, 106, 89, 46, 187, 41, 152, 213, 72, 232, 233, 205, 221, 100, 140, 209, 104, 104, 146, 248, 243, 110, 94, 204, 153, 207, 223, 75, 137, 196, 150, 65, 244, 114, 206, 54, 201, 223, 217, 71, 188, 169, 118, 241, 124, 206, 46, 53, 128, 136, 92, 167, 167, 250, 66, 91, 155, 167, 101, 112, 224, 172, 1, 54, 148, 138, 37, 236, 103, 14, 177, 180, 252, 14, 59, 31, 175, 110, 192, 191, 186, 233, 5, 240, 228, 66, 55, 119, 1, 188, 185, 182, 155, 255, 199, 61, 251, 61, 0, 216, 84, 140, 133, 246, 8, 144, 48, 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 };
	Texture2D eyeTex;
		

	[MenuItem(MenuItemSTR)]
	static void Init () 
	{
		// Get existing open window or if none, make a new one:
		DAX_ParticleScaler window = (DAX_ParticleScaler)EditorWindow.GetWindow (typeof (DAX_ParticleScaler));
		window.titleContent.text = TabCaptionSTR;
		window.Show();
	}

    bool __ContainParentInArray( Transform parent )
    {
        if (parent == null) { return false; };
        if (findedObjects == null) { return false; };
        if (findedObjects.Length == 0) { return false; };

        for (int i = 0; i < findedObjects.Length; i++)
        {
            if ( (findedObjects[i] as GameObject).transform == parent)
            {
                return true;
            }
        }
        return false;
    }

    void _DAX_Filter_Childs_Of_Parents_Already_Selected()
    {
        ArrayList tfObj = new ArrayList();
        ArrayList expObj = new ArrayList();

        for (int i = 0; i < findedObjects.Length; i++)
        {
            if (!__ContainParentInArray( (findedObjects[i] as GameObject).transform.parent ))
            {
                tfObj.Add( findedObjects[i] );
                expObj.Add( exportedObjects[i] );
            }
        }

        findedObjects = new Object[ tfObj.Count ];
        exportedObjects = new bool[ expObj.Count ];

        for (int i = 0; i < tfObj.Count; i++)
        {
            findedObjects[i] = (Object)tfObj[i];
            exportedObjects[i] = (bool)expObj[i];
        }

    }

    void updateInfo(bool loadSelection, bool tOnlyROOT = true, bool EXMode = false) //  | FINDER PROGRAM | EX mode will filter all childs of selected parents =)
	{
		try{
			findedObjects = new Object[0];
			if (loadSelection) 
			{
				findedObjects = Selection.objects;
			}
			else
			{
				findedObjects = GameObject.FindObjectsOfType (typeof(GameObject));
			}
			
			if ( findedObjects != null )
			{
				Object[] tfObj;
				int maxSize = 0;
				for (int i = 0; i < findedObjects.Length; i++) 
				{
					//Debug.Log( findedObjects[i].GetType().ToString() );
					if (!AssetDatabase.Contains( findedObjects[i] ))
					{
						if (!tOnlyROOT)
                        {					
							maxSize++;
						}
						else if ((findedObjects[i] as GameObject).transform.parent == null)
						{
							maxSize++;
						}
					}
				}
				
				tfObj = new Object[maxSize];
				int curItem = 0;
				for (int i = 0; i < findedObjects.Length; i++) 
				{
					if (!AssetDatabase.Contains( findedObjects[i] ))
					{
						if (!tOnlyROOT | ((findedObjects[i] as GameObject).transform.parent == null) )
						{
							tfObj[curItem] = findedObjects[i];
							curItem++;
						}
					}
				}
				findedObjects = tfObj;	
				
				exportedObjects = new bool[findedObjects.Length];
				for(int i = 0; i < findedObjects.Length; i++) 
				{
					exportedObjects[i] = true;
				}
			}

            if (EXMode)
            {
                _DAX_Filter_Childs_Of_Parents_Already_Selected();
            }
		}catch{};
	}
	
	
	void OnGUI () 
	{
		try
		{
			GUILayout.Label ("Scale factor Settings", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical ();
			
			EditorGUILayout.BeginHorizontal ();
			Scale = EditorGUILayout.Slider( Scale, 0.1f, 10.0f );
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal ();
			
			EditorGUILayout.BeginVertical ();
				EditorGUILayout.BeginVertical(GUI.skin.box);

                ScaleGameObjects = EditorGUILayout.ToggleLeft( "Scale 'Game Objects' exclude PS", ScaleGameObjects, GUILayout.MaxWidth(210.0f) );
				ScaleShapeModule = EditorGUILayout.ToggleLeft( "Scale 'ShapeModule' for PS ", ScaleShapeModule, GUILayout.MaxWidth(210.0f) );

					EditorGUILayout.BeginHorizontal(GUI.skin.box);
					try{
						GUIStyle richTextStyle = new GUIStyle ();
						richTextStyle.richText = true;
						richTextStyle.wordWrap = true;
						if (ScaleGameObjects & ScaleShapeModule)
						{
							GUILayout.Label( "<b>Separated scale</b> GameObjects with ParticleSystem component(affect only shape module) and GameObjects without ParticleSystem component(affect Transform.Scale)", richTextStyle );
						}
						else if (!ScaleGameObjects & ScaleShapeModule)
						{
							GUILayout.Label( "Scale GameObjects with ParticleSystem component(<b>affect only shape module</b>) and NOT affect GameObjects without ParticleSystem component", richTextStyle );
						}
						else if (ScaleGameObjects & !ScaleShapeModule)
						{
							GUILayout.Label( "Scale all GameObjects(<b>affect Transform.Scale, NOT affect shape module</b>) ", richTextStyle );
						}
						else
						{
							GUILayout.Label( "<b>NOT affect Transform.Scale or shape module</b>", richTextStyle );
						}
					}catch{};
						
						
					EditorGUILayout.EndHorizontal();
					
				EditorGUILayout.EndVertical ();

                ScaleObjectHierarchy = EditorGUILayout.ToggleLeft( "Scale Objects hierarchy", ScaleObjectHierarchy, GUILayout.MaxWidth(210.0f) );
					MakeClones = EditorGUILayout.ToggleLeft( "Make renamed clones", MakeClones, GUILayout.MaxWidth(210.0f) );

			

			
				EditorGUILayout.EndVertical ();	
			EditorGUILayout.EndHorizontal();	
			EditorGUILayout.EndVertical ();
				
			EditorGUILayout.Separator();
				
			EditorGUILayout.BeginHorizontal ();
				
				if(GUILayout.RepeatButton("Load ALL", GUILayout.MaxWidth(70.0f)))
				{
					updateInfo( false );
				}
				if(GUILayout.RepeatButton("Load Selection(Root)", GUILayout.MaxWidth(135.0f)))
				{
					updateInfo( true );
				}
                if (GUILayout.RepeatButton("Load Selection(Child)", GUILayout.MaxWidth(135.0f)))
                {
                    updateInfo( true, false, true );
                }
				
			EditorGUILayout.EndHorizontal ();
				
			GUILayout.BeginHorizontal(GUI.skin.box/*, GUILayout.MaxHeight(400.0f)*/);
				
				if (eyeTex==null) //no eye lodaded
				{
					eyeTex = new Texture2D(2,2);
					eyeTex.LoadImage( pngBytesEYE ); //load eye
				}	
				
				scrollVector = GUILayout.BeginScrollView(scrollVector);
				if (findedObjects != null) 
				{
					for (int i = 0; i < findedObjects.Length; i++) 
					{
						if (findedObjects [i] != null)
						{		
							try
							{			
								EditorGUILayout.BeginHorizontal ();	
								
								if (GUILayout.Button( eyeTex, GUI.skin.label, GUILayout.MaxHeight(18.0f), GUILayout.MaxWidth( 20.0f )   )) //draw asterix
								{
									EditorGUIUtility.PingObject( findedObjects [i] );
								}
								
								try 
								{		
									exportedObjects [i] = EditorGUILayout.BeginToggleGroup (findedObjects [i].name, exportedObjects [i]); //checkboxes left
									EditorGUILayout.EndToggleGroup ();
									
								}catch{};
								EditorGUILayout.EndHorizontal ();
							}catch{};
						}		
					}
				}
				GUILayout.EndScrollView();
			GUILayout.EndHorizontal();
				
				
			EditorGUILayout.BeginHorizontal (GUI.skin.box);
				
				if (GUILayout.Button ("Rescale")) 
				{
					if (findedObjects != null) 
					{
						for (int i = 0; i < findedObjects.Length; i++) 
						{
							if (findedObjects [i] != null)
							{		
								try
								{	
									if ((exportedObjects [i]))
									{									
										StartScale( findedObjects [i] as GameObject );										
									}
								}	
								catch{};
							}
						}
					}
				}
				
				GUILayout.FlexibleSpace();
				
				if (GUILayout.Button ("Cancel")) 
				{
					DAX_ParticleScaler window = (DAX_ParticleScaler)EditorWindow.GetWindow (typeof (DAX_ParticleScaler));
					window.Close();
				}
				
			EditorGUILayout.EndHorizontal ();
		}catch{};
	}
	
	
	void StartScale( GameObject SelectedObject )  
	{
		//check if we need to update
		if ((Scale > 0) & (SelectedObject!=null))
		{
		
			if (this.MakeClones)
			{
				GameObject Clone;
				Clone = Instantiate(SelectedObject) as GameObject; 
				if (Clone==null){return;};
				Clone.name = SelectedObject.name + "_" + this.Scale.ToString( "00.00" );
				SelectedObject = Clone;
			}
			
			Transform baseTransform = SelectedObject.transform;
			
			Transform[] AllTransforms = SelectedObject.GetComponentsInChildren<Transform>( true );
			if (AllTransforms==null){return;};
			foreach (Transform Cur in AllTransforms )
			{
				if (Cur!=null)
				{
					ParticleSystem tPS = Cur.gameObject.GetComponent<ParticleSystem>();
					if ((ScaleGameObjects & !ScaleShapeModule ) | ( ScaleGameObjects & /*ScaleShapeModule &*/ tPS==null ))
					{
						Cur.localScale = new Vector3(Scale, Scale, Scale);
					}
					if (ScaleObjectHierarchy & Cur!=baseTransform ) //& detect root object
					{
						Cur.localPosition *= Scale;
					}
				}
			}

			//scale shuriken particle systems
			ScaleShurikenSystem( SelectedObject, Scale );

			//scale trail renders
			ScaleTrailRenderers( SelectedObject, Scale );
		}
	}
	
	void ScaleShurikenSystem( GameObject SelectedObject, float scaleFactor )
	{
		if (SelectedObject==null){return;};
		
		ParticleSystem[] AllSystems = SelectedObject.GetComponentsInChildren<ParticleSystem>( true );
		if (AllSystems==null){return;};
		
		foreach (ParticleSystem Cur in AllSystems)
		{
            var CurMainModule = Cur.main;
            CurMainModule.startSpeedMultiplier *= scaleFactor;
			//Cur.startSpeed *= scaleFactor;
            CurMainModule.startSizeMultiplier *= scaleFactor;
			//Cur.startSize *= scaleFactor;
            CurMainModule.gravityModifierMultiplier *= scaleFactor;
			//Cur.gravityModifier *= scaleFactor;

			SerializedObject CurSerializedOBJ = new SerializedObject(Cur);
			
			 
			CurSerializedOBJ.FindProperty("VelocityModule.x.scalar").floatValue *= scaleFactor;
			CurSerializedOBJ.FindProperty("VelocityModule.y.scalar").floatValue *= scaleFactor;
			CurSerializedOBJ.FindProperty("VelocityModule.z.scalar").floatValue *= scaleFactor;
	
			CurSerializedOBJ.FindProperty("ForceModule.x.scalar").floatValue *= scaleFactor;
			CurSerializedOBJ.FindProperty("ForceModule.y.scalar").floatValue *= scaleFactor;
			CurSerializedOBJ.FindProperty("ForceModule.z.scalar").floatValue *= scaleFactor;
					
			CurSerializedOBJ.FindProperty("ClampVelocityModule.magnitude.scalar").floatValue *= scaleFactor;
			CurSerializedOBJ.FindProperty("ClampVelocityModule.x.scalar").floatValue *= scaleFactor;
			CurSerializedOBJ.FindProperty("ClampVelocityModule.y.scalar").floatValue *= scaleFactor;
			CurSerializedOBJ.FindProperty("ClampVelocityModule.z.scalar").floatValue *= scaleFactor;
					
			CurSerializedOBJ.FindProperty("ColorBySpeedModule.range").vector2Value *= scaleFactor;
			
			CurSerializedOBJ.FindProperty("SizeBySpeedModule.range").vector2Value *= scaleFactor;
			
			if (this.ScaleShapeModule )
			{
				CurSerializedOBJ.FindProperty("ShapeModule.radius").floatValue *= scaleFactor;
				CurSerializedOBJ.FindProperty("ShapeModule.length").floatValue *= scaleFactor;
				CurSerializedOBJ.FindProperty("ShapeModule.boxX").floatValue *= scaleFactor;
				CurSerializedOBJ.FindProperty("ShapeModule.boxY").floatValue *= scaleFactor;
				CurSerializedOBJ.FindProperty("ShapeModule.boxZ").floatValue *= scaleFactor;
			}
					
			
			CurSerializedOBJ.FindProperty("RotationBySpeedModule.range").vector2Value *= scaleFactor;
			
			
			//ListAllPropertiesInSerializedObject(CurSerializedOBJ);
			CurSerializedOBJ.ApplyModifiedProperties();
			
			CurSerializedOBJ = null;
		}
	}
	
	
	void ScaleTrailRenderers( GameObject SelectedObject, float scaleFactor )
	{
		//get all animators we need to do scaling on
		TrailRenderer[] trails = SelectedObject.GetComponentsInChildren<TrailRenderer>(true);
		
		//apply scaling to animators
		foreach (TrailRenderer trail in trails)
		{
			trail.startWidth *= scaleFactor;
			trail.endWidth *= scaleFactor;
		}
	}



}

#endif
