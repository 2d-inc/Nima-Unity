using System;
using System.IO;
using UnityEngine;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class ActorImageComponent : ActorNodeComponent
	{
		public override void Initialize(ActorComponent actorComponent, Nima.ActorNode actorNode)
		{
			base.Initialize(actorComponent, actorNode);

			ActorNodeComponent[] actorBones = actorComponent.SkinnedBoneNodes;
			ActorImage imageNode = m_ActorNode as ActorImage;
			// Attach the skinned bone nodes to the bones of the skinned renderer.
			if(imageNode.IsSkinned)
			{
				SkinnedMeshRenderer skinnedRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

				Transform[] transforms = new Transform[imageNode.ConnectedBoneCount+1];
				transforms[0] = actorComponent.DefaultBone.transform;
				int idx = 1;
				foreach(ActorImage.BoneConnection bc in imageNode.BoneConnections)
				{
					foreach(ActorNodeComponent ab in actorBones)
					{
						if(ab.Node == bc.Node)
						{
							transforms[idx] = ab.transform;
							break;
						}
					}
					idx++;
				}
				skinnedRenderer.bones = transforms;
			}
		}

		public override void UpdateTransform()
		{
			if(m_ActorNode == null)
			{
				return;
			}

			ActorImage imageNode = m_ActorNode as ActorImage;

			if(!imageNode.IsSkinned)
			{
				base.UpdateTransform();
			}

			Renderer renderer = gameObject.GetComponent<Renderer>();
			renderer.sharedMaterial.renderQueue = imageNode.DrawOrder;
		}

#if UNITY_EDITOR
		public void Update()
		{
			// See if actor node has updated and update our transforms here.
			UpdateTransform();
			if(!Application.isPlaying)
			{
				// We are in the editor and not playing, so let's make sure things stay synced.
				MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
				ActorImage imageNode = m_ActorNode as ActorImage;
				if(imageNode != null && meshRenderer != null && meshRenderer.sharedMaterial != null)
				{
					meshRenderer.sharedMaterial.renderQueue = imageNode.DrawOrder;
				}
			}
		}
#endif
	}
}