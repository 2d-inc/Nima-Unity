using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;

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
			
			PropertyInfo sortingLayersProperty = typeof(InternalEditorUtility).GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        	string[] sortLayerNames = (string[])sortingLayersProperty.GetValue(null, new object[0]);

        	PropertyInfo sortingLayerUniqueIDsProperty = typeof(InternalEditorUtility).GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
        	int[] sortLayerIds = (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);


			int currentlySelectedIndex = -1;
			for(int i = 0 ; i < sortLayerIds.Length ; i++)
			{
				if(actor.SortingLayerID == sortLayerIds[i])
				{
				    currentlySelectedIndex = i;
				}
			}

			int displaySelectedIndex = currentlySelectedIndex;
			if(displaySelectedIndex == -1)
			{
				// Find default layer.
				for(int i = 0 ; i < sortLayerIds.Length ; i++)
				{
					if(sortLayerIds[i] == 0)
					{
						displaySelectedIndex = i;
					}
				}
			}

			bool reload = false;
        	int selectedIndex = EditorGUILayout.Popup("Sorting Layer", displaySelectedIndex, sortLayerNames);
        	if(selectedIndex != currentlySelectedIndex)
        	{
        		actor.SortingLayerID = sortLayerIds[selectedIndex];
				reload = true;
        	}

			int order = EditorGUILayout.IntField("Order in Layer", actor.SortingOrder);
			if(order != actor.SortingOrder)
			{
				actor.SortingOrder = order;
				reload = true;
			}

			if(reload)
			{
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