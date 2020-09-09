using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drawing {
	/// <summary>
	/// Inherit from this class to draw gizmos.
	/// See: getstarted (view in online documentation for working links)
	/// </summary>
	public abstract class MonoBehaviourGizmos : MonoBehaviour, IDrawGizmos {
		public MonoBehaviourGizmos() {
			DrawingManager.Register(this);
		}

		// Why an empty OnDrawGizmos method?
		// This is because only objects with an OnDrawGizmos method will show up in Unity's menu for enabling/disabling
		// the gizmos per object type (upper right corner of the scene view). So we need it here even though
		// we don't use normal gizmos.
		void OnDrawGizmos () {
		}

		public virtual void DrawGizmos () {
		}
	}
}
