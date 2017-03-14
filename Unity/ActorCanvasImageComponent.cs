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
			CanvasRenderer renderer = GetComponent<CanvasRenderer>();
			renderer.Clear();
			renderer.SetAlpha(1.0f);
			renderer.SetColor(Color.white);
			renderer.SetMesh(m_Mesh);
			renderer.materialCount = 1; 
			renderer.SetMaterial(m_Material, 0);
			m_VertexPositionBuffer = m_ActorNode.MakeVertexPositionBuffer();
		}


		public void UpdateMesh()
		{
			if(m_ActorNode == null || m_Mesh == null)
			{
				return;
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
			//m_Material.renderQueue = m_ActorNode.DrawOrder;

			CanvasRenderer renderer = GetComponent<CanvasRenderer>();
			renderer.Clear();
			renderer.SetAlpha(1.0f);
			renderer.SetColor(Color.white);
			renderer.SetMesh(m_Mesh);
			renderer.materialCount = 1; 
			renderer.SetMaterial(m_Material, 0);
		}
	}
}