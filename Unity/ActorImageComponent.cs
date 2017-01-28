using System;
using System.IO;
using UnityEngine;

namespace Nima.Unity
{
	public class ActorImageComponent : ActorNodeComponent
	{
		private int m_RenderQueueOffset = 0;
		public override void Initialize(ActorComponent actorComponent, Nima.ActorNode actorNode)
		{
			base.Initialize(actorComponent, actorNode);

			m_RenderQueueOffset = actorComponent.RenderQueueOffset;

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
					ActorNodeComponent boneComponent = actorComponent.Nodes[bc.m_BoneIdx];
					transforms[idx] = boneComponent.gameObject.transform;
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
			if(imageNode.IsVertexDeformDirty)
			{
				Mesh mesh;
				if(imageNode.IsSkinned)
				{
					mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
				}
				else
				{
					MeshFilter meshFilter = GetComponent<MeshFilter>();
					mesh = meshFilter.sharedMesh;
				}

				float[] v = imageNode.AnimationDeformedVertices;
				int l = imageNode.VertexCount;
				int vi = 0;
				Vector3[] verts = mesh.vertices;
				for(int i = 0; i < l; i++)
				{
					float x = v[vi++];
					float y = v[vi++];
					verts[i] = new Vector3(x, y, 0.0f);
				}
				mesh.vertices = verts;
				imageNode.IsVertexDeformDirty = false;
			}
			if(!imageNode.IsSkinned)
			{
				base.UpdateTransform();
			}

			Renderer renderer = gameObject.GetComponent<Renderer>();

			if(renderer.sharedMaterial.color.a != m_ActorNode.RenderOpacity)
			{
				renderer.sharedMaterial.color = new Color(1.0f,1.0f,1.0f,m_ActorNode.RenderOpacity);
			}
			renderer.sharedMaterial.renderQueue = m_RenderQueueOffset+imageNode.DrawOrder;
		}
	}
}