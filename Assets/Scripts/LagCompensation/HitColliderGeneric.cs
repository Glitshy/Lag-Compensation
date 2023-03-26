using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using Mirror;


namespace PG.LagCompensation
{

    /// <summary>
    /// Abstract parent class for colliders of parametric raycasts (no physics)
    /// </summary>
    public abstract class HitColliderGeneric : HitCollider
    {

        /// <summary>
	    /// Cast at current transform. Return true if hit. Outpout hit entry and exit point, normal and distance.
	    /// </summary>
	    /// <param name="rayOrigin"></param>
	    /// <param name="rayDirection"></param>
	    /// <param name="range"></param>
	    /// <param name="hit"></param>
	    /// <returns></returns>
	    public virtual bool ColliderCast(Vector3 rayOrigin, Vector3 rayDirection, float range, out ColliderCastHit hit)
        {
            hit = ColliderCastHit.Zero;

            return false;
        }

        /// <summary>
	    /// Cast at cached location/rotation. Return true if hit. Outpout hit entry and exit point, normal and distance.
	    /// </summary>
	    /// <param name="rayOrigin"></param>
	    /// <param name="rayDirection"></param>
	    /// <param name="range"></param>
	    /// <param name="hit"></param>
	    /// <returns></returns>
	    public virtual bool ColliderCastCached(Vector3 rayOrigin, Vector3 rayDirection, float range, out ColliderCastHit hit)
        {
            hit = ColliderCastHit.Zero;

            return false;
        }


        #region Tools


        [ContextMenu("Try get parameters from collider")]
        private void TryGet()
        {
            TryGetParametersFromPhysicsCollider();

        }


        public virtual void TryGetParametersFromPhysicsCollider()
        {

        }


        #endregion


    }

}