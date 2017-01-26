using System;
using System.IO;
using System.Collections.Generic;
using Nima.Math2D;
using UnityEngine;

namespace Nima.Unity
{
	public class ActorAsset : ScriptableObject
	{
		[SerializeField]
		private TextAsset m_RawAsset;

		private Nima.Actor m_Actor;
		private Mesh[] m_Meshes;
		private ActorImage[] m_ImageNodes;
		public Material[] m_TextureMaterials;

		public void OnEnable()
		{
			Load();
		}

		public bool Load(TextAsset asset)
		{
			m_RawAsset = asset;
			using (MemoryStream ms = new MemoryStream(asset.bytes))
			{
				m_Actor = new Actor();
				m_Actor.LoadFrom(ms);
				if(m_Actor != null)
				{
					InitializeActor();
					return true;
				}
			}
			return false;
		}

		public bool Load()
		{
			if(m_RawAsset == null)
			{
				return false;
			}
			return Load(m_RawAsset);
		}

		public Nima.Actor Actor
		{
			get
			{
				return m_Actor;
			}
		}

		public bool Loaded
		{
			get
			{
				return m_Actor != null;
			}
		}

		public ActorImage[] ImageNodes
		{
			get
			{
				return m_ImageNodes;
			}
		}

		public Mesh GetMesh(int idx)
		{
			return m_Meshes[idx];
		}

		public Material GetMaterial(int idx)
		{
			return m_TextureMaterials[idx];
		}

		private void InitializeActor()
		{
			const float NimaToUnityScale = 0.01f;

			IEnumerable<ActorNode> nodes = m_Actor.Nodes;
			m_Actor.Root.ScaleX = NimaToUnityScale;
			m_Actor.Root.ScaleY = NimaToUnityScale;

			int imgNodeCount = 0;
			foreach(ActorNode node in nodes)
			{
				ActorImage ai = node as ActorImage;
				if(ai != null)
				{
					imgNodeCount++;
				}
			}
			m_ImageNodes = new ActorImage[imgNodeCount];
			m_Meshes = new Mesh[imgNodeCount];

			int imgIdx = 0;
			foreach(ActorNode node in nodes)
			{
				ActorImage ai = node as ActorImage;
				if(ai != null)
				{
					m_ImageNodes[imgIdx] = ai;

					Mesh mesh = new Mesh();
					m_Meshes[imgIdx] = mesh;
					imgIdx++;

					int aiVertexCount = ai.VertexCount;
					int aiVertexStride = ai.VertexStride;
					int aiPositionOffset = ai.VertexPositionOffset;
					int aiUVOffset = ai.VertexUVOffset;
					float[] vertexBuffer = ai.Vertices;

					Vector3[] vertices = new Vector3[aiVertexCount];
					Vector2[] uvs = new Vector2[aiVertexCount];
					if(aiVertexStride == 12)
					{
						// We have bone weights.
						int aiVertexBoneIndexOffset = ai.VertexBoneIndexOffset;
						int aiVertexBoneWeightOffset = ai.VertexBoneWeightOffset;
						int idx = 0;

						Mat2D newWorldOverride = new Mat2D();
						Mat2D.Multiply(newWorldOverride, m_Actor.Root.Transform, ai.WorldTransformOverride);
						ai.WorldTransformOverride = newWorldOverride;
						// Unity expects skinned mesh vertices to be in bone world space (character world).
						// So we transform them to our world transform.
						BoneWeight[] weights = new BoneWeight[aiVertexCount];

						Mat2D wt = ai.WorldTransform;

						for(int j = 0; j < aiVertexCount; j++)
						{
							float x = vertexBuffer[idx+aiPositionOffset];
							float y = vertexBuffer[idx+aiPositionOffset+1];
							vertices[j] = new Vector3(wt[0] * x + wt[2] * y + wt[4], wt[1] * x + wt[3] * y + wt[5], 0.0f);

							uvs[j] = new Vector2(vertexBuffer[idx+aiUVOffset], 1.0f-vertexBuffer[idx+aiUVOffset+1]);

							BoneWeight weight = new BoneWeight();
							weight.boneIndex0 = (int)vertexBuffer[idx+aiVertexBoneIndexOffset];
							weight.boneIndex1 = (int)vertexBuffer[idx+aiVertexBoneIndexOffset+1];
							weight.boneIndex2 = (int)vertexBuffer[idx+aiVertexBoneIndexOffset+2];
							weight.boneIndex3 = (int)vertexBuffer[idx+aiVertexBoneIndexOffset+3];
							weight.weight0 = vertexBuffer[idx+aiVertexBoneWeightOffset];
							weight.weight1 = vertexBuffer[idx+aiVertexBoneWeightOffset+1];
							weight.weight2 = vertexBuffer[idx+aiVertexBoneWeightOffset+2];
							weight.weight3 = vertexBuffer[idx+aiVertexBoneWeightOffset+3];
							weights[j] = weight;

							idx += aiVertexStride;
						}
						mesh.vertices = vertices;
						mesh.uv = uvs;
						mesh.boneWeights = weights;

						// Set up bind poses.
						int bindBoneCount = ai.ConnectedBoneCount + 1; // Always an extra bone for the root transform (identity).

						Matrix4x4[] bindPoses = new Matrix4x4[bindBoneCount];
						for(int i = 0; i < bindBoneCount; i++)
						{
							Matrix4x4 mat = new Matrix4x4();
							mat = Matrix4x4.identity;
							bindPoses[i] = mat;
						}

						int bidx = 1;
						Mat2D w = m_Actor.Root.Transform;
						Mat2D temp = new Mat2D();

						foreach(ActorImage.BoneConnection bc in ai.BoneConnections)
						{
							Matrix4x4 mat = bindPoses[bidx];

							// Transform the bind by our root world (because we scale it) and then re-invert.
							Mat2D.Multiply(temp, w, bc.Bind);
							Mat2D.Invert(temp, temp);

							mat[0,0] = temp[0];
							mat[1,0] = temp[1];
							mat[0,1] = temp[2];
							mat[1,1] = temp[3];
							mat[0,3] = temp[4];
							mat[1,3] = temp[5];
					        bindPoses[bidx] = mat;
					        bidx++;
						}
						mesh.bindposes = bindPoses;
					}
					else
					{
						int idx = 0;
						for(int j = 0; j < aiVertexCount; j++)
						{
							vertices[j] = new Vector3(vertexBuffer[idx+aiPositionOffset], vertexBuffer[idx+aiPositionOffset+1], 0);
							uvs[j] = new Vector2(vertexBuffer[idx+aiUVOffset], 1.0f-vertexBuffer[idx+aiUVOffset+1]);

							idx += aiVertexStride;
						}
						mesh.vertices = vertices;
						mesh.uv = uvs;
					}


					int triangleCount = ai.TriangleCount;
					ushort[] triangleBuffer = ai.Triangles;

					int[] tris = new int[triangleCount*3];
					int triIdx = 0;
					for(int j = 0; j < triangleCount; j++)
					{
						tris[triIdx] = (int)triangleBuffer[triIdx+2];
						tris[triIdx+1] = (int)triangleBuffer[triIdx+1];
						tris[triIdx+2] = (int)triangleBuffer[triIdx];
						triIdx += 3;
					}

					mesh.triangles = tris;
					mesh.RecalculateBounds();
					mesh.RecalculateNormals();

					// We don't need to hold the geometry data in the node now that it's in our buffers.
					//ai.DisposeGeometry();
				}
			}
		}
	}
}