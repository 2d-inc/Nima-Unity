using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nima.Unity
{
	public abstract class ActorBaseComponent : MonoBehaviour
	{
		[SerializeField]
		protected ActorAsset m_ActorAsset;
		protected IActorAnimationController[] m_AnimationControllers;
		protected IActorManipulationController[] m_ManipulationControllers;
		protected List<IActorFinalFormDependent> m_FinalFormDependents;

		protected ActorNodeComponent[] m_Nodes;
		protected Actor m_ActorInstance;

		public ActorNodeComponent[] Nodes
		{
			get
			{
				return m_Nodes;
			}
		}

		protected virtual bool InstanceColliders
		{
			get
			{
				return true;
			}
		}
		
#if UNITY_EDITOR
		protected abstract void UpdateEditorBounds();

		void OnDrawGizmos() 
		{
			Gizmos.DrawIcon(transform.position, "ActorAsset Icon.png", false);
		}

#endif

		public void AddFinalFormDependent(IActorFinalFormDependent dependent)
		{
			m_FinalFormDependents.Add(dependent);
		}

		public void RemoveFinalFormDependent(IActorFinalFormDependent dependent)
		{
			if(m_FinalFormDependents != null)
			{
				m_FinalFormDependents.Remove(dependent);
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

			m_AnimationControllers = gameObject.GetComponents<IActorAnimationController>();
			m_ManipulationControllers = gameObject.GetComponents<IActorManipulationController>();
			m_FinalFormDependents = new List<IActorFinalFormDependent>();
		
			if(m_ManipulationControllers != null)
			{
				foreach(IActorManipulationController manipulationController in m_ManipulationControllers)
				{
					if(manipulationController != null)
					{
						manipulationController.SetupManipulator(this);
					}
				}
			}
#if UNITY_EDITOR
			UpdateEditorBounds();
#endif
		}

		public ActorNode GetActorNode(string name)
		{
			return m_ActorInstance.GetNode(name);
		}

		public GameObject GetActorGameObject(string name)
		{
			if(m_Nodes == null)
			{
				return null;
			}
			foreach(ActorNodeComponent nodeComponent in m_Nodes)
			{
				if(nodeComponent != null && nodeComponent.Node != null && nodeComponent.Node.Name == name)
				{
					return nodeComponent.gameObject;
				}
			}
			return null;
		}

		public ActorNodeComponent GetActorNodeComponent(string name)
		{
			if(m_Nodes == null)
			{
				return null;
			}
			foreach(ActorNodeComponent nodeComponent in m_Nodes)
			{
				if(nodeComponent != null && nodeComponent.Node != null && nodeComponent.Node.Name == name)
				{
					return nodeComponent;
				}
			}
			return null;
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
		protected virtual void RemoveNodes()
		{
			m_AnimationControllers = null;
			m_ManipulationControllers = null;
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
				m_Nodes = null;
			}
		}

		void OnDestroy() 
		{
			RemoveNodes();
		}

		protected virtual void OnActorInstanced()
		{

		}

		protected virtual void OnActorInitialized()
		{

		}

		protected abstract ActorNodeComponent MakeImageNodeComponent(int imageNodeIndex, ActorImage actorImage);

		public void InitializeFromAsset(ActorAsset actorAsset)
		{
			HideFlags hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontUnloadUnusedAsset | HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			m_ActorAsset = actorAsset;

			if(!actorAsset.Loaded && !actorAsset.Load())
			{
#if UNITY_EDITOR
				Debug.Log("Bad actorAsset referenced by Actor2D.");
#endif
				return;
			}

			RemoveNodes();

			m_ActorInstance = new Actor();
			m_ActorInstance.Copy(m_ActorAsset.Actor);
			OnActorInstanced();
			
			// Instance actor bones first as our image nodes need to know about them if they are skinned.
			{
				//ActorNode[] allNodes = m_ActorInstance.AllNodes;
				IList<Nima.ActorComponent> actorComponents = m_ActorInstance.Components;
				m_Nodes = new ActorNodeComponent[actorComponents.Count];
				
				int imgNodeIdx = 0;
				for(int i = 0; i < actorComponents.Count; i++)
				{
					Nima.ActorComponent ac = actorComponents[i];
					ActorNode an = ac as ActorNode;
					if(an == null)
					{
						continue;
					}
					GameObject go = null;
					ActorImage ai = an as ActorImage;
					if(ai != null)
					{
						ActorNodeComponent nodeComponent = MakeImageNodeComponent(imgNodeIdx, ai);
						nodeComponent.Node = ai;
						go = nodeComponent.gameObject;
						m_Nodes[i] = nodeComponent;
						imgNodeIdx++;
					}
					else
					{
						ActorCollider actorCollider = an as ActorCollider;
						if(actorCollider != null && InstanceColliders)
						{
							go = new GameObject(an.Name, typeof(ActorColliderComponent));
							ActorColliderComponent colliderComponent = go.GetComponent<ActorColliderComponent>();
							colliderComponent.Node = actorCollider;
							m_Nodes[i] = colliderComponent;
						}
						// else
						// {
						// 	go = new GameObject(an.Name, typeof(ActorNodeComponent));

						// 	ActorNodeComponent nodeComponent = go.GetComponent<ActorNodeComponent>();
						// 	nodeComponent.Node = an;
						// 	m_Nodes[i] = nodeComponent;
						// }
					}
					
					if(go != null)
					{
						go.hideFlags = hideFlags;
					}
				}
				// After they are all created, initialize them.
				for(int i = 0; i < m_Nodes.Length; i++)
				{
					ActorNodeComponent nodeComponent = m_Nodes[i];
					if(nodeComponent != null)
					{
						nodeComponent.Initialize(this);
					}
				}
			}

			OnActorInitialized();
			
#if UNITY_EDITOR
			LateUpdate();
			UpdateEditorBounds();
#endif
		}

		public ActorAsset Asset
		{
			get
			{
				return m_ActorAsset;
			}
			set
			{
				m_ActorAsset = value;
#if UNITY_EDITOR
				Reload();
#endif
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
			if(m_AnimationControllers != null)
			{
				foreach(IActorAnimationController animationController in m_AnimationControllers)
				{
					if(animationController != null)
					{
						animationController.UpdateAnimations(Time.deltaTime);
					}
				}
			}

			if(m_ActorInstance != null)
			{
				m_ActorInstance.Advance(Time.deltaTime);
			}

			if(m_ManipulationControllers != null)
			{
				foreach(IActorManipulationController manipulationController in m_ManipulationControllers)
				{
					if(manipulationController != null)
					{
						manipulationController.ManipulateActor(this);
					}
				}
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

			if(m_FinalFormDependents != null)
			{
				foreach(IActorFinalFormDependent dependent in m_FinalFormDependents)
				{
					if(dependent != null)
					{
						dependent.OnFinalForm(this);
					}	
				}
			}
#if UNITY_EDITOR
			UpdateEditorBounds();
#endif
		}
	}
}