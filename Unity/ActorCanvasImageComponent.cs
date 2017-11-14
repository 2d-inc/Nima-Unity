using System;
using System.IO;
using UnityEngine;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class ActorCanvasImageComponent : MonoBehaviour
	{
		[NonSerialized]
		public Mesh m_Mesh;
		[NonSerialized]
		public Material m_Material;
		private Nima.ActorImage m_ActorNode;
		private float[] m_VertexPositionBuffer;
		private CanvasRenderer m_Renderer;

		public Nima.ActorImage Node
		{
			get
			{
				return m_ActorNode;
			}
			set
			{
				m_ActorNode = value;
			}
		}

		void Start()
		{
			if(m_ActorNode == null)
			{
				return;
			}
			m_Renderer = GetComponent<CanvasRenderer>();
			m_Renderer.Clear();
			m_Renderer.SetAlpha(m_ActorNode.RenderCollapsed ? 0.0f : 1.0f);
			m_Renderer.SetColor(Color.white);
			m_Renderer.SetMesh(m_Mesh);
			m_Renderer.materialCount = 1; 
			m_Renderer.SetMaterial(m_Material, 0);
			m_VertexPositionBuffer = m_ActorNode.MakeVertexPositionBuffer();
		}


		public void UpdateMesh()
		{
			if(m_ActorNode == null || m_Mesh == null || m_ActorNode.VertexCount == 0)
			{
				return;
			}

			if (m_ActorNode.RenderCollapsed) {
				m_Renderer.SetAlpha (0.0f);
			} else {
				m_Renderer.SetAlpha (1.0f);
			}

			m_ActorNode.UpdateVertexPositionBuffer(m_VertexPositionBuffer);

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

			byte alpha = (byte)(255.0f*m_ActorNode.RenderOpacity);
			if(m_Mesh.colors32[0].a != alpha)
			{
				Color32[] colors = new Color32[m_Mesh.colors32.Length];
				for(int i = 0; i < colors.Length; i++)
				{
					colors[i] = new Color32(255, 255, 255, alpha);
				}
				m_Mesh.colors32 = colors;
			}
			m_Renderer.SetMesh(m_Mesh);
		}
	}
}