using System;
using System.IO;
using UnityEngine;
using Nima.Math2D;

namespace Nima.Unity
{
	public class PlayActorAnimation : MonoBehaviour, IActorAnimationController
	{
		[SerializeField]
		private string m_AnimationName;
		[SerializeField]
		private bool m_Loop;
		[SerializeField]
		private float m_Offset = 0.0f;
		[SerializeField]
		private float m_Speed = 1.0f;

		Nima.Animation.ActorAnimation m_Animation;
		float m_AnimationTime;
		Actor m_ActorInstance;

		public string AnimationName
		{
			get
			{
				return m_AnimationName;
			}
			set
			{
				m_AnimationName = value;
			}
		}

		public bool Loop
		{
			get
			{
				return m_Loop;
			}
			set
			{
				m_Loop = value;
			}
		}

		public float Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				m_Offset = value;
			}
		}

		public float Speed
		{
			get
			{
				return m_Speed;
			}
			set
			{
				m_Speed = value;
			}
		}

		public void Start()
		{
			ActorBaseComponent actor = gameObject.GetComponent<ActorBaseComponent>();
			if(actor != null)
			{
				m_ActorInstance = actor.ActorInstance;
				if(m_ActorInstance != null)
				{
					m_Animation = m_ActorInstance.GetAnimation(m_AnimationName);
				}
			}
			m_AnimationTime = m_Offset*m_Animation.Duration;
		}

		public void UpdateAnimations(float elapsedSeconds)
		{
			if(m_Animation == null)
			{
				return;
			}
			m_AnimationTime += elapsedSeconds*m_Speed;
			if(m_Loop)
			{
				m_AnimationTime %= m_Animation.Duration;
			}
			if(m_Animation != null)
			{
				m_Animation.Apply(m_AnimationTime, m_ActorInstance, 1.0f);
			}
		}
	}
}