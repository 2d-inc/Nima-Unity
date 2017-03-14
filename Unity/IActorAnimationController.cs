using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nima.Unity
{
	public interface IActorAnimationController
	{
		void UpdateAnimations(float elapsedSeconds);
	}
}