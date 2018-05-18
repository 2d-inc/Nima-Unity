using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nima.Unity
{
	[ExecuteInEditMode]
	[SelectionBase]
	public class ActorSingleMeshComponent : ActorBaseComponent, IRenderSortableComponent
	{
		private Mesh m_Mesh;
		private MeshRenderer m_MeshRenderer;

		private List<ActorSingleMeshImageComponent> m_ImageComponents;

		[SerializeField]
		private int m_SortingLayerID;
		[SerializeField]
		private int m_SortingOrder;

		public int SortingOrder
		{
			get
			{
				return m_SortingOrder;
			}
			set
			{
				m_SortingOrder = value;
				if(m_MeshRenderer != null)
				{
					m_MeshRenderer.sortingOrder = m_SortingOrder;
				}
			}
		}

		public int SortingLayerID
		{
			get
			{
				return m_SortingLayerID;
			}
			set
			{
				m_SortingLayerID = value;
				if(m_MeshRenderer != null)
				{
					m_MeshRenderer.sortingLayerID = m_SortingLayerID;
				}
			}
		}

		protected override bool InstanceColliders
		{
			get
			{
				return true;
			}
		}

		protected override void RemoveNodes()
		{
			if(m_ImageComponents != null)
			{
				foreach(ActorSingleMeshImageComponent c in m_ImageComponents)
				{
					if(c != null)
					{
						if(c.gameObject != null)
						{
							DestroyImmediate(c.gameObject);	
						}
						else
						{
							DestroyImmediate(c);	
						}
					}
				}
				m_ImageComponents = null;
			}
			base.RemoveNodes();
		}

		protected override void OnActorInstanced()
		{
			m_ImageComponents = new List<ActorSingleMeshImageComponent>();
		}
		
		/// <summary>
		/// Force set the material for a specific blend mode. Note that this leaves texture atlas and shader selection up to you!
		/// </summary>
		public void SetMaterial(Material material, BlendModes blendMode)
		{
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null || (component.Node as ActorImage).BlendMode != blendMode)
				{
					continue;
				}
				component.m_Material = material;
			}
		}

		/// <summary>
		/// Force set the material for the entire set of components. Note that this leaves texture atlas and shader selection up to you!
		/// </summary>
		public void SetMaterial(Material material)
		{
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null)
				{
					continue;
				}
				component.m_Material = material;
			}
		}

		/// <summary>
		/// Set the material for the a component with name nodeName. Note that this leaves texture atlas and shader selection up to you!
		/// </summary>
		public void SetMaterial(Material material, String nodeName)
		{
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null || component.Node.Name != nodeName)
				{
					continue;
				}
				component.m_Material = material;
			}
		}
		
		protected override ActorNodeComponent MakeImageNodeComponent(int imageNodeIndex, ActorImage actorImage)
		{
			bool hasBones = actorImage.ConnectedBoneCount > 0;

			GameObject go = new GameObject(actorImage.Name, typeof(ActorSingleMeshImageComponent));
			
			ActorSingleMeshImageComponent actorImageComponent = go.GetComponent<ActorSingleMeshImageComponent>();
			actorImageComponent.Node = actorImage;
			m_ImageComponents.Add(actorImageComponent);

			Material material = m_ActorAsset.GetMaterial(actorImage.TextureIndex);
			if(material != null)
			{
				switch(actorImage.BlendMode)
				{
					case BlendModes.Screen:
					{
						Material overrideMaterial = new Material(Shader.Find("Nima/Screen"));
						overrideMaterial.mainTexture = material.mainTexture;
						material = overrideMaterial;
						break;
					}
					case BlendModes.Additive:
					{
						Material overrideMaterial = new Material(Shader.Find("Nima/Additive"));
						overrideMaterial.mainTexture = material.mainTexture;
						material = overrideMaterial;
						break;
					}
					case BlendModes.Multiply:
					{
						Material overrideMaterial = new Material(Shader.Find("Nima/Multiply"));
						overrideMaterial.mainTexture = material.mainTexture;
						material = overrideMaterial;
						break;
					}
					default:
					{
					/*	Material overrideMaterial = new Material(Shader.Find("Nima/Normal"));
						//Material overrideMaterial = new Material(Shader.Find("Transparent/Diffuse"));
						overrideMaterial.color = Color.white;
						overrideMaterial.mainTexture = material.mainTexture;
						material = overrideMaterial;*/
						break;
					}
				}
			}
		//	actorImageComponent.m_Mesh = mesh;
			actorImageComponent.m_Material = material;
			if(material != null)
			{
				material.hideFlags = HideFlags.HideInInspector;
			}

			return actorImageComponent;
		}

		bool m_UsingA;
		private Vector3[] m_VerticesA;
		private Vector3[] m_VerticesB;
		private Color32[] m_ColorsA;
		private Color32[] m_ColorsB;
		private Material[] m_MaterialsA;
		private Material[] m_MaterialsB;
		private Vector2[] m_UVs;

		protected override void OnActorInitialized()
		{
			m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
			m_MeshRenderer.sortingOrder = m_SortingOrder;
			m_MeshRenderer.sortingLayerID = m_SortingLayerID;

			m_Mesh = new Mesh();
			m_Mesh.MarkDynamic();
			m_Mesh.subMeshCount = m_ImageComponents.Count;

			// Count vertex size.
			int totalVertexCount = 0;
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null)
				{
					continue;
				}
				ActorImage imageNode = component.Node as ActorImage;

				totalVertexCount += imageNode.VertexCount;
			}


			int vertexOffset = 0;

			m_VerticesA = new Vector3[totalVertexCount];
			m_VerticesB = new Vector3[totalVertexCount];
			m_ColorsA = new Color32[totalVertexCount];
			m_ColorsB = new Color32[totalVertexCount];
			m_MaterialsA = new Material[m_Mesh.subMeshCount];
			m_MaterialsB = new Material[m_Mesh.subMeshCount];

			m_UsingA = true;
			Vector3[] vertices = m_VerticesA;
			Color32[] colors = m_ColorsA;
			Material[] materials = m_MaterialsA;

			m_UVs = new Vector2[totalVertexCount];
			m_MeshRenderer.sharedMaterials = materials;

			// Update vertices and colors first so that triangles are set after the buffers are valid.
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null)
				{
					continue;
				}
				ActorImage ai = component.Node as ActorImage;
				int vertexCount = ai.VertexCount;
				component.UpdateMesh(vertices, colors, vertexOffset);

				int writeIdx = vertexOffset;
				float[] vertexBuffer = ai.Vertices;
				int aiUVOffset = ai.VertexUVOffset;
				int aiVertexStride = ai.VertexStride;
				int idx = 0;
				for(int i = 0; i < vertexCount; i++)
				{
					m_UVs[writeIdx++] = new Vector2(vertexBuffer[idx+aiUVOffset], 1.0f-vertexBuffer[idx+aiUVOffset+1]);
					idx += aiVertexStride;
				}
				vertexOffset += vertexCount;
			}
			
			m_Mesh.vertices = vertices;
			m_Mesh.colors32 = colors;
			m_Mesh.uv = m_UVs;

			// Unity requires setting the triangles after the mesh is valid (we also do this to ensure bounds are calculated correctly)
			vertexOffset = 0;
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null)
				{
					continue;
				}
				ActorImage ai = component.Node as ActorImage;
				int vertexCount = ai.VertexCount;
				component.UpdateMesh(vertices, colors, vertexOffset);

				int triangleCount = ai.TriangleCount;
				ushort[] triangleBuffer = ai.Triangles;
				int[] triangles = new int[triangleCount*3];
				int triIdx = 0;
				for(int i = 0; i < triangleCount; i++)
				{
					triangles[triIdx] = (int)triangleBuffer[triIdx+2] + vertexOffset;
					triangles[triIdx+1] = (int)triangleBuffer[triIdx+1] + vertexOffset;
					triangles[triIdx+2] = (int)triangleBuffer[triIdx] + vertexOffset;
					triIdx += 3;
				}

				component.m_Triangles = triangles;
				m_Mesh.SetTriangles(ai.RenderCollapsed ? new int[0] : triangles, ai.DrawIndex);
				materials[ai.DrawIndex] = component.m_Material;

				vertexOffset += vertexCount;
			}

			
			// set to show they updated

			MeshFilter filter = GetComponent<MeshFilter>();
			filter.sharedMesh = m_Mesh;
			m_MeshRenderer.hideFlags = HideFlags.HideInInspector;
			filter.hideFlags = HideFlags.HideInInspector;

			m_MeshRenderer.sharedMaterials = materials;
			m_Mesh.RecalculateBounds();
			m_Mesh.RecalculateNormals();
		}

		public new void LateUpdate()
		{
			base.LateUpdate();
			if(m_Mesh == null)
			{
				return;
			}
			m_Mesh.Clear();
			m_Mesh.subMeshCount = m_ImageComponents.Count;
			int vertexOffset = 0;

			Vector3[] vertices = m_UsingA ? m_VerticesB : m_VerticesA;
			Color32[] colors = m_UsingA ? m_ColorsB : m_ColorsA;
			Material[] materials = m_UsingA ? m_MaterialsB : m_MaterialsA;

			m_UsingA = !m_UsingA;

			// Set vertices, colors, and uv first
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null)
				{
					continue;
				}
				ActorImage ai = component.Node as ActorImage;
				int vertexCount = ai.VertexCount;
				component.UpdateMesh(vertices, colors, vertexOffset);
				vertexOffset += vertexCount;
			}
			
			m_Mesh.vertices = vertices;
			m_Mesh.colors32 = colors;
			m_Mesh.uv = m_UVs;

			// Then updated triangles (same as before)...
			foreach(ActorSingleMeshImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null)
				{
					continue;
				}
				ActorImage ai = component.Node as ActorImage;
				m_Mesh.SetTriangles(ai.RenderCollapsed ? new int[0] : component.m_Triangles, ai.DrawIndex);
				materials[ai.DrawIndex] = component.m_Material;
			}

			m_MeshRenderer.sharedMaterials = materials;
		}

#if UNITY_EDITOR

		protected override void UpdateEditorBounds()
		{
		}
#endif
	}
}
