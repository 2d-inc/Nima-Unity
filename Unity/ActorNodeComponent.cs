using System;
using System.IO;
using UnityEngine;
using Nima.Math2D;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class ActorNodeComponent : MonoBehaviour
	{
		protected ActorBaseComponent m_ActorComponent;
		protected Nima.ActorNode m_ActorNode;

		public Nima.ActorNode Node
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

		public virtual void Initialize(ActorBaseComponent actorComponent)
		{
			m_ActorComponent = actorComponent;
			gameObject.transform.parent = actorComponent.gameObject.transform;	
			/*
			ActorNodeComponent parentComponent = actorComponent.Nodes[m_ActorNode.ParentIdx];
			if(parentComponent == this)
			{
				// This is the root.
				// If the parent is self, we've reached root.
				gameObject.transform.parent = actorComponent.gameObject.transform;	
			}
			else
			{
				gameObject.transform.parent = parentComponent.gameObject.transform;
			}
			//UpdateTransform();
			 */
		}

		public virtual void UpdateTransform()
		{
			// if(m_ActorNode == null)
			// {
			// 	return;
			// }
			// //m_ActorNode.UpdateTransforms();
			// transform.localEulerAngles = new Vector3(0.0f, 0.0f, m_ActorNode.Rotation * Mathf.Rad2Deg);
			// transform.localPosition = new Vector3(m_ActorNode.X, m_ActorNode.Y, 0.0f);
			// transform.localScale = new Vector3(m_ActorNode.ScaleX, m_ActorNode.ScaleY, 1.0f);
		}
	}
}