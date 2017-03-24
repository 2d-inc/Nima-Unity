using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Nima.Unity
{
	[SelectionBase]
	public class ActorMecanimComponent : MonoBehaviour
	{
		private Animator m_Animator;
		private ActorBaseComponent m_ActorBaseComponent;
		private Nima.Actor m_Actor;

		private Dictionary<int, Nima.Animation.ActorAnimation> m_AnimationLookup;

		public void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_ActorBaseComponent = GetComponent<ActorBaseComponent>();
			m_AnimationLookup = new Dictionary<int, Nima.Animation.ActorAnimation>();
			if(m_ActorBaseComponent != null)
			{
				m_Actor = m_ActorBaseComponent.ActorInstance;
			}
		}

		Nima.Animation.ActorAnimation ClipToAnimation(AnimationClip clip)
		{
			Nima.Animation.ActorAnimation actorAnimation;
			if(!m_AnimationLookup.TryGetValue(clip.GetInstanceID(), out actorAnimation))
			{
				foreach(Nima.Animation.ActorAnimation anim in m_Actor.Animations)
				{
					if(anim.Name == clip.name)
					{
						actorAnimation = anim;
						m_AnimationLookup.Add(clip.GetInstanceID(), anim);
						break;
					}
				}
			}
			return actorAnimation;
		}

		public void Update()
		{
			if(m_Animator == null || m_Actor == null)
			{
				return;
			}
			int numLayers = m_Animator.layerCount;
			for(int i = 0; i < numLayers; i++)
			{
				float layerMix = i == 0 ? 1.0f : m_Animator.GetLayerWeight(i);
				AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(i);
				AnimatorStateInfo nextStateInfo = m_Animator.GetNextAnimatorStateInfo(i);

				bool hasNext = nextStateInfo.fullPathHash != 0;
				AnimatorClipInfo[] clipInfos = m_Animator.GetCurrentAnimatorClipInfo(i);
				AnimatorClipInfo[] nextClipInfos = m_Animator.GetNextAnimatorClipInfo(i);

				for (int c = 0; c < clipInfos.Length; c++) 
				{
					AnimatorClipInfo clipInfo = clipInfos[c];	
					float mix = clipInfo.weight * layerMix; 
					
					if (mix == 0.0f)
					{
						continue;
					}
					Nima.Animation.ActorAnimation actorAnimation = ClipToAnimation(clipInfo.clip);
					if(actorAnimation != null)
					{
						float time = (stateInfo.normalizedTime * clipInfo.clip.length);
						if(stateInfo.loop)
						{
							time %= actorAnimation.Duration;
						}
						actorAnimation.Apply(time, m_Actor, mix);
					}
				}
				if (hasNext) 
				{
					for (int c = 0; c < nextClipInfos.Length; c++) 
					{
						AnimatorClipInfo clipInfo = nextClipInfos[c]; 
						float mix = clipInfo.weight * layerMix; 
						if(mix == 0.0f)
						{
							continue;
						}
						Nima.Animation.ActorAnimation actorAnimation = ClipToAnimation(clipInfo.clip);
						if(actorAnimation != null)
						{
							float time = (nextStateInfo.normalizedTime * clipInfo.clip.length);
							if(nextStateInfo.loop)
							{
								time %= actorAnimation.Duration;
							}
							actorAnimation.Apply(time, m_Actor, mix);
						}
					}
				}
			}
		}
	}
}