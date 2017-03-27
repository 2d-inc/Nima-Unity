using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(Play2DAnimation))]
	public class Play2DAnimationInspector : UnityEditor.Editor 
	{
		ActorComponent m_Actor;
		string[] m_AnimationNames;
		void OnEnable () 
		{
			
		}

		int m_AnimationIndex;
		public override void OnInspectorGUI() 
		{
			int animationIndex = 0;
			bool animationLoop = false;
			Play2DAnimation play2DAnimation = target as Play2DAnimation;
			if(play2DAnimation != null)
			{
				//Debug.Log("OBJ NOT NULL");
				m_Actor = play2DAnimation.GetComponent<ActorComponent>();
				List<string> names = new List<string>();
				if(m_Actor != null && m_Actor.ActorInstance != null)
				{
				//	Debug.Log("GOT ACTOR");
					if(m_Actor.ActorInstance == null)
					{
				//		Debug.Log("GOT ACTOR NULL");
					}
					foreach(Nima.Animation.ActorAnimation animation in m_Actor.ActorInstance.Animations)
					{
						names.Add(animation.Name);
					}
					m_AnimationNames = names.ToArray();
					animationIndex = names.IndexOf(play2DAnimation.AnimationName);
					animationLoop = play2DAnimation.Loop;
				}
				else
				{
					m_AnimationNames = null;
				}
			}
			else
			{
				//Debug.Log("TARGET NULL :(");
				m_AnimationNames = null;
			}
			if(m_AnimationNames != null)
			{
				int idx = EditorGUILayout.Popup(animationIndex, m_AnimationNames);
				if(idx != animationIndex)
				{
					play2DAnimation.AnimationName = m_AnimationNames[idx];
				}
				bool loop = EditorGUILayout.Toggle("Loop", animationLoop);
				if(loop != animationLoop)
				{
					play2DAnimation.Loop = loop;
				}
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