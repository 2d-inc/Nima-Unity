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
	public class ActorComponent : ActorBaseComponent, IRenderSortableComponent
	{
		[SerializeField]
		private int m_SortingLayerID;
		[SerializeField]
		private int m_SortingOrder;

		private GameObject m_DefaultBone;


		public GameObject DefaultBone
		{
			get
			{
				return m_DefaultBone;
			}
		}

		public int SortingOrder
		{
			get
			{
				return m_SortingOrder;
			}
			set
			{
				m_SortingOrder = value;
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
				if(m_Nodes != null)
				{
					foreach(ActorNodeComponent c in m_Nodes)
					{
						ActorImageComponent imageComponent = c as ActorImageComponent;
						if(imageComponent != null)
						{
							Renderer renderer = imageComponent.gameObject.GetComponent<Renderer>();
							renderer.sortingLayerID = m_SortingLayerID;
						}
					}
				}
			}
		}

		protected override void RemoveNodes()
		{
			base.RemoveNodes();
			DestroyImmediate(m_DefaultBone);
		}

		protected override void OnActorInstanced()
		{
			HideFlags hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontUnloadUnusedAsset | HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			m_DefaultBone = new GameObject("Default Bone");
			m_DefaultBone.transform.SetParent(gameObject.transform, false);
			m_DefaultBone.hideFlags = hideFlags;
		}

		protected override ActorNodeComponent MakeImageNodeComponent(int imageNodeIndex, ActorImage actorImage)
		{
			if(actorImage.VertexCount == 0)
			{
				GameObject go = new GameObject(actorImage.Name, typeof(ActorNodeComponent));
				ActorNodeComponent nodeComponent = go.GetComponent<ActorNodeComponent>();
				return nodeComponent;
			}
			else
			{
				Mesh mesh = m_ActorAsset.GetMesh(imageNodeIndex);
				bool hasBones = actorImage.ConnectedBoneCount > 0;

				GameObject go = hasBones ? 
								new GameObject(actorImage.Name, typeof(SkinnedMeshRenderer), typeof(ActorImageComponent)) : 
								new GameObject(actorImage.Name, typeof(MeshFilter), typeof(MeshRenderer), typeof(ActorImageComponent));
				

				ActorImageComponent actorImageComponent = go.GetComponent<ActorImageComponent>();
				// Clone the vertex array alway right now so we can update opacity
				// In future we could check if this node animates opacity as we did for vertex deform
				//if(actorImage.DoesAnimationVertexDeform)
 				{
 					// Clone the vertex array if we deform.
 					Mesh clonedMesh = new Mesh();
 					clonedMesh.vertices = (Vector3[]) mesh.vertices.Clone();
 					clonedMesh.uv = mesh.uv;
 					clonedMesh.boneWeights = mesh.boneWeights;
 					clonedMesh.bindposes = mesh.bindposes;
 					clonedMesh.triangles = mesh.triangles;
 					clonedMesh.colors32 = (UnityEngine.Color32[]) mesh.colors32.Clone();
 					clonedMesh.RecalculateNormals();
 					clonedMesh.MarkDynamic();
 					mesh = clonedMesh;
 				}
				if(hasBones)
				{
					Mesh skinnedMesh = new Mesh();
 					skinnedMesh.vertices = mesh.vertices;
 					skinnedMesh.uv = mesh.uv;
 					skinnedMesh.boneWeights = mesh.boneWeights;
 					skinnedMesh.triangles = mesh.triangles;
 					skinnedMesh.colors32 = (UnityEngine.Color32[]) mesh.colors32.Clone();
 					skinnedMesh.bindposes = mesh.bindposes;
 
 					go.GetComponent<SkinnedMeshRenderer>().sharedMesh = skinnedMesh;
				}
				else
				{
					MeshFilter meshFilter = go.GetComponent<MeshFilter>();
					meshFilter.sharedMesh = mesh;
				}

				Renderer renderer = go.GetComponent<Renderer>();

				Material material = m_ActorAsset.GetMaterial(actorImage.TextureIndex);
				renderer.sortingLayerID = m_SortingLayerID;

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
						/*Material overrideMaterial = new Material(Shader.Find("Nima/Normal"));
						overrideMaterial.mainTexture = material.mainTexture;
						material = overrideMaterial;*/
						break;
					}
				}
				
				renderer.sharedMaterial = material;
				return actorImageComponent;
			}
		}

#if UNITY_EDITOR
		private Bounds RecurseEncapsulate(Bounds bounds, Transform transform)
		{
			foreach (Transform child in transform)
			{
				bounds = RecurseEncapsulate(bounds, child.gameObject.transform);
				Renderer r = child.gameObject.GetComponent<Renderer>();
				if(r == null)
				{
					continue;
				}
				bounds.Encapsulate(r.bounds);
			}
			return bounds;
		}

		protected override void UpdateEditorBounds()
		{
			if(!Application.isPlaying)
			{
				Quaternion currentRotation = this.transform.rotation;
				Vector3 currentScale = this.transform.localScale;
				Vector3 currentPosition = this.transform.position;

		        this.transform.rotation = Quaternion.Euler(0f,0f,0f);
		        this.transform.localScale = new Vector3(1f,1f,1f);
		        this.transform.position = new Vector3(0f,0f,0f);

				Bounds bounds = new Bounds(this.transform.position, Vector3.zero);
				bounds = RecurseEncapsulate(bounds, transform);
				
				MeshFilter filter = GetComponent<MeshFilter>();
				if(filter == null)
				{
					filter = gameObject.AddComponent<MeshFilter>();
					Renderer renderer = gameObject.AddComponent<MeshRenderer>();
					renderer.hideFlags = HideFlags.HideInInspector;
					filter.hideFlags = HideFlags.HideInInspector;
				}

				if(filter.sharedMesh == null)
				{
					filter.sharedMesh = new Mesh();
				}
				//Vector3 localCenter = bounds.center - this.transform.position;
         		//bounds.center = localCenter;
				GetComponent<MeshFilter>().sharedMesh.bounds = bounds;
				this.transform.rotation = currentRotation;
				this.transform.localScale = currentScale;
				this.transform.position = currentPosition;
			}
		}
#endif
	}
}