using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(ActorAsset))]
	public class ActorAssetInspector : UnityEditor.Editor 
	{
		private PreviewRenderUtility m_PreviewRenderUtility;
		public override bool HasPreviewGUI()
		{
			if(m_PreviewRenderUtility == null)
			{
				m_PreviewRenderUtility = new PreviewRenderUtility();
				m_PreviewRenderUtility.m_Camera.orthographic = true;//transform.position = new Vector3(0, 0, -6);
				//m_PreviewRenderUtility.m_Camera.transform.rotation = Quaternion.identity;
			}
			return true;
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			//_drag = Drag2D(_drag, r);

			if (Event.current.type == EventType.Repaint)
			{
				/*if(_targetMeshRenderer == null)
				{
					EditorGUI.DropShadowLabel(r, "Mesh Renderer Required");
				}
				else*/
				{

					
					m_PreviewRenderUtility.m_Camera.orthographicSize = 12.0f;//scale * 2f;
					m_PreviewRenderUtility.m_Camera.nearClipPlane = 0f;
					m_PreviewRenderUtility.m_Camera.farClipPlane = 25f;

					m_PreviewRenderUtility.BeginPreview(r, background);
					ActorAsset asset = target as ActorAsset;
					if(asset != null && asset.Actor == null)
					{
						asset.Load();
					}
					/*foreach(ActorNodeComponent component in actorComponent.Nodes)
					{
						if(component is ActorImageComponent)// && component.Node != null)
						{
							//Debug.Log("HERE?1");
							ActorImageComponent imageComponent = component as ActorImageComponent;
							ActorImage imageNode = imageComponent.Node as ActorImage;
							if(imageNode == null)
							{
								Debug.Log("It's an image but the node is nul");
								continue;
							}
							if(!imageNode.IsSkinned)
							{
								Nima.Math2D.Mat2D worldTransform = imageNode.WorldTransform;
								Matrix4x4 mat = Matrix4x4.identity;
								mat[0,0] = worldTransform[0];
								mat[1,0] = worldTransform[1];
								mat[0,1] = worldTransform[2];
								mat[1,1] = worldTransform[3];
								mat[0,3] = worldTransform[4];
								mat[1,3] = worldTransform[5];
								MeshFilter filter = imageComponent.GetComponent<MeshFilter>();
        						MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
								m_PreviewRenderUtility.DrawMesh(filter.sharedMesh, mat, renderer.sharedMaterial, 0);
							}
							
						}
					}*/

					/*m_PreviewRenderUtility.m_Camera.transform.position = Vector2.zero;
					m_PreviewRenderUtility.m_Camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
					m_PreviewRenderUtility.m_Camera.transform.position = m_PreviewRenderUtility.m_Camera.transform.forward * -6f;*/
					m_PreviewRenderUtility.m_Camera.Render();

					Texture resultRender = m_PreviewRenderUtility.EndPreview();
					GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
				}
			}
		}
	}

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