using System;
using System.IO;
using UnityEngine;
using Nima.Math2D;

namespace Nima.Unity
{
	public class ArcherController : MonoBehaviour
	{
		private struct AimSlice
		{
			public Vec2D point;
			public Vec2D dir;
		}
		const int AimSliceCount = 60;
		const float MixSpeed = 3.5f;

		AimSlice[] m_AimLookup = new AimSlice[AimSliceCount];
		AimSlice[] m_AimWalkingLookup = new AimSlice[AimSliceCount];

		private ActorComponent m_Actor;
		private Nima.Animation.ActorAnimation m_Idle;
		private Nima.Animation.ActorAnimation m_Aim;
		private float m_IdleTime;
		private float m_AimAnimationTime;

		public void Start()
		{
			m_Actor = gameObject.GetComponent<ActorComponent>();
			if(m_Actor != null)
			{
				if(m_Actor.ActorInstance != null)
				{
					m_Idle = m_Actor.ActorInstance.GetAnimation("Idle");
					m_Aim = m_Actor.ActorInstance.GetAnimation("Aim2");

					// Calculate aim slices.
					if(m_Aim != null)
					{
						ActorNode muzzle = m_Actor.ActorInstance.GetNode("Muzzle");
						if(muzzle != null)
						{
							for(int i = 0; i < AimSliceCount; i++)
							{
								float position = i / (float)(AimSliceCount-1) * m_Aim.Duration;
								m_Aim.Apply(position, m_Actor.ActorInstance, 1.0f);
								Mat2D worldTransform = muzzle.WorldTransform;

								AimSlice slice = m_AimLookup[i];

								// Extract forward vector and position.
								slice.dir = new Vec2D();
								Vec2D.Normalize(slice.dir, new Vec2D(worldTransform[0], worldTransform[1]));
								slice.point = new Vec2D(worldTransform[4], worldTransform[5]);
								m_AimLookup[i] = slice;
							}
						}
					}
				}
			}
			m_IdleTime = 0.0f;
		}

		public void Update()
		{
			if(m_Actor == null)
			{
				return;
			}
			if(m_Idle != null)
			{
				m_IdleTime = (m_IdleTime+Time.deltaTime)%m_Idle.Duration;
				m_Idle.Apply(m_IdleTime, m_Actor.ActorInstance, 1.0f);
			}

			Actor actorInstance = m_Actor.ActorInstance;

			float scaleX = ActorAsset.NimaToUnityScale;
			// Find cursor position in world space.
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 actorLocalCursor = m_Actor.gameObject.transform.InverseTransformPoint(ray.origin);
			if(actorLocalCursor[0] < 0.0f)
			{
				scaleX *= -1.0f;
				actorLocalCursor[0] *= -1.0f;
			}

			actorInstance.Root.ScaleX = scaleX;

			if(m_Aim != null)
			{
				// Figure out best aim position.
				// Target is in actor local space, but now we need to go into nima root space.
				// Unity -> Unity Character Root (actor local) -> Nima Actor Root -> Bones/Images/Etc
				Mat2D inverseToActor = new Mat2D();
				Vec2D actorTarget = new Vec2D(actorLocalCursor[0], actorLocalCursor[1]);/*new Vec2D();
				Mat2D.Invert(inverseToActor, actorInstance.Root.WorldTransform);
				Vec2D.TransformMat2D(actorTarget, new Vec2D(actorLocalCursor[0], actorLocalCursor[1]), inverseToActor);*/

				// Now actorTarget is in Nima root space.
				float maxDot = -1.0f;
				int bestIndex = 0;
				AimSlice[] lookup = m_AimLookup;
				
				Vec2D targetDir = new Vec2D();
				for(int i = 0; i < AimSliceCount; i++)
				{
					AimSlice aim = lookup[i];

					
					Vec2D.Subtract(targetDir, actorTarget, aim.point);
					Vec2D.Normalize(targetDir, targetDir);
					float d = Vec2D.Dot(targetDir, aim.dir);
					if(d > maxDot)
					{
						maxDot = d;
						bestIndex = i;
					}
				}
				float targetAimTime = bestIndex/(float)(AimSliceCount-1) * m_Aim.Duration;

				m_AimAnimationTime += (targetAimTime-m_AimAnimationTime) * Math.Min(1.0f, Time.deltaTime*10.0f);
				m_Aim.Apply(m_AimAnimationTime, m_Actor.ActorInstance, 1.0f);
			}
		}
	}
}