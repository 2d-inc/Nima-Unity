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
				m_PreviewRenderUtility.camera.orthographic = true;//transform.position = new Vector3(0, 0, -6);
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

					
					m_PreviewRenderUtility.camera.orthographicSize = 12.0f;//scale * 2f;
					m_PreviewRenderUtility.camera.nearClipPlane = 0f;
					m_PreviewRenderUtility.camera.farClipPlane = 25f;

					m_PreviewRenderUtility.BeginPreview(r, background);
					ActorAsset asset = target as ActorAsset;
					if(asset != null && asset.Actor == null)
					{
						asset.Load();
					}
					m_PreviewRenderUtility.camera.Render();

					Texture resultRender = m_PreviewRenderUtility.EndPreview();
					GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
				}
			}
		}

		public void OnDisable()
		{
			m_PreviewRenderUtility.Cleanup();
		}
	}
}