using System;
using System.IO;
using UnityEngine;
using Nima.Math2D;

namespace Nima.Unity
{
	public class PlayActorAnimation : MonoBehaviour
	{
		[SerializeField]
		private string m_AnimationName;
		[SerializeField]
		private bool m_Loop;

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

		public void Start()
		{
			ActorComponent actor = gameObject.GetComponent<ActorComponent>();
			if(actor != null)
			{
				m_ActorInstance = actor.ActorInstance;
				if(m_ActorInstance != null)
				{
					m_Animation = m_ActorInstance.GetAnimation(m_AnimationName);
				}
			}
			m_AnimationTime = 0.0f;
		}

		public void Update()
		{
			m_AnimationTime += Time.deltaTime;
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