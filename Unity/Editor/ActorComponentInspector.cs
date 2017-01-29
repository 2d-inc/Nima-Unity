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
			}
			int order = EditorGUILayout.IntField("Render Queue Offset:", actor.RenderQueueOffset);
			if(order != actor.RenderQueueOffset)
			{
				actor.RenderQueueOffset = order;
				actor.Reload();
			}
		}
	}
}