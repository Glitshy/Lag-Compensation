using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PG.LagCompensation
{
    public class HitColliderCapsule : HitColliderGeneric
    {
        enum Direction { x = 0, y = 1, z = 2 }

        public float height = 2f;
        public float radius = 0.5f;

        [SerializeField]
        [Tooltip("Direction of capsule")]
        private Direction direction;

        


        public override float GetBoundingSphereRadius => height * 0.5f;
        public override float GetBoundingSphereRadiusSquared => height * height * 0.25f;

        public override void TryGetParametersFromPhysicsCollider()
        {
            CapsuleCollider col = transform.GetComponent<CapsuleCollider>();
            if (col)
            {
                center = col.center;
                height = col.height;
                radius = col.radius;
                direction = (Direction)col.direction;
            }
        }

        #region Raycasting

        public override bool ColliderCast(Vector3 rayOrigin, Vector3 rayDirection, float range, out ColliderCastHit hit)
        {
            if (ParametricRaycastCapsule(transform.position, transform.rotation, center, height, radius, direction, rayOrigin, rayDirection, out hit))
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
            if (ParametricRaycastCapsule(cachedPosRot.position, cachedPosRot.rotation, center, height, radius, direction, rayOrigin, rayDirection, out hit))
            {
                return hit.entryDistance <= range && hit.entryDistance >= 0f;
            }
            else
            {
                return false;
            }
        }


        private static Vector3 GetCapsuleDirection(Direction _dir, Quaternion _rotation)
        {
            switch (_dir)
            {
                case Direction.x: // x
                    return _rotation * Vector3.right;
                case Direction.y: // y
                    return _rotation * Vector3.up;
                case Direction.z: // z
                    return _rotation * Vector3.forward;
            }

            return Vector3.up;
        }

        private Vector3 GetCapsuleDirectionTransform()
        {
            switch (direction)
            {
                case Direction.x: // x
                    return transform.right;
                case Direction.y: // y
                    return transform.up;
                case Direction.z: // z
                    return transform.forward;
            }

            return Vector3.up;
        }


        public static Quaternion GetCapsuleRotationWithGivenDirection(Quaternion _rotation, int _direction)
        {
            switch (_direction)
            {
                case 0: // x
                    return _rotation * Quaternion.Euler(0f, 90f, 0f);
                //return Quaternion.LookRotation(transform.right, transform.up);
                case 1: // y
                    return _rotation * Quaternion.Euler(90f, 0f, 0f);
                //return Quaternion.LookRotation(transform.up, transform.right);
                case 2: // z
                    return _rotation;
            }

            return Quaternion.identity;
        }

        private static bool ParametricRaycastCapsule(Vector3 _position, Quaternion _rotation, Vector3 _center, float _height, float _radius, Direction _dir, Vector3 o, Vector3 d, out ColliderCastHit _hit)
        {
            // using https://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html

            Vector3 _centerPosition = _position + _rotation * _center;
            Vector3 _capsuleDirection = GetCapsuleDirection(_dir, _rotation);



            _hit = new ColliderCastHit();

            //d = d.normalized; // normalize direction vector

            Vector3 a = _centerPosition - _capsuleDirection * (_height * 0.5f - _radius);
            Vector3 b = _centerPosition + _capsuleDirection * (_height * 0.5f - _radius);

            Vector3 A = b - a;
            Vector3 k = a - o;

            /*
            float sol_a = (A.y * A.y * (d.z * d.z)) + (-2 * A.y * A.z * (d.z * d.y)) + (A.z * A.z * (d.y * d.y));          // quadratic
            float sol_b = (A.y * A.y * (-2 * k.z * d.z)) + (-2 * A.y * A.z * (-k.z * d.y - d.z * k.y)) + (A.z * A.z * (-2 * k.y * d.y));     // linear
            float sol_c = (A.y * A.y * (k.z * k.z)) + (-2 * A.y * A.z * (k.z * k.y)) + (A.z * A.z * (k.y * k.y));          // constant

            sol_a += (A.z * A.z * (d.x * d.x)) + (-2 * A.z * A.x * (d.x * d.z)) + (A.x * A.x * (d.z * d.z));          // quadratic
            sol_b += (A.z * A.z * (-2 * k.x * d.x)) + (-2 * A.z * A.x * (-k.x * d.z - d.x * k.z)) + (A.x * A.x * (-2 * k.z * d.z));     // linear
            sol_c += (A.z * A.z * (k.x * k.x)) + (-2 * A.z * A.x * (k.x * k.z)) + (A.x * A.x * (k.z * k.z));          // constant  ( z --> x, y --> z) relative to first block

            sol_a += (A.x * A.x * (d.y * d.y)) + (-2 * A.x * A.y * (d.y * d.x)) + (A.y * A.y * (d.x * d.x));          // quadratic
            sol_b += (A.x * A.x * (-2 * k.y * d.y)) + (-2 * A.x * A.y * (-k.y * d.x - d.y * k.x)) + (A.y * A.y * (-2 * k.x * d.x));     // linear
            sol_c += (A.x * A.x * (k.y * k.y)) + (-2 * A.x * A.y * (k.y * k.x)) + (A.y * A.y * (k.x * k.x));          // constant ( y --> x, z --> y) relative to first block

            float _inverseDenominator = 1f / (A.x * A.x + A.y * A.y + A.z * A.z);

            sol_a *= _inverseDenominator;
            sol_b *= _inverseDenominator;
            sol_c *= _inverseDenominator;


            sol_c -= _radius * _radius;
            */



            float _inverseDenominator = 1f / (A.x * A.x + A.y * A.y + A.z * A.z);

            float sol_a = (A.y * A.y * d.z * d.z - 2 * A.y * A.z * d.z * d.y + A.z * A.z * d.y * d.y
                        + A.z * A.z * d.x * d.x - 2 * A.z * A.x * d.x * d.z + A.x * A.x * d.z * d.z
                        + A.x * A.x * d.y * d.y - 2 * A.x * A.y * d.y * d.x + A.y * A.y * d.x * d.x) * _inverseDenominator;

            float sol_b = (A.y * A.y * (-2 * k.z * d.z) - 2 * A.y * A.z * (-k.z * d.y - d.z * k.y) + A.z * A.z * (-2 * k.y * d.y)
                        + A.z * A.z * (-2 * k.x * d.x) - 2 * A.z * A.x * (-k.x * d.z - d.x * k.z) + A.x * A.x * (-2 * k.z * d.z)
                        + A.x * A.x * (-2 * k.y * d.y) - 2 * A.x * A.y * (-k.y * d.x - d.y * k.x) + A.y * A.y * (-2 * k.x * d.x)) * _inverseDenominator;

            float sol_c = (A.y * A.y * k.z * k.z - 2 * A.y * A.z * k.z * k.y + A.z * A.z * k.y * k.y
                        + A.z * A.z * k.x * k.x - 2 * A.z * A.x * k.x * k.z + A.x * A.x * k.z * k.z
                        + A.x * A.x * k.y * k.y - 2 * A.x * A.y * k.y * k.x + A.y * A.y * k.x * k.x) * _inverseDenominator
                        - _radius * _radius;



            _hit.exitDistance = quadForm(sol_a, sol_b, sol_c, true);
            _hit.entryDistance = quadForm(sol_a, sol_b, sol_c, false);


            if (!float.IsNaN(_hit.entryDistance)) // we have an entry hit
            {
                _hit.entryPoint = o + _hit.entryDistance * d; // entry point assuming we are in the cylinder region. Will be overriden if we are in spere "a" or sphere "b"
                _hit.exitPoint = o + _hit.exitDistance * d; // exit point assuming we are in the cylinder region. Will be overriden if we are in spere "a" or sphere "b"


                float t_entry = GetTValueAlongLine(a, b, _hit.entryPoint);
                float t_exit = GetTValueAlongLine(a, b, _hit.exitPoint);

                if (t_entry > 1f) // look at upper sphere (centered at "b")
                {
                    if (t_exit > 1f) // look at upper sphere (centered at "b")
                    {
                        ParametricRaycastSphereBothSided(b, _radius, o, d, out _hit.entryPoint, out _hit.entryNormal, out _hit.entryDistance, out _hit.exitPoint, out _hit.exitNormal, out _hit.exitDistance);
                    }
                    else
                    {
                        if (ParametricRaycastSphereEntry(b, _radius, o, d, out _hit.entryPoint, out _hit.entryNormal, out _hit.entryDistance))
                        {
                            if (t_exit > 0f) // we are in the cylinder range of the capsule
                            {
                                Vector3 _closestPoint = a + t_exit * (b - a);
                                _hit.exitNormal = (_hit.exitPoint - _closestPoint).normalized;
                            }
                            else // look at lower sphere (centered at "a")
                            {
                                ParametricRaycastSphereExit(a, _radius, o, d, out _hit.exitPoint, out _hit.exitNormal, out _hit.exitDistance);
                            }
                        }
                    }
                }
                else if (t_entry < 0f) // look at lower sphere (centered at "a")
                {
                    if (t_exit < 0f) // look at lower sphere (centered at "a")
                    {
                        ParametricRaycastSphereBothSided(a, _radius, o, d, out _hit.entryPoint, out _hit.entryNormal, out _hit.entryDistance, out _hit.exitPoint, out _hit.exitNormal, out _hit.exitDistance);
                    }
                    else
                    {
                        if (ParametricRaycastSphereEntry(a, _radius, o, d, out _hit.entryPoint, out _hit.entryNormal, out _hit.entryDistance))
                        {
                            if (t_exit < 1f) // we are in the cylinder range of the capsule
                            {
                                Vector3 _closestPoint = a + t_exit * (b - a);
                                _hit.exitNormal = (_hit.exitPoint - _closestPoint).normalized;
                            }
                            else // look at upper sphere (centered at "b")
                            {
                                ParametricRaycastSphereExit(b, _radius, o, d, out _hit.exitPoint, out _hit.exitNormal, out _hit.exitDistance);
                            }
                        }
                    }
                }
                else // we are in the cylinder range of the capsule
                {
                    Vector3 _closestPoint = a + t_entry * (b - a);

                    _hit.entryNormal = (_hit.entryPoint - _closestPoint).normalized;
                    //Debug.DrawLine(_closestPoint, _hit.entryPoint, Color.blue);


                    if (t_exit > 1f) // look at upper sphere (centered at "b")
                    {
                        ParametricRaycastSphereExit(b, _radius, o, d, out _hit.exitPoint, out _hit.exitNormal, out _hit.exitDistance);
                    }
                    else if (t_exit < 0f) // look at lower sphere (centered at "a")
                    {
                        ParametricRaycastSphereExit(a, _radius, o, d, out _hit.exitPoint, out _hit.exitNormal, out _hit.exitDistance);
                    }
                    else // we are in the cylinder range of the capsule
                    {
                        _closestPoint = a + t_exit * (b - a);
                        _hit.exitNormal = (_hit.exitPoint - _closestPoint).normalized;
                    }
                }


                if (float.IsNaN(_hit.entryDistance)) // we did not hit after all
                {
                    return false;
                }
                else // we did hit
                {
                    return true;
                }

            }
            else // we did not hit
            {
                return false;
            }



        }



        #endregion



        #region Interpolation



        #endregion




        #region Debug Draw

        public override void DebugDraw(Vector3 position, Quaternion rotation, float _duration, Color _col)
        {
            DebugDrawCapsule(position + rotation * center, rotation, (int)direction, height, radius, _duration, _col);

        }

        public static void DebugDrawCapsule(Vector3 _centerGlobal, Quaternion _rotation, int _direction, float _height, float _radius, float _duration, Color _color, bool gizmo = false)
        {
            Vector3 centerInGlobalSpace = _centerGlobal;

            Quaternion _adjustedRotation = GetCapsuleRotationWithGivenDirection(_rotation, _direction);


            Vector3 _planeDirection1 = _adjustedRotation * Vector3.right;
            Vector3 _planeDirection2 = _adjustedRotation * Vector3.up;
            Vector3 _capusleDirection = _adjustedRotation * Vector3.forward;
            //Vector3 _capusleDirection = _trans.rotation * Vector3.up;

            Vector3 _topCircleCenter = centerInGlobalSpace + _capusleDirection * (_height * 0.5f - _radius);
            Vector3 _bottomCircleCenter = centerInGlobalSpace - _capusleDirection * (_height * 0.5f - _radius);

            DrawLine(_topCircleCenter + _planeDirection1 * _radius, _bottomCircleCenter + _planeDirection1 * _radius, _color, _duration, gizmo);
            DrawLine(_topCircleCenter - _planeDirection1 * _radius, _bottomCircleCenter - _planeDirection1 * _radius, _color, _duration, gizmo);
            DrawLine(_topCircleCenter + _planeDirection2 * _radius, _bottomCircleCenter + _planeDirection2 * _radius, _color, _duration, gizmo);
            DrawLine(_topCircleCenter - _planeDirection2 * _radius, _bottomCircleCenter - _planeDirection2 * _radius, _color, _duration, gizmo);

            DebugDrawCircle(_topCircleCenter, _planeDirection1, _planeDirection2, _radius, _duration, _color, gizmo);
            DebugDrawCircle(_bottomCircleCenter, _planeDirection1, _planeDirection2, _radius, _duration, _color, gizmo);

            DebugDrawHalfCircle(_topCircleCenter, _planeDirection2, _capusleDirection, _radius, _duration, _color, gizmo);
            DebugDrawHalfCircle(_topCircleCenter, _planeDirection1, _capusleDirection, _radius, _duration, _color, gizmo);
            DebugDrawHalfCircle(_bottomCircleCenter, _planeDirection2, -_capusleDirection, _radius, _duration, _color, gizmo);
            DebugDrawHalfCircle(_bottomCircleCenter, _planeDirection1, -_capusleDirection, _radius, _duration, _color, gizmo);
        }

        private void OnDrawGizmosSelected()
        {


            Gizmos.color = Color.blue;

            DebugDrawCapsule(transform.TransformPoint(center), transform.rotation, (int)direction, height, radius, 1f, Color.white, true);


            // debug show bounding sphere
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(transform.TransformPoint(center), Mathf.Sqrt(height * height * 0.25f));
        }


        #endregion

    }

}