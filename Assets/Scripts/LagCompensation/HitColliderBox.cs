using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PG.LagCompensation
{
    public class HitColliderBox : HitColliderGeneric
    {
        public Vector3 size = Vector3.one;


        /// <summary>
        /// Draw lines when raycasting at this collider
        /// </summary>
        private static bool drawDebugLines = false;



        public override float GetBoundingSphereRadius => (size * 0.5f).magnitude;
        public override float GetBoundingSphereRadiusSquared => (size * 0.5f).sqrMagnitude;


        public override void TryGetParametersFromPhysicsCollider()
        {
            BoxCollider col = transform.GetComponent<BoxCollider>();
            if (col)
            {
                center = col.center;
                size = col.size;
            }
        }


        #region Raycasting

        public override bool ColliderCast(Vector3 rayOrigin, Vector3 rayDirection, float range, out ColliderCastHit hit)
        {
            if (BoxTest(transform.position, transform.rotation, center, size, new Ray(rayOrigin, rayDirection), out hit))
            {
                return hit.entryDistance <= range && hit.entryDistance >= 0f;
            }
            else
            {
                return false;
            }

        }

        public override bool ColliderCastCached(Vector3 rayOrigin, Vector3 rayDirection, float range, out ColliderCastHit hit)
        {


            if (BoxTest(cachedPosRot.position, cachedPosRot.rotation, center, size, new Ray(rayOrigin, rayDirection), out hit))
            {
                return hit.entryDistance <= range && hit.entryDistance >= 0f;
            }
            else
            {
                return false;
            }

        }




        /// <summary>
        /// Parametric raycast at box given by transform, center and size
        /// </summary>
        /// <returns></returns>
        private static bool BoxTest(Vector3 pos, Quaternion rot, Vector3 boxCenter, Vector3 boxSize, Ray _ray, out ColliderCastHit _hit)
        {
            //Quaternion _inverseRotation = new Quaternion(-_t.rotation.x, -_t.rotation.y, -_t.rotation.z, _t.rotation.w);
            Quaternion _inverseRotation = Quaternion.Inverse(rot);
            Vector3 _origin = _inverseRotation * (_ray.origin - pos);
            Vector3 _direction = _inverseRotation * _ray.direction;

            Ray _transformedRay = new Ray(_origin, _direction);
            Ray _transformedRayOppositeDirection = new Ray(_origin + _direction * 9999f, -_direction);

            Bounds _bound = new Bounds(Vector3.zero + boxCenter, boxSize);

            bool _hitBoolean = _bound.IntersectRay(_transformedRay, out float entryDistance);

            if (_hitBoolean) // --> figure out exit point and normals of entry and exit
            {
                _bound.IntersectRay(_transformedRayOppositeDirection, out float exitDistance);
                exitDistance = 9999f - exitDistance;

                _hit = new ColliderCastHit() { entryPoint = _ray.origin + _ray.direction * entryDistance, entryDistance = entryDistance, exitPoint = _ray.origin + _ray.direction * exitDistance, exitDistance = exitDistance };

                Vector3 _LocalEntryPoint = _transformedRay.origin + _transformedRay.direction * entryDistance - boxCenter;

                Vector3 _deltaLocalEntryPointAbs = new Vector3(0.5f * boxSize.x - Mathf.Abs(_LocalEntryPoint.x), 0.5f * boxSize.y - Mathf.Abs(_LocalEntryPoint.y), 0.5f * boxSize.z - Mathf.Abs(_LocalEntryPoint.z));

                if (_deltaLocalEntryPointAbs.x < _deltaLocalEntryPointAbs.y && _deltaLocalEntryPointAbs.x < _deltaLocalEntryPointAbs.z) // smallest delta is in x direction
                {
                    _hit.entryNormal = rot * Vector3.right * Mathf.Sign(_LocalEntryPoint.x);
                }
                else if (_deltaLocalEntryPointAbs.y < _deltaLocalEntryPointAbs.x && _deltaLocalEntryPointAbs.y < _deltaLocalEntryPointAbs.z) // smallest delta is in y direction
                {
                    _hit.entryNormal = rot * Vector3.up * Mathf.Sign(_LocalEntryPoint.y);
                }
                else // smallest delta is in z direction
                {
                    _hit.entryNormal = rot * Vector3.forward * Mathf.Sign(_LocalEntryPoint.z);
                }

                Vector3 _LocalExitPoint = _transformedRay.origin + _transformedRay.direction * exitDistance - boxCenter;

                Vector3 _deltaLxitEntryPointAbs = new Vector3(0.5f * boxSize.x - Mathf.Abs(_LocalExitPoint.x), 0.5f * boxSize.y - Mathf.Abs(_LocalExitPoint.y), 0.5f * boxSize.z - Mathf.Abs(_LocalExitPoint.z));

                if (_deltaLxitEntryPointAbs.x < _deltaLxitEntryPointAbs.y && _deltaLxitEntryPointAbs.x < _deltaLxitEntryPointAbs.z) // smallest delta is in x direction
                {
                    _hit.exitNormal = rot * Vector3.right * Mathf.Sign(_LocalExitPoint.x);
                }
                else if (_deltaLxitEntryPointAbs.y < _deltaLxitEntryPointAbs.x && _deltaLxitEntryPointAbs.y < _deltaLxitEntryPointAbs.z) // smallest delta is in y direction
                {
                    _hit.exitNormal = rot * Vector3.up * Mathf.Sign(_LocalExitPoint.y);
                }
                else // smallest delta is in z direction
                {
                    _hit.exitNormal = rot * Vector3.forward * Mathf.Sign(_LocalExitPoint.z);
                }

            }
            else
            {
                _hit = new ColliderCastHit();

            }

            if (drawDebugLines)
            {
                DebugDrawBox(pos, rot, boxCenter, boxSize, 10f, Color.green);
                DebugDrawBox(Vector3.zero, Quaternion.identity, boxCenter, boxSize, 10f, Color.blue);

                if (_hitBoolean)
                {
                    Debug.DrawLine(_transformedRay.origin, _transformedRay.origin + _transformedRay.direction * entryDistance, Color.cyan, 10f);
                }
                else
                {
                    Debug.DrawLine(_transformedRay.origin, _transformedRay.origin + _transformedRay.direction * 10f, Color.black, 10f);
                }
            }



            return _hitBoolean;
        }



        #endregion


        #region Interpolation



        #endregion



        #region Debug Draw

        public override void DebugDraw(Vector3 position, Quaternion rotation, float _duration, Color _col)
        {
            DebugDrawBox(position, rotation, size, center, _duration, _col);
        }

        public static void DebugDrawBox(Vector3 _position, Quaternion _rotation, Vector3 _size, Vector3 _center, float _duration, Color _color, bool gizmo = false)
        {
            Vector3 dimensions = _size;

            Vector3 centerInGlobalSpace = _rotation * _center + _position;

            Vector3 _forward = _rotation * Vector3.forward;
            Vector3 _right = _rotation * Vector3.right;
            Vector3 _up = _rotation * Vector3.up;

            Vector3 _RightUpForward = centerInGlobalSpace + _right * (dimensions.x / 2f) + _up * (dimensions.y / 2f) + _forward * (dimensions.z / 2f);
            Vector3 _RightUpBackward = centerInGlobalSpace + _right * (dimensions.x / 2f) + _up * (dimensions.y / 2f) - _forward * (dimensions.z / 2f);
            Vector3 _RightDownForward = centerInGlobalSpace + _right * (dimensions.x / 2f) - _up * (dimensions.y / 2f) + _forward * (dimensions.z / 2f);
            Vector3 _RightDownBackward = centerInGlobalSpace + _right * (dimensions.x / 2f) - _up * (dimensions.y / 2f) - _forward * (dimensions.z / 2f);
            Vector3 _LeftUpForward = centerInGlobalSpace - _right * (dimensions.x / 2f) + _up * (dimensions.y / 2f) + _forward * (dimensions.z / 2f);
            Vector3 _LeftUpBackward = centerInGlobalSpace - _right * (dimensions.x / 2f) + _up * (dimensions.y / 2f) - _forward * (dimensions.z / 2f);
            Vector3 _LeftDownForward = centerInGlobalSpace - _right * (dimensions.x / 2f) - _up * (dimensions.y / 2f) + _forward * (dimensions.z / 2f);
            Vector3 _LeftDownBackward = centerInGlobalSpace - _right * (dimensions.x / 2f) - _up * (dimensions.y / 2f) - _forward * (dimensions.z / 2f);


            // from forward to backward
            DrawLine(_RightUpForward, _RightUpBackward, _color, _duration, gizmo);
            DrawLine(_RightDownForward, _RightDownBackward, _color, _duration, gizmo);
            DrawLine(_LeftUpForward, _LeftUpBackward, _color, _duration, gizmo);
            DrawLine(_LeftDownForward, _LeftDownBackward, _color, _duration, gizmo);

            // front face
            DrawLine(_RightUpForward, _RightDownForward, _color, _duration, gizmo);
            DrawLine(_RightUpForward, _LeftUpForward, _color, _duration, gizmo);
            DrawLine(_LeftDownForward, _RightDownForward, _color, _duration, gizmo);
            DrawLine(_LeftDownForward, _LeftUpForward, _color, _duration, gizmo);

            // back face
            DrawLine(_RightUpBackward, _RightDownBackward, _color, _duration, gizmo);
            DrawLine(_RightUpBackward, _LeftUpBackward, _color, _duration, gizmo);
            DrawLine(_LeftDownBackward, _RightDownBackward, _color, _duration, gizmo);
            DrawLine(_LeftDownBackward, _LeftUpBackward, _color, _duration, gizmo);
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            DebugDrawBox(transform.position, transform.rotation, size, center, 0f, Color.white, true);

            // debug show bounding sphere
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(transform.TransformPoint(center), Mathf.Sqrt((size * 0.5f).sqrMagnitude));

        }

        #endregion
    }



}