using System;
using System.IO;
using UnityEngine;
using Nima.Math2D;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class MountToActor : MonoBehaviour, IActorFinalFormDependent
	{
		[SerializeField]
		private GameObject m_ActorGameObject;
		[SerializeField]
		private string m_NodeName;
		[SerializeField]
		private bool m_InheritScale = true;
		[SerializeField]
		private bool m_InheritRotation = true;
		[SerializeField]
		private float m_ScaleModifier = 1.0f;

		private ActorBaseComponent m_ActorBase;
		private ActorNode m_MountNode;

		public GameObject ActorGameObject
		{
			get
			{
				return m_ActorGameObject;
			}
			set
			{
				m_ActorGameObject = value;
			}
		}

		public string NodeName
		{
			get
			{
				return m_NodeName;
			}
			set
			{
				m_NodeName = value;
			}
		}

		public bool InheritScale
		{
			get
			{
				return m_InheritScale;
			}
			set
			{
				m_InheritScale = value;
			}
		}

		public bool InheritRotation
		{
			get
			{
				return m_InheritRotation;
			}
			set
			{
				m_InheritRotation = value;
			}
		}

		public float ScaleModifier
		{
			get
			{
				return m_ScaleModifier;
			}
			set
			{
				m_ScaleModifier = value;
			}
		}

		public void Start()
		{
			if(m_ActorGameObject != null)
			{
				m_ActorBase = m_ActorGameObject.GetComponent<ActorBaseComponent>();
				if(m_ActorBase != null)
				{
					m_MountNode = m_ActorBase.GetActorNode(m_NodeName);
					if(m_MountNode != null)
					{
						m_ActorBase.AddFinalFormDependent(this);
					}
					//if(go != null)
					//{
						//gameObject.transform.SetParent(go.transform, false);
					//}
				}
			}
		}

		public void OnDisable()
		{
			if(m_ActorBase != null)
			{
				m_ActorBase.RemoveFinalFormDependent(this);
				m_ActorBase = null;
				m_MountNode = null;
			}
		}

		public void UpdateMount()
		{
			if(m_MountNode == null)
			{
				return;
			}
			//Matrix4x4 world = m_MountTargetObject.transform.localToWorldMatrix;

			Matrix4x4 localParent = Matrix4x4.identity;
			if(gameObject.transform.parent)
			{
				localParent = gameObject.transform.parent.worldToLocalMatrix;
			}
			Matrix4x4 localTransform = localParent * m_ActorGameObject.transform.localToWorldMatrix;
			// m_MountNode

			Mat2D world = m_MountNode.WorldTransform;
			Mat2D m2d = new Mat2D();
			m2d[0] = localTransform[0,0];
			m2d[1] = localTransform[1,0];
			m2d[2] = localTransform[0,1];
			m2d[3] = localTransform[1,1];
			m2d[4] = localTransform[0,3] + world[4] * ActorAsset.NimaToUnityScale * localTransform[0,0];
			m2d[5] = localTransform[1,3] + world[5] * ActorAsset.NimaToUnityScale * localTransform[1,1];


			if(m_InheritRotation && m_InheritScale)
			{
				Vec2D scale = new Vec2D();
				float angle = Mat2D.Decompose(world, scale);
				transform.localEulerAngles = new Vector3(0.0f, 0.0f, angle * Mathf.Rad2Deg);
				transform.localScale = new Vector3(scale[0]*m_ScaleModifier, scale[1]*m_ScaleModifier, 1.0f);
			}
			else if(m_InheritRotation)
			{
				float angle = (float)Math.Atan2(world[1], world[0]);
				transform.localEulerAngles = new Vector3(0.0f, 0.0f, angle * Mathf.Rad2Deg);
			}
			else if(m_InheritScale)
			{
				Vec2D scale = new Vec2D();
				Mat2D.GetScale(world, scale);
				transform.localScale = new Vector3(scale[0]*m_ScaleModifier, scale[1]*m_ScaleModifier, 1.0f);
			}

			transform.localPosition = new Vector3(m2d[4], m2d[5], transform.localPosition.z);
		}
		
		public void OnFinalForm(ActorBaseComponent actorBase)
		{
			UpdateMount();
		}
	}
}