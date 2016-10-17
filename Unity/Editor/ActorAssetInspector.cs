using System;
using UnityEditor;
using UnityEngine;

namespace Nima.Unity.Editor 
{
//	[CustomEditor(typeof(ActorAsset))]
	public class ActorAssetInspector : UnityEditor.Editor 
	{
		SerializedProperty testProp;
		void OnEnable () 
		{
			// Setup the SerializedProperties.
			//testProp = serializedObject.FindProperty ("test");
			Debug.Log("TRIED TO GET IT");
		}
	
		public override void OnInspectorGUI() 
		{
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