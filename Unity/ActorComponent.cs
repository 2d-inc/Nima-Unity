using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	[SelectionBase]
	public class ActorComponent : MonoBehaviour
	{
		[SerializeField]
		private ActorAsset m_ActorAsset;
		[SerializeField]
		private int m_RenderQueueOffset;

		private ActorNodeComponent[] m_Nodes;
		//private ActorImageComponent[] m_ImageNodes;
		//private ActorNodeComponent[] m_SkinnedBoneNodes;
		private GameObject m_DefaultBone;

		private Actor m_ActorInstance;

		public ActorNodeComponent[] Nodes
		{
			get
			{
				return m_Nodes;
			}
		}

		public GameObject DefaultBone
		{
			get
			{
				return m_DefaultBone;
			}
		}

		public int RenderQueueOffset
		{
			get
			{
				return m_RenderQueueOffset;
			}
			set
			{
				m_RenderQueueOffset = value;
			}
		}

		public void Awake()
		{
			if(m_ActorAsset == null)
			{
				return;
			}

			if(m_ActorInstance == null)
			{
				InitializeFromAsset(m_ActorAsset);
			}
#if UNITY_EDITOR
			UpdateEditorBounds();
#endif
		}

#if UNITY_EDITOR
		public void SetActorAsset(ActorAsset asset)
		{
			m_ActorAsset = asset;
			InitializeFromAsset(m_ActorAsset);
		}

		public void Reload()
		{
			if(m_ActorAsset != null && m_ActorAsset.Load())
			{
				InitializeFromAsset(m_ActorAsset);
			}
		}
#endif

		void RemoveNodes()
		{
			DestroyImmediate(m_DefaultBone);
			if(m_Nodes != null)
			{
				foreach(ActorNodeComponent node in m_Nodes)
				{
					if(node == null)
					{
						continue;
					}
					if(node.gameObject != null)
					{
						DestroyImmediate(node.gameObject);
					}
					else
					{
						DestroyImmediate(node);
					}
				}
			}
		}

		void OnDestroy() 
		{
			RemoveNodes();
		}


		public void InitializeFromAsset(ActorAsset actorAsset)
		{
			HideFlags hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontUnloadUnusedAsset | HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			m_ActorAsset = actorAsset;

			if(!actorAsset.Loaded && !actorAsset.Load())
			{
				Debug.Log("Bad actorAsset referenced by Actor2D.");
				return;
			}

			RemoveNodes();

			m_ActorInstance = new Actor();
			m_ActorInstance.Copy(m_ActorAsset.Actor);
			
			// Instance actor bones first as our image nodes need to know about them if they are skinned.
			{
				ActorNode[] allNodes = m_ActorInstance.AllNodes;
				m_Nodes = new ActorNodeComponent[allNodes.Length];
				m_DefaultBone = new GameObject("Default Bone");
				m_DefaultBone.transform.parent = gameObject.transform;
				m_DefaultBone.hideFlags = hideFlags;
				
				int imgNodeIdx = 0;
				for(int i = 0; i < allNodes.Length; i++)
				{
					ActorNode an = allNodes[i];
					GameObject go;
					ActorImage ai = an as ActorImage;
					if(ai != null)
					{
						if(ai.VertexCount == 0)
						{
							go = new GameObject(an.Name, typeof(ActorNodeComponent));
						}
						else
						{
							Mesh mesh = m_ActorAsset.GetMesh(imgNodeIdx);
							bool hasBones = ai.ConnectedBoneCount > 0;

							go = hasBones ? 
											new GameObject(ai.Name, typeof(SkinnedMeshRenderer), typeof(ActorImageComponent)) : 
											new GameObject(ai.Name, typeof(MeshFilter), typeof(MeshRenderer), typeof(ActorImageComponent));

							ActorImageComponent actorImage = go.GetComponent<ActorImageComponent>();
							if(ai.DoesAnimationVertexDeform)
			 				{
			 					// Clone the vertex array if we deform.
			 					Mesh clonedMesh = new Mesh();
			 					clonedMesh.vertices = (Vector3[]) mesh.vertices.Clone();
			 					clonedMesh.uv = mesh.uv;
			 					clonedMesh.boneWeights = mesh.boneWeights;
			 					clonedMesh.bindposes = mesh.bindposes;
			 					clonedMesh.triangles = mesh.triangles;
			 					clonedMesh.RecalculateNormals();
			 					mesh = clonedMesh;
			 				}
							if(hasBones)
							{
								Mesh skinnedMesh = new Mesh();
			 					skinnedMesh.vertices = mesh.vertices;
			 					skinnedMesh.uv = mesh.uv;
			 					skinnedMesh.boneWeights = mesh.boneWeights;
			 					skinnedMesh.triangles = mesh.triangles;
			 					skinnedMesh.bindposes = mesh.bindposes;
			 
			 					go.GetComponent<SkinnedMeshRenderer>().sharedMesh = skinnedMesh;
							}
							else
							{
								MeshFilter meshFilter = go.GetComponent<MeshFilter>();
								meshFilter.sharedMesh = mesh;
							}

							Renderer renderer = go.GetComponent<Renderer>();

							Material material = m_ActorAsset.GetMaterial(ai.TextureIndex);
							switch(ai.BlendMode)
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
									Material overrideMaterial = new Material(Shader.Find("Nima/Normal"));
									overrideMaterial.mainTexture = material.mainTexture;
									material = overrideMaterial;
									break;
								}
							}
							
							renderer.sharedMaterial = material;
						}
						imgNodeIdx++;
					}
					else
					{
						go = new GameObject(an.Name, typeof(ActorNodeComponent));
					}
					
					go.hideFlags = hideFlags;
					m_Nodes[i] = go.GetComponent<ActorNodeComponent>();
				}

				// After they are all created, initialize them.
				for(int i = 0; i < allNodes.Length; i++)
				{
					m_Nodes[i].Initialize(this, allNodes[i]);
				}
			}
#if UNITY_EDITOR
			UpdateEditorBounds();
#endif
		}
#if UNITY_EDITOR
		private void UpdateEditorBounds()
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
				foreach (Transform child in transform)
				{
					Renderer r = child.gameObject.GetComponent<Renderer>();
					if(r == null)
					{
						continue;
					}
					bounds.Encapsulate(r.bounds);
				}
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
		public ActorAsset Asset
		{
			get
			{
				return m_ActorAsset;
			}
			set
			{
				m_ActorAsset = value;
				Reload();
			}
		}

		public Nima.Actor ActorInstance
		{
			get
			{
				return m_ActorInstance;
			}
		}

		public void LateUpdate()
		{
			if(m_ActorInstance != null)
			{
				m_ActorInstance.Advance(Time.deltaTime);
			}
			
			if(m_Nodes != null)
			{
				foreach(ActorNodeComponent node in m_Nodes)
				{
					if(node == null)
					{
						continue;
					}
					node.UpdateTransform();
				}
			}
#if UNITY_EDITOR
			UpdateEditorBounds();
#endif
		}
	}
}