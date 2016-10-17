using System;
using System.IO;
using UnityEngine;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class ActorImage2D : ActorNode2D
	{
		public override void Initialize(Actor2D actor2D, Nima.ActorNode actorNode)
		{
			base.Initialize(actor2D, actorNode);

			ActorNode2D[] actorBones = actor2D.SkinnedBoneNodes;

			ActorImage imageNode = m_ActorNode as ActorImage;
			// Attach the skinned bone nodes to the bones of the skinned renderer.
			if(imageNode.IsSkinned)
			{
				SkinnedMeshRenderer skinnedRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

				Transform[] transforms = new Transform[imageNode.ConnectedBonesCount+1];
				transforms[0] = actor2D.DefaultBone.transform;
				int idx = 1;
				foreach(ActorImage.BoneConnection bc in imageNode.BoneConnections)
				{
					foreach(ActorNode2D ab in actorBones)
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

		public void Update()
		{
			// See if actor node has updated and update our transforms here.
			UpdateTransform();

#if UNITY_EDITOR
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
#endif
		}
	}
}