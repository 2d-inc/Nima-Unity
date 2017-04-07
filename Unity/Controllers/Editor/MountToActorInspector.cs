using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Nima.Unity.Editor 
{
	[CustomEditor(typeof(MountToActor))]
	public class MountToActorInspector : UnityEditor.Editor 
	{
		public override void OnInspectorGUI() 
		{
			MountToActor mountToActor = target as MountToActor;
			UnityEngine.Object obj = EditorGUILayout.ObjectField("Mount Actor", mountToActor.ActorGameObject, typeof(GameObject), true);
			if(obj is GameObject && (obj as GameObject) != mountToActor.ActorGameObject)
			{
				mountToActor.ActorGameObject = obj as GameObject;
			}

			string[] nodeNames;
			if(mountToActor.ActorGameObject != null)
			{
				ActorBaseComponent actorBase = mountToActor.ActorGameObject.GetComponent<ActorBaseComponent>();
				List<string> names = new List<string>();
				if(actorBase != null && actorBase.Asset != null && actorBase.Asset.Actor == null)
				{
					actorBase.Asset.Load();
				}


				if(actorBase != null && actorBase.Asset != null && actorBase.Asset.Actor != null)
				{
					foreach(Nima.ActorNode node in actorBase.Asset.Actor.Nodes)
					{
						if(node == null)
						{
							continue;
						}
						if(names.Contains(node.Name))
						{
							continue;
						}
						names.Add(node.Name);
					}
					nodeNames = names.ToArray();
				}
				else
				{
					nodeNames = null;
				}
			}
			else
			{
				nodeNames = null;
			}

			if(nodeNames != null)
			{
				int nodeIndex = Array.IndexOf<string>(nodeNames, mountToActor.NodeName);

				int idx = EditorGUILayout.Popup("Node", nodeIndex, nodeNames);
				if(idx != nodeIndex)
				{
					mountToActor.NodeName = nodeNames[idx];
				}
				mountToActor.InheritScale = EditorGUILayout.Toggle("Inherit Scale", mountToActor.InheritScale);
				mountToActor.InheritRotation = EditorGUILayout.Toggle("Inherit Rotation", mountToActor.InheritRotation);
				mountToActor.ScaleModifier = EditorGUILayout.FloatField("Scale Modifier", mountToActor.ScaleModifier);
				mountToActor.UpdateMount();
			}
		}
	}
}