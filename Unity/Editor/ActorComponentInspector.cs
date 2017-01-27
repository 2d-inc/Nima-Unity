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

		public override void OnInspectorGUI() 
		{
			ActorComponent actor = serializedObject.targetObject as ActorComponent;
			if(GUILayout.Button("Reload"))
			{
				actor.Reload();
				Debug.Log("REINIT");
			}
			int order = EditorGUILayout.IntField("Render Queue Offset:", actor.RenderQueueOffset);
			if(order != actor.RenderQueueOffset)
			{
				actor.RenderQueueOffset = order;
				actor.Reload();
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