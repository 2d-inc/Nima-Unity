using System;
using System.IO;
using UnityEngine;

namespace Nima.Unity
{
	public class ActorImageComponent : ActorNodeComponent
	{
		private int m_SortingOrderOffset = 0;
		private Mesh m_Mesh;

		public override void Initialize(ActorBaseComponent actorBaseComponent)
		{
			base.Initialize(actorBaseComponent);

			ActorComponent actorComponent = actorBaseComponent as ActorComponent;
			m_SortingOrderOffset = actorComponent.SortingOrder;

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

		void Start() 
		{
			ActorImage imageNode = m_ActorNode as ActorImage;
			if(imageNode == null)
			{
				return;
			}
			if(imageNode.IsSkinned)
			{
				m_Mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
			}
			else
			{
				m_Mesh = GetComponent<MeshFilter>().sharedMesh;
			}
		}

		public override void UpdateTransform()
		{
			if(m_ActorNode == null || m_Mesh == null)
			{
				return;
			}

			ActorImage imageNode = m_ActorNode as ActorImage;
			if(imageNode.IsVertexDeformDirty)
			{
				float[] v = imageNode.AnimationDeformedVertices;
				int l = imageNode.VertexCount;
				int vi = 0;
				Vector3[] verts = m_Mesh.vertices;
				for(int i = 0; i < l; i++)
				{
					float x = v[vi++];
					float y = v[vi++];
					verts[i] = new Vector3(x, y, 0.0f);
				}
				m_Mesh.vertices = verts;
				imageNode.IsVertexDeformDirty = false;
			}
			if(!imageNode.IsSkinned)
			{
				base.UpdateTransform();
			}

			Renderer renderer = gameObject.GetComponent<Renderer>();


			if(m_Mesh.colors[0].a != m_ActorNode.RenderOpacity)
			{
				Color32[] colors = new Color32[m_Mesh.colors32.Length];
				for(int i = 0; i < m_Mesh.colors32.Length; i++)
				{
					colors[i] = new Color32(255, 255, 255, (byte)(255.0f*m_ActorNode.RenderOpacity));
				}
				m_Mesh.colors32 = colors;
				//renderer.sharedMaterial.color = new Color(1.0f,1.0f,1.0f,m_ActorNode.RenderOpacity);
			}
			renderer.sortingOrder = m_SortingOrderOffset+imageNode.DrawOrder;
			//renderer.sharedMaterial.renderQueue = m_RenderQueueOffset+imageNode.DrawOrder;
		}
	}
}