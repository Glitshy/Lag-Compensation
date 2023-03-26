using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PG.LagCompensation
{

    public class HitColliderSphere : HitColliderGeneric
    {
        public float radius = 1f;


        public override float GetBoundingSphereRadius => radius;
        public override float GetBoundingSphereRadiusSquared => radius * radius;


        public override void TryGetParametersFromPhysicsCollider()
        {
            SphereCollider col = transform.GetComponent<SphereCollider>();
            if (col)
            {
                center = col.center;
                radius = col.radius;
            }
        }


        #region Raycasting

        public override bool ColliderCast(Vector3 rayOrigin, Vector3 rayDirection, float range, out ColliderCastHit hit)
        {
            if (ParametricRaycastSphereBothSided(transform.TransformPoint(center), radius, rayOrigin, rayDirection, out hit.entryPoint, out hit.entryNormal, out hit.entryDistance, out hit.exitPoint, out hit.exitNormal, out hit.exitDistance))
            {
                return hit.entryDistance <= range && hit.entryDistance >= 0f;
            }
            else
            {
                //hit = new ColliderCastHit();
                return false;
            }
        }


        public override bool ColliderCastCached(Vector3 rayOrigin, Vector3 rayDirection, float range, out ColliderCastHit hit)
        {
            if (ParametricRaycastSphereBothSided(cachedPosRot.position, radius, rayOrigin, rayDirection, out hit.entryPoint, out hit.entryNormal, out hit.entryDistance, out hit.exitPoint, out hit.exitNormal, out hit.exitDistance))
            {
                return hit.entryDistance <= range && hit.entryDistance >= 0f;
            }
            else
            {
                hit = new ColliderCastHit();
                return false;
            }
        }

        #endregion

        #region Interpolation


        #endregion


        #region Debug Draw

        public override void DebugDraw(Vector3 position, Quaternion rotation, float _duration, Color _col)
        {
            DebugDrawSphere(position + rotation * center, rotation, radius, _duration, _col);

        }


        private void OnDrawGizmosSelected()
        {


            Gizmos.color = Color.blue;

            DebugDrawSphere(transform.TransformPoint(center), transform.rotation, radius, 1f, Color.white, true);
        }

        #endregion
    }


}