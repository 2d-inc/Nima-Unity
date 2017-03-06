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
			
		}

		public override void OnInspectorGUI() 
		{
			ActorComponent actor = serializedObject.targetObject as ActorComponent;

			ActorAsset asset = EditorGUILayout.ObjectField(actor.Asset, typeof(ActorAsset), false) as ActorAsset;
			if(asset != actor.Asset)
			{
				actor.Asset = asset;
			}
			if(GUILayout.Button("Reload"))
			{
				actor.Reload();
				Animator animator = actor.gameObject.GetComponent<Animator>();
				if(animator != null && animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController != null)
				{
					Importer.ReloadMecanimController(actor, animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController);
				}
			}
			int order = EditorGUILayout.IntField("Render Queue Offset:", actor.RenderQueueOffset);
			if(order != actor.RenderQueueOffset)
			{
				actor.RenderQueueOffset = order;
				actor.Reload();
			}
			ActorMecanimComponent actorMecanim = actor.gameObject.GetComponent<ActorMecanimComponent>();
			if(actorMecanim == null)
			{
				if(GUILayout.Button("Add Mecanim Components"))
				{
					Animator animatorComponent = actor.gameObject.AddComponent( typeof(Animator) ) as Animator;
					ActorMecanimComponent mecanimComponent = actor.gameObject.AddComponent( typeof(ActorMecanimComponent) ) as ActorMecanimComponent;
				}
			}
		}
	}
}