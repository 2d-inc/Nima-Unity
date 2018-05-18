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
	public class ActorCanvasComponent : ActorBaseComponent
	{
		private List<ActorCanvasImageComponent> m_ImageComponents;
		private ActorCanvasImageComponent[] m_SortedImageComponents;
		// We generate a root which will store our drawn components.
		private GameObject m_DrawOrderRoot;
		public GameObject DrawOrderRoot
		{
			get
			{
				return m_DrawOrderRoot;
			}
		}

		protected override bool InstanceColliders
		{
			get
			{
				return false;
			}
		}

		protected override void RemoveNodes()
		{
			if(m_ImageComponents != null)
			{
				foreach(ActorCanvasImageComponent c in m_ImageComponents)
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
			DestroyImmediate(m_DrawOrderRoot);	
			m_DrawOrderRoot = null;
			base.RemoveNodes();
		}

		protected override void OnActorInstanced()
		{
			HideFlags hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontUnloadUnusedAsset | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			m_ImageComponents = new List<ActorCanvasImageComponent>();
			m_DrawOrderRoot = new GameObject("Draw Order Root", typeof(RectTransform));
			m_DrawOrderRoot.transform.SetParent(gameObject.transform, false);
			m_DrawOrderRoot.hideFlags = hideFlags;
		}

		/// <summary>
		/// Force set the material for a specific blend mode. Note that this leaves texture atlas and shader selection up to you!
		/// </summary>
		public void SetMaterial(Material material, BlendModes blendMode)
		{
			foreach(ActorCanvasImageComponent component in m_ImageComponents)
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
			foreach(ActorCanvasImageComponent component in m_ImageComponents)
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
			foreach(ActorCanvasImageComponent component in m_ImageComponents)
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
			GameObject gameObject = new GameObject(actorImage.Name, typeof(ActorNodeComponent));
			ActorNodeComponent nodeComponent = gameObject.GetComponent<ActorNodeComponent>();
			
			Mesh mesh = m_ActorAsset.GetMesh(imageNodeIndex);
			bool hasBones = actorImage.ConnectedBoneCount > 0;

			GameObject go = new GameObject(actorImage.Name, typeof(RectTransform), typeof(CanvasRenderer), typeof(ActorCanvasImageComponent));

			ActorCanvasImageComponent actorImageComponent = go.GetComponent<ActorCanvasImageComponent>();
			actorImageComponent.Node = actorImage;
			go.transform.SetParent(m_DrawOrderRoot.transform, false);
			m_ImageComponents.Add(actorImageComponent);

			// Clone the vertex array alway right now so we can update opacity
			Mesh clonedMesh = new Mesh();
			clonedMesh.vertices = (Vector3[]) mesh.vertices.Clone();
			clonedMesh.uv = mesh.uv;
			clonedMesh.triangles = mesh.triangles;
			clonedMesh.colors32 = (UnityEngine.Color32[]) mesh.colors32.Clone();
			clonedMesh.RecalculateNormals();
			clonedMesh.MarkDynamic();
			mesh = clonedMesh;

			Material material = m_ActorAsset.GetMaterial(actorImage.TextureIndex);

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
			
			actorImageComponent.m_Mesh = mesh;
			actorImageComponent.m_Material = material;

			return nodeComponent;
		}

		protected override void OnActorInitialized()
		{
			m_SortedImageComponents = new ActorCanvasImageComponent[m_ImageComponents.Count];
		}

		public new void LateUpdate()
		{
			base.LateUpdate();

			if(m_SortedImageComponents == null)
			{
				return;
			}

			foreach(ActorCanvasImageComponent component in m_ImageComponents)
			{
				if(component == null || component.Node == null)
				{
					continue;
				}
				//Debug.Log("DRAWID" + component.Node.DrawIndex + " " + m_SortedImageComponents.Length);
				m_SortedImageComponents[component.Node.DrawIndex] = component;
			}
			foreach(ActorCanvasImageComponent component in m_SortedImageComponents)
			{
				if(component == null)
				{
					continue;
				}
				component.gameObject.transform.SetAsLastSibling();
				component.UpdateMesh();
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
			return;
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
