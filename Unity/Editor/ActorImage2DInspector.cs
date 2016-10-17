using System;
using UnityEditor;
using UnityEngine;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(ActorImage2D))]
	public class ActorImage2DInspector : UnityEditor.Editor 
	{
		 Tool LastTool = Tool.None;

		void OnEnable()
		{
			LastTool = Tools.current;
			Tools.current = Tool.None;
		}

		void OnDisable()
		{
			Tools.current = LastTool;
		}

		public override void OnInspectorGUI() 
		{
        	if (GUI.changed)
        	{
        		Debug.Log("INSPECTORGUI CHANGED");
        	}
			/*if(testProp == null)
			{
				Debug.Log("NULL!?");
			}
			serializedObject.Update ();
		
			// Show the custom GUI controls.
			EditorGUILayout.IntSlider (testProp, 0, 100, new GUIContent ("Damage"));

			// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
			serializedObject.ApplyModifiedProperties ();*/
		}

		public void OnValidate()
		{
			Debug.Log("VALIDATE ACTORIMAGE 2D Inspector");
		}
	}
}