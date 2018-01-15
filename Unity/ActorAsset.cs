using System;
using System.IO;
using System.Collections.Generic;
using Nima.Math2D;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class ActorAsset : ScriptableObject
	{
		[SerializeField]
		private TextAsset m_RawAsset;

		[NonSerialized]
		private Nima.Actor m_Actor;
		[NonSerialized]
		private Mesh[] m_Meshes;
		[NonSerialized]
		private ActorImage[] m_ImageNodes;

		// Hideous hack. We store a test mesh to detect when Unity decides to null out all meshes for us.
		// Still not sure why this is happening when we're holding a reference to them, some internal optimization.
		private Mesh m_TestMesh;

		[SerializeField]
		public Material[] m_TextureMaterials;

		public bool Load(TextAsset asset)
		{
			m_TestMesh = new Mesh();
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
				return m_Actor != null && m_TestMesh != null;
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
			if(idx < 0 || idx >= m_TextureMaterials.Length)
			{
				return null;
			}
			return m_TextureMaterials[idx];
		}
		
		public const float NimaToUnityScale = 0.01f;

		private void InitializeActor()
		{
			IEnumerable<ActorNode> nodes = m_Actor.Nodes;
			//m_Actor.Root.ScaleX = NimaToUnityScale;
			//m_Actor.Root.ScaleY = NimaToUnityScale;

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
					if(ai.DoesAnimationVertexDeform)
					{
						mesh.MarkDynamic();
					}
					m_Meshes[imgIdx] = mesh;
					imgIdx++;

					int aiVertexCount = ai.VertexCount;
					int aiVertexStride = ai.VertexStride;
					int aiPositionOffset = ai.VertexPositionOffset;
					int aiUVOffset = ai.VertexUVOffset;
					float[] vertexBuffer = ai.Vertices;

					Vector3[] vertices = new Vector3[aiVertexCount];
					Vector2[] uvs = new Vector2[aiVertexCount];
					Color32[] colors = new Color32[aiVertexCount];

					if(aiVertexStride == 12)
					{
						// We have bone weights.
						int aiVertexBoneIndexOffset = ai.VertexBoneIndexOffset;
						int aiVertexBoneWeightOffset = ai.VertexBoneWeightOffset;
						int idx = 0;

						Mat2D newWorldOverride = new Mat2D();
						// Change the override to scale by our UnityScale
						Mat2D.Multiply(newWorldOverride, m_Actor.Root.Transform, ai.WorldTransformOverride);
						ai.WorldTransformOverride = newWorldOverride;

						// We don't use the bind transforms in the regular render path as we let Unity do the deform
						// But in the Canvas render path we need to manually deform the vertices so we need to have our
						// bind matrices in the correct world transform.
						ai.TransformBind(m_Actor.Root.Transform);

						if(ai.DoesAnimationVertexDeform)
						{
							// Update the vertex deforms too.
							ai.TransformDeformVertices(ai.WorldTransform);
						}

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
							colors[j] = new Color32(255, 255, 255, (byte)Math.Round(255*ai.RenderOpacity));

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

						foreach(ActorImage.BoneConnection bc in ai.BoneConnections)
						{
							Matrix4x4 mat = bindPoses[bidx];
							Mat2D ibind = bc.InverseBind;

							mat[0,0] = ibind[0];
							mat[1,0] = ibind[1];
							mat[0,1] = ibind[2];
							mat[1,1] = ibind[3];
							mat[0,3] = ibind[4];
							mat[1,3] = ibind[5];
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
							colors[j] = new Color32(255, 255, 255, (byte)Math.Round(255*ai.RenderOpacity));

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
					mesh.colors32 = colors;
					mesh.RecalculateBounds();
					mesh.RecalculateNormals();

					// We don't need to hold the geometry data in the node now that it's in our buffers.
					// ai.DisposeGeometry(); // We now do need to hold onto it as we manually deform.
				}
			}

			// Find any vertex deform animation keyframes and update them to scale the vertices as is necessary for the skinned path.
			foreach(Nima.Animation.ActorAnimation animation in m_Actor.Animations)
			{
				if(animation == null || animation.AnimatedComponents == null)
				{
					continue;
				}
				foreach(Nima.Animation.ComponentAnimation componentAnimation in animation.AnimatedComponents)
				{
					ActorNode node = m_Actor[componentAnimation.ComponentIndex] as ActorNode;
					if(node == null)
					{
						continue;
					}
					ActorImage actorImage = node as ActorImage;
					if(actorImage != null && actorImage.ConnectedBoneCount == 0)
					{
						// This image is in the hierarchy, no need to transform the vertices.
						continue;
					}
					foreach(Nima.Animation.PropertyAnimation propertyAnimation in componentAnimation.Properties)
					{
						if(propertyAnimation != null && propertyAnimation.PropertyType == Nima.Animation.PropertyTypes.VertexDeform)
						{
							foreach(Nima.Animation.KeyFrame keyFrame in propertyAnimation.KeyFrames)
							{
								(keyFrame as Nima.Animation.KeyFrameVertexDeform).TransformVertices(node.WorldTransform);
							}
						}
					}
				}
			}
		}
	}
}