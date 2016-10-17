using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	[SelectionBase]
	public class Actor2D : MonoBehaviour
	{
		[SerializeField]
		private ActorAsset m_ActorAsset;
		[SerializeField]
		private ActorImage2D[] m_ImageNodes;
		[SerializeField]
		private ActorNode2D[] m_SkinnedBoneNodes;
		[SerializeField]
		private GameObject m_DefaultBone;
		[SerializeField]
		private GameObject m_Root;

		private ActorInstance m_ActorInstance;

		public ActorNode2D[] SkinnedBoneNodes
		{
			get
			{
				return m_SkinnedBoneNodes;
			}
		}

		public GameObject DefaultBone
		{
			get
			{
				return m_DefaultBone;
			}
		}

		public GameObject Root
		{
			get
			{
				return m_Root;
			}
		}

		Nima.Animation.ActorAnimation m_TestAnimation;
		float m_TestAnimationTime;
		public void Start()
		{
			EditorApplication.update += TestUpdate;

			if(m_ActorAsset == null || m_ImageNodes == null)
			{
				return;
			}

			if(m_ActorInstance == null)
			{
				m_ActorInstance = new ActorInstance(m_ActorAsset.Actor);
			}

			// Init the bone nodes.
			{
				IEnumerable<ActorNode> nodes = m_ActorInstance.Nodes;
				List<ActorBone> skinnedBones = new List<ActorBone>();
				foreach(ActorNode node in nodes)
				{
					ActorBone ab = node as ActorBone;
					if(ab != null && ab.IsConnectedToImage)
					{
						skinnedBones.Add(ab);
					}
				}
				
				if(skinnedBones.Count != m_SkinnedBoneNodes.Length)
				{
					Debug.Log("Loaded asset does not match Unity serialized Actor2D. Reload " + name + ".");
				}
				for(int i = 0; i < m_SkinnedBoneNodes.Length; i++)
				{
					m_SkinnedBoneNodes[i].Initialize(this, skinnedBones[i]);
				}
			}

			// Sync nodes from actor with ones we had saved with our character.
			int imgNodeIdx = 0;
			foreach(ActorImage ai in m_ActorInstance.ImageNodes)
			{
				if(imgNodeIdx < m_ImageNodes.Length)
				{
					ActorImage2D ai2D = m_ImageNodes[imgNodeIdx];
					if(ai2D == null)
					{
						continue;
					}
					m_ImageNodes[imgNodeIdx].Initialize(this, ai);
				}
				imgNodeIdx++;
			}
			
			//Initialize(m_ActorAsset);
#if UNITY_EDITOR
			UpdateEditorBounds();
#endif
			TestAnimation();

		}

		public void TestAnimation()
		{
			m_TestAnimationTime = 0.0f;
			if(m_ActorAsset == null || m_ActorAsset.Actor == null)
			{
				return;
			}
			
			m_TestAnimation = m_ActorAsset.Actor.GetAnimation("Head Turn");
			if(m_TestAnimation != null)
			{
				Debug.Log("FOUND ANIMATION RUN");
			}
			else
			{
				Debug.Log("NO ANIMATION");
			}
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
			TestAnimation();
		}
#endif

		void OnDestroy() 
		{
			EditorApplication.update -= TestUpdate;
			if(m_SkinnedBoneNodes != null)
			{
				foreach(ActorNode2D node in m_SkinnedBoneNodes)
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
			if(m_ImageNodes != null)
			{
				foreach(ActorImage2D node in m_ImageNodes)
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

#if UNITY_EDITOR
		public void InitializeFromAsset(ActorAsset actorAsset)
		{
			m_ActorAsset = actorAsset;
			if(!actorAsset.Loaded && !actorAsset.Load())
			{
				Debug.Log("Bad actorAsset referenced by Actor2D.");
				return;
			}

			m_Root = new GameObject("Root");
			m_Root.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			m_Root.transform.parent = gameObject.transform;
			m_Root.transform.localScale = new Vector3(0.01f,0.01f,0.01f);

			m_ActorInstance = new ActorInstance(m_ActorAsset.Actor);
			
			// Instance actor bones first as our image nodes need to know about them if they are skinned.
			{
				if(m_SkinnedBoneNodes != null)
				{
					foreach(ActorNode2D node in m_SkinnedBoneNodes)
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
				IEnumerable<ActorNode> nodes = m_ActorInstance.Nodes;
				List<ActorBone> skinnedBones = new List<ActorBone>();
				foreach(ActorNode node in nodes)
				{
					ActorBone ab = node as ActorBone;
					if(ab != null && ab.IsConnectedToImage)
					{
						skinnedBones.Add(ab);
					}
				}
				
				m_SkinnedBoneNodes = new ActorNode2D[skinnedBones.Count];
				m_DefaultBone = new GameObject("Default Bone");
				m_DefaultBone.transform.parent = m_Root.transform;
				m_DefaultBone.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				for(int i = 0; i < skinnedBones.Count; i++)
				{
					ActorBone ab = skinnedBones[i];

					GameObject go = new GameObject(ab.Name, typeof(ActorNode2D));
					go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					go.transform.parent = m_Root.transform;
					m_SkinnedBoneNodes[i] = go.GetComponent<ActorNode2D>();
					m_SkinnedBoneNodes[i].Initialize(this, ab);
				}
			}

			if(m_ImageNodes != null)
			{
				foreach(ActorImage2D node in m_ImageNodes)
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

			m_ImageNodes = new ActorImage2D[m_ActorInstance.ImageNodeCount];

			int imgNodeIdx = 0;
			foreach(ActorImage ai in m_ActorInstance.ImageNodes)
			{
				if(ai.VertexCount == 0)
				{
					imgNodeIdx++;
					continue;
				}
				Mesh mesh = m_ActorAsset.GetMesh(imgNodeIdx);
				bool hasBones = ai.ConnectedBonesCount > 0;

				GameObject go = hasBones ? 
									new GameObject(ai.Name, typeof(SkinnedMeshRenderer), typeof(ActorImage2D)) : 
									new GameObject(ai.Name, typeof(MeshFilter), typeof(MeshRenderer), typeof(ActorImage2D));
				//go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset;//HideFlags.HideAndDontSave;
				go.transform.parent = m_Root.transform;

				ActorImage2D actorImage = go.GetComponent<ActorImage2D>();
				m_ImageNodes[imgNodeIdx] = actorImage;

				if(hasBones)
				{
					go.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
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
						material = new Material(material);
						break;
				}
				
				renderer.sharedMaterial = material;

				actorImage.Initialize(this, ai);
				imgNodeIdx++;
			}

			UpdateEditorBounds();
		}

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
		}

		public Nima.Actor Actor
		{
			get
			{
				return m_ActorInstance;
			}
		}

		public void TestUpdate()
		{
			m_TestAnimationTime += 0.01f;//Time.deltaTime*0.1f;
			if(m_TestAnimationTime > 1.82f)
			{
				m_TestAnimationTime = 0.0f;
			}
			//m_TestAnimationTime = 19.0f/60.0f;
			if(m_TestAnimation != null)
			{
				m_TestAnimation.Apply(m_TestAnimationTime, m_ActorInstance.AllNodes, 1.0f);
				foreach(ActorNode2D n in m_SkinnedBoneNodes)
				{
					if(n == null)
					{
						continue;
					}
					n.Update();
				}
				foreach(ActorImage2D n in m_ImageNodes)
				{
					if(n == null)
					{
						continue;
					}
					n.Update();
				}
			}
		}

		public void Update()
		{
#if UNITY_EDITOR
			UpdateEditorBounds();
#endif
		}
	}
}