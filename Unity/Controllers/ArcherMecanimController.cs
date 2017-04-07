using System;
using System.IO;
using UnityEngine;
using Nima.Math2D;

namespace Nima.Unity
{
	public class ArcherMecanimController : MonoBehaviour, IActorAnimationController
	{
		private Animator m_Animator;
		private float m_HorizontalVelocity;
		private bool m_IsRunning;
		private ActorComponent m_Actor;
		private Nima.Animation.ActorAnimation m_Aim;
		private float m_AimAnimationTime = 0.0f;
		private Vector3 m_ActorLocalCursor;

		private struct AimSlice
		{
			public Vec2D point;
			public Vec2D dir;
		}
		const int AimSliceCount = 60;
		const float MixSpeed = 3.5f;

		AimSlice[] m_AimLookup = new AimSlice[AimSliceCount];
		AimSlice[] m_AimWalkingLookup = new AimSlice[AimSliceCount];

		void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Actor = gameObject.GetComponent<ActorComponent>();
			if(m_Actor != null)
			{
				if(m_Actor.ActorInstance != null)
				{
					m_Aim = m_Actor.ActorInstance.GetAnimation("Aim2");
					Nima.Animation.ActorAnimation walk = m_Actor.ActorInstance.GetAnimation("Walk");
					Nima.Animation.ActorAnimation walkToIdle = m_Actor.ActorInstance.GetAnimation("WalkToIdle");


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
						if(walk != null)
						{
							walk.Apply(0.0f, m_Actor.ActorInstance, 1.0f);

							for(int i = 0; i < AimSliceCount; i++)
							{
								float position = i / (float)(AimSliceCount-1) * m_Aim.Duration;
								m_Aim.Apply(position, m_Actor.ActorInstance, 1.0f);
								Mat2D worldTransform = muzzle.WorldTransform;

								AimSlice slice = m_AimWalkingLookup[i];

								// Extract forward vector and position.
								slice.dir = new Vec2D();
								Vec2D.Normalize(slice.dir, new Vec2D(worldTransform[0], worldTransform[1]));
								slice.point = new Vec2D(worldTransform[4], worldTransform[5]);
								m_AimWalkingLookup[i] = slice;
							}
						}

						if(walkToIdle != null)
						{
							walkToIdle.Apply(walkToIdle.Duration, m_Actor.ActorInstance, 1.0f);
						}
					}
				}
			}
		}


	//	float idletime = 0;
		void Update()
		{
			// Update input.
			if( m_HorizontalVelocity != -1.0f && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ))
			{
				m_HorizontalVelocity = -1.0f;
			}
			else if( m_HorizontalVelocity == -1.0f && (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow) ))
			{
				m_HorizontalVelocity = 0.0f;
			}
			if( m_HorizontalVelocity != 1.0f && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ))
			{
				m_HorizontalVelocity = 1.0f;
			}
			else if( m_HorizontalVelocity == 1.0f && (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow) ))
			{
				m_HorizontalVelocity = 0.0f;
			}
			if( !m_IsRunning && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) ))
			{
				m_IsRunning = true;
			}
			else if( m_IsRunning && (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) ))
			{
				m_IsRunning = false;
			}

			// Find cursor position in world space.
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			m_ActorLocalCursor = m_Actor.gameObject.transform.InverseTransformPoint(ray.origin);
			m_Animator.SetFloat("HorizontalDirection", m_ActorLocalCursor[0] > 0.0f ? 1.0f : -1.0f);
			if(m_ActorLocalCursor[0] < 0.0f)
			{
				m_ActorLocalCursor[0] *= -1.0f;
			}
			m_Animator.SetFloat("HorizontalSpeed", Math.Abs(m_HorizontalVelocity));
			m_Animator.SetBool("IsRunning", m_IsRunning);
		}

		public void UpdateAnimations(float elapsedSeconds)
		{			
			// Apply aim animation.
			if(!isActiveAndEnabled || m_Actor == null)
			{
				return;
			}
			Actor actorInstance = m_Actor.ActorInstance;
			if(m_Aim != null)
			{
				// Figure out best aim position.
				Vec2D actorTarget = new Vec2D(m_ActorLocalCursor[0], m_ActorLocalCursor[1]);

				// Now actorTarget is in Nima root space.
				float maxDot = -1.0f;
				int bestIndex = 0;
				AimSlice[] lookup = Math.Abs(m_HorizontalVelocity) > 0.0f ? m_AimWalkingLookup : m_AimLookup;
				
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

				/*Nima.Animation.ActorAnimation idle = m_Actor.ActorInstance.GetAnimation("Idle");
				idletime += elapsedSeconds;
				idletime %= idle.Duration;
				idle.Apply(idletime, m_Actor.ActorInstance, 1.0f);*/
				m_AimAnimationTime += (targetAimTime-m_AimAnimationTime) * Math.Min(1.0f, elapsedSeconds*10.0f);
				m_Aim.Apply(m_AimAnimationTime, m_Actor.ActorInstance, 1.0f);
			}
		}
	}
}