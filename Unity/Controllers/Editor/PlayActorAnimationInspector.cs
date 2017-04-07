using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(PlayActorAnimation))]
	public class PlayActorAnimationInspector : UnityEditor.Editor 
	{
		ActorBaseComponent m_Actor;
		string[] m_AnimationNames;
		
		int m_AnimationIndex;
		public override void OnInspectorGUI() 
		{
			PlayActorAnimation playActorAnimation = target as PlayActorAnimation;
			if(playActorAnimation != null)
			{
				m_Actor = playActorAnimation.GetComponent<ActorBaseComponent>();
				List<string> names = new List<string>();
				if(m_Actor != null && m_Actor.Asset != null && m_Actor.Asset.Actor == null)
				{
					m_Actor.Asset.Load();
				}


				if(m_Actor != null && m_Actor.Asset != null && m_Actor.Asset.Actor != null)
				{
					foreach(Nima.Animation.ActorAnimation animation in m_Actor.Asset.Actor.Animations)
					{
						names.Add(animation.Name);
					}
					m_AnimationNames = names.ToArray();
				}
				else
				{
					m_AnimationNames = null;
				}
			}
			else
			{
				m_AnimationNames = null;
			}

			if(m_AnimationNames != null)
			{
				bool animationLoop  = playActorAnimation.Loop;
				int animationIndex = Array.IndexOf<string>(m_AnimationNames, playActorAnimation.AnimationName);
				playActorAnimation.Offset = EditorGUILayout.Slider("Offset", playActorAnimation.Offset, 0.0F, 1.0F);
				playActorAnimation.Speed = EditorGUILayout.Slider("Speed", playActorAnimation.Speed, 0.01F, 3.0F);

				int idx = EditorGUILayout.Popup(animationIndex, m_AnimationNames);
				if(idx != animationIndex)
				{
					playActorAnimation.AnimationName = m_AnimationNames[idx];
				}
				bool loop = EditorGUILayout.Toggle("Loop", animationLoop);
				if(loop != animationLoop)
				{
					playActorAnimation.Loop = loop;
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