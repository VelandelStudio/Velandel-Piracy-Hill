//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Network.NST
{
	/// <summary>
	/// Non-generic base class for SettingsScriptableObject to make initialize a common base call.
	/// </summary>
	public abstract class SettingsScriptableObjectBase : ScriptableObject
	{
		public virtual void Initialize() { }
	}

	/// <summary>
	/// Base class for all of the settings scriptable objects.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SettingsScriptableObject<T> : SettingsScriptableObjectBase where T : ScriptableObject
	{
		[HideInInspector]
		public abstract string SettingsName { get; }


		public static T single;
		/// <summary>
		/// Using Single rather than single fires off an check to make sure the singleton SO has been found and is mapped it 'single'.
		/// It also fires off an Initialize() to ensure everything is in order. Do not use Single in a hot path for this reason, but rather
		/// use the backing single field instead.
		/// </summary>
		public static T Single
		{
			get
			{
				if (!single)
				{
					string classname = typeof(T).Name;
					single = Resources.Load<T>(classname) as T;
					if (single)
						(single as SettingsScriptableObject<T>).Initialize();
				}
				return single;
			}
		}

		protected virtual void Awake()
		{
			Initialize();
		}


#if UNITY_EDITOR

		[HideInInspector]
		public abstract string HelpURL { get; }

		public static Dictionary<object, bool> expandeds = new Dictionary<object, bool>();
			
		/// <summary>
		/// EditorGUILayout Serialize all visible properties in this Scriptable Object. Returns true if expanded;
		/// </summary>
		public virtual bool DrawGui(object target, bool asFoldout, bool includeScriptField)
		{
			if (!expandeds.ContainsKey(target))
				expandeds.Add(target, true);

			Rect rt = EditorGUILayout.GetControlRect();
			EditorGUI.LabelField(rt, SettingsName, (GUIStyle)"BoldLabel");

			// Width a one pixel wider than the header, fixing this.
			rt.width -= 1;

			//Adjust the find button to left align correctly
			rt.xMin += (asFoldout) ? 2 : -7;

			if (GUI.Button(rt, SettingsName, (GUIStyle)"PreButton"))
			{
				EditorGUIUtility.PingObject(Single);
				Application.OpenURL(HelpURL);
			}

			//if (GUI.Button(rt, EditorGUIUtility.IconContent("_Help")))
			//	Application.OpenURL(helpURL);

			//Adjust the xmin back
			rt.xMin += (asFoldout) ? -2 : 7;

			if (asFoldout)
				expandeds[target] = EditorGUI.Foldout(rt, expandeds[target], "");

			// Adjust the find button to left align correctly
			rt.xMin += (asFoldout) ? 2 : -7;

			if (GUI.Button(rt, SettingsName, (GUIStyle)"PreButton"))
			{
				EditorGUIUtility.PingObject(Single);
				Application.OpenURL(HelpURL);
			}


			//rt.xMin += (rt.width - 16);
			//if (GUI.Button(rt, EditorGUIUtility.IconContent("_Help")))
			//	Application.OpenURL(helpURL);

			if (!asFoldout || expandeds[target])
			{
				EditorGUILayout.Space();

				SerializedObject so = new SerializedObject(Single);
				SerializedProperty sp = so.GetIterator();
				sp.Next(true);

				// Skip drawing the script reference?
				if (!includeScriptField)
					sp.NextVisible(false);

				EditorGUI.BeginChangeCheck();
				while (sp.NextVisible(false))
				{
					EditorGUILayout.PropertyField(sp);
				}

				so.ApplyModifiedProperties();

				EditorGUILayout.Space();

				if (EditorGUI.EndChangeCheck())
				{
					Initialize();
					AssetDatabase.SaveAssets();
				}
			}
			return expandeds[target];
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(ScriptableObject))]
	public class SettingsSOBaseEditor<T> : NSTHeaderEditorBase where T : SettingsScriptableObject<T>
	{
		public override void OnEnable()
		{
			headerName = HeaderSettingsName;
			headerColor = HeaderSettingsColor;
			base.OnEnable();
			if (!SettingsScriptableObject<T>.expandeds.ContainsKey(target))
				SettingsScriptableObject<T>.expandeds.Add(target, true);
		}

		public override void OnInspectorGUI()
		{
			OverlayHeader();
		}
	}
#endif
}
