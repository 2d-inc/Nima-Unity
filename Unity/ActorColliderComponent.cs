using System;
using System.IO;
using UnityEngine;
using Nima.Math2D;

namespace Nima.Unity
{
	[ExecuteInEditMode]
	public class ActorColliderComponent : ActorNodeComponent
	{
		protected Collider2D m_Collider;
		protected ActorCollider m_ActorCollider;

		public override void Initialize(ActorBaseComponent actorComponent)
		{
			base.Initialize(actorComponent);
			m_ActorCollider = m_ActorNode as ActorCollider;

			if(m_ActorCollider is ActorColliderCircle)
			{
				gameObject.AddComponent(typeof(CircleCollider2D));
				CircleCollider2D circleCollider = gameObject.GetComponent<CircleCollider2D>();

				ActorColliderCircle actorCircleCollider = m_ActorCollider as ActorColliderCircle;
				circleCollider.radius = actorCircleCollider.Radius;

				m_Collider = circleCollider;
			}
			else if(m_ActorCollider is ActorColliderRectangle)
			{
				gameObject.AddComponent(typeof(BoxCollider2D));
				BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();

				ActorColliderRectangle actorRectangleCollider = m_ActorCollider as ActorColliderRectangle;
				boxCollider.size = new Vector2(actorRectangleCollider.Width, actorRectangleCollider.Height);

				m_Collider = boxCollider;
			}
			else if(m_ActorCollider is ActorColliderPolygon)
			{
				gameObject.AddComponent(typeof(PolygonCollider2D));
				PolygonCollider2D polygonCollider = gameObject.GetComponent<PolygonCollider2D>();

				ActorColliderPolygon actorPolygonCollider = m_ActorCollider as ActorColliderPolygon;
				polygonCollider.pathCount = 1;
				float[] contourBuffer = actorPolygonCollider.ContourVertices;
				Vector2[] points = new Vector2[contourBuffer.Length/2];
				int readIdx = 0;
				for(int i = 0; i < points.Length; i++)
				{
					points[i] = new Vector2(contourBuffer[readIdx], contourBuffer[readIdx+1]);
					readIdx += 2;
				}
				polygonCollider.SetPath(0, points);

				m_Collider = polygonCollider;
			}
			else if(m_ActorCollider is ActorColliderTriangle)
			{
				gameObject.AddComponent(typeof(PolygonCollider2D));
				PolygonCollider2D polygonCollider = gameObject.GetComponent<PolygonCollider2D>();

				ActorColliderTriangle actorTriangleCollider = m_ActorCollider as ActorColliderTriangle;
				polygonCollider.pathCount = 1;

				float hwidth = actorTriangleCollider.Width/2.0f;
				float hheight = actorTriangleCollider.Height/2.0f;
				Vector2[] points = new Vector2[3];
				points[0] = new Vector2(-hwidth, -hheight);
				points[1] = new Vector2(0.0f, hheight);
				points[2] = new Vector2(hwidth, -hheight);
				polygonCollider.SetPath(0, points);

				m_Collider = polygonCollider;
			}
		}

		public override void UpdateTransform()
		{
			if(m_ActorCollider == null)
			{
				return;
			}
			base.UpdateTransform();

			m_Collider.enabled = m_ActorCollider.IsCollisionEnabled;
			
			Mat2D world = m_ActorNode.WorldTransform;
			Vec2D scale = new Vec2D();
			float angle = Mat2D.Decompose(world, scale);
			transform.localEulerAngles = new Vector3(0.0f, 0.0f, angle * Mathf.Rad2Deg);
			transform.localScale = new Vector3(scale[0] * ActorAsset.NimaToUnityScale, scale[1] * ActorAsset.NimaToUnityScale, 1.0f);
			transform.localPosition = new Vector3(world[4] * ActorAsset.NimaToUnityScale, world[5] * ActorAsset.NimaToUnityScale, 0.0f);
		}
	}
}