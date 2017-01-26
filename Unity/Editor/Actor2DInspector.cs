using System;
using UnityEditor;
using UnityEngine;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(ActorComponent))]
	public class ActorComponentInspector : UnityEditor.Editor 
	{
		void OnEnable () 
		{
			// Setup the SerializedProperties.
			//testProp = serializedObject.FindProperty ("test");
			
		}

		int m_AnimationIndex;
		public override void OnInspectorGUI() 
		{
			if(GUILayout.Button("Reload"))
			{
				ActorComponent ActorComponent = serializedObject.targetObject as ActorComponent;
				ActorComponent.Reload();
				Debug.Log("REINIT");
			}
			string[] options = new string[] {"Cube", "Sphere", "Plane"};
			m_AnimationIndex = EditorGUILayout.Popup(m_AnimationIndex, options);
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