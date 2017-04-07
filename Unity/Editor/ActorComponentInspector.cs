using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(ActorBaseComponent), true)]
	public class ActorComponentInspector : UnityEditor.Editor 
	{
		void OnEnable () 
		{
			
		}


		public override void OnInspectorGUI() 
		{
			ActorBaseComponent actor = serializedObject.targetObject as ActorBaseComponent;

			ActorAsset asset = EditorGUILayout.ObjectField(actor.Asset, typeof(ActorAsset), false) as ActorAsset;
			if(asset != actor.Asset)
			{
				actor.Asset = asset;
			}
			/*if(actor.Asset != null)
			{
				for(int i = 0; i < actor.Asset.m_TextureMaterials.Length; i++)
				{
					Material mat = null;
					Material m = EditorGUILayout.ObjectField(mat, typeof(Material), false) as Material;
				}
			}*/
			if(GUILayout.Button("Reload"))
			{
				actor.Reload();
				Animator animator = actor.gameObject.GetComponent<Animator>();
				if(animator != null && animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController != null)
				{
					Importer.ReloadMecanimController(actor, animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController);
				}
			}
			
			if(actor is IRenderSortableComponent)
			{
				IRenderSortableComponent renderSortable = actor as IRenderSortableComponent;
				PropertyInfo sortingLayersProperty = typeof(InternalEditorUtility).GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
	        	string[] sortLayerNames = (string[])sortingLayersProperty.GetValue(null, new object[0]);

	        	PropertyInfo sortingLayerUniqueIDsProperty = typeof(InternalEditorUtility).GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
	        	int[] sortLayerIds = (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);


				int currentlySelectedIndex = -1;
				for(int i = 0 ; i < sortLayerIds.Length ; i++)
				{
					if(renderSortable.SortingLayerID == sortLayerIds[i])
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
	        		renderSortable.SortingLayerID = sortLayerIds[selectedIndex];
					reload = true;
	        	}

				int order = EditorGUILayout.IntField("Order in Layer", renderSortable.SortingOrder);
				if(order != renderSortable.SortingOrder)
				{
					renderSortable.SortingOrder = order;
					reload = true;
				}

				if(reload)
				{
					actor.Reload();
				}
			}
			ActorMecanimComponent actorMecanim = actor.gameObject.GetComponent<ActorMecanimComponent>();
			if(actorMecanim == null)
			{
				if(GUILayout.Button("Add Mecanim Components"))
				{
					//Animator animatorComponent = actor.gameObject.AddComponent( typeof(Animator) ) as Animator;
					/*ActorMecanimComponent mecanimComponent = */actor.gameObject.AddComponent( typeof(ActorMecanimComponent) );// as ActorMecanimComponent;
				}
			}
		}
	}
}