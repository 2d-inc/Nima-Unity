using System;
using UnityEditor;
using UnityEngine;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(Actor2D))]
	public class Actor2DInspector : UnityEditor.Editor 
	{
		void OnEnable () 
		{
			// Setup the SerializedProperties.
			//testProp = serializedObject.FindProperty ("test");
			
		}

		public override void OnInspectorGUI() 
		{
			if(GUILayout.Button("Reload"))
			{
				Actor2D actor2D = serializedObject.targetObject as Actor2D;
				actor2D.Reload();
				Debug.Log("REINIT");
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
	}
}