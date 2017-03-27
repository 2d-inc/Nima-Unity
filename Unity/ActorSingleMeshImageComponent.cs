using System;
using System.IO;
using UnityEngine;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class ActorSingleMeshImageComponent : ActorNodeComponent
	{
		[NonSerialized]
		public Material m_Material;
		private float[] m_VertexPositionBuffer;
		public int[] m_Triangles;
		private ActorImage m_ActorImage;

		void Start()
		{
			if(m_ActorNode == null)
			{
				return;
			}
			m_ActorImage = m_ActorNode as ActorImage;
			m_VertexPositionBuffer = m_ActorImage.MakeVertexPositionBuffer();
		}


		public void UpdateMesh(Vector3[] verts, Color32[] colors, int offset)
		{
			if(m_ActorImage == null)
			{
				return;
			}
			m_ActorImage.UpdateVertexPositionBuffer(m_VertexPositionBuffer);

			int vertexCount = m_ActorImage.VertexCount;
			int readIdx = 0;
			int writeIdx = offset;
			for(int i = 0; i < vertexCount; i++)
			{
				float x = m_VertexPositionBuffer[readIdx++];
				float y = m_VertexPositionBuffer[readIdx++];
				verts[writeIdx] = new Vector3(x, y, 0.0f);
				writeIdx++;
			}

			byte alpha = (byte)(255.0f*m_ActorImage.RenderOpacity);
			if(colors[offset].a != alpha)
			{
				int colorIdx = offset;
				for(int i = 0; i < vertexCount; i++)
				{
					colors[colorIdx] = new Color32(255, 255, 255, alpha);
					colorIdx++;
				}
			}
/*
			int numVerts = m_VertexPositionBuffer.Length/2;
			int readIdx = 0;
			Vector3[] verts = m_Mesh.vertices;
			for(int i = 0; i < numVerts; i++)
			{
				float x = m_VertexPositionBuffer[readIdx++];
				float y = m_VertexPositionBuffer[readIdx++];
				verts[i] = new Vector3(x, y, 0.0f);
			}
			m_Mesh.vertices = verts;

			if(m_Mesh.colors[0].a != m_ActorNode.RenderOpacity)
			{
				Color32[] colors = new Color32[m_Mesh.colors32.Length];
				for(int i = 0; i < m_Mesh.colors32.Length; i++)
				{
					colors[i] = new Color32(255, 255, 255, (byte)(255.0f*m_ActorNode.RenderOpacity));
				}
				m_Mesh.colors32 = colors;
			}
			m_Renderer.SetMesh(m_Mesh);*/
		}
	}
}