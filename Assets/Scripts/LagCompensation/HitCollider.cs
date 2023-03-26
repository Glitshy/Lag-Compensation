using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using Mirror;


namespace PG.LagCompensation
{

    /// <summary>
    /// Abstract parent class for parametric raycasts (no physics)
    /// </summary>
    public abstract class HitCollider : MonoBehaviour
    {
        /// <summary>
        /// Center of collider in local space relative to transform
        /// </summary>
        public Vector3 center;

        public static int FrameHistory = 40;
        public static float updateIntervalTime = 0.2f;

        private List<TransformFrameData> FrameData = new List<TransformFrameData>();
        public List<double> FrameTimes = new List<double>();


        /// <summary>
        /// Assigned by 'SetStateTransform()'. Use this position/rotation when doing the bounding sphere check and collider cast check. This means we don't need to override the transform position/rotation for lag compensation.
        /// </summary>
        protected TransformFrameData cachedPosRot;

        public TransformFrameData SetCachedPosRot { set => cachedPosRot = value; }

        /// <summary>
        /// Get Radius of bounding sphere
        /// </summary>
        public virtual float GetBoundingSphereRadius => 0f;

        /// <summary>
        /// Get squared Radius of bounding sphere (better performance in some cases)
        /// </summary>
        public virtual float GetBoundingSphereRadiusSquared => 0f;

        // Start is called before the first frame update
        void Start()
        {

        }

	    private void OnDestroy()
	    {
            
        }

	    // Update is called once per frame
	    void Update()
        {
        
        }



	    #region Raycasting

	    

        /// <summary>
        /// Check if a line defined by origin "o" and direction "d" intersects with the bounding sphere of this collider
        /// </summary>
        /// <returns>True --> intersects, False --> No intersection</returns>
        public bool CheckBoundingSphereTransform(Vector3 o, Vector3 d)
        {
            return GetBoundingSphereRadiusSquared >= GetSquaredMinimumDistanceBetwenPointAndLine(transform.TransformPoint(center), o, d);
        }

        /// <summary>
        /// Check if a line defined by origin "o" and direction "d" intersects with the bounding sphere of this collider. Uses "boundingSphereTestRotPos", more performant when overriding transforms
        /// </summary>
        /// <returns>True --> intersects, False --> No intersection</returns>
        public bool CheckBoundingSphereCached(Vector3 o, Vector3 d)
        {
            return GetBoundingSphereRadiusSquared >= GetSquaredMinimumDistanceBetwenPointAndLine(cachedPosRot.position + cachedPosRot.rotation * center, o, d); // cachedPosRot.position + cachedPosRot.rotation * center
        }

        public virtual bool CheckBoundingSphereDistanceTransform(Vector3 o, Vector3 d, float range)
        {
            float closestDistance = GetTValueAlongLine(o, o + d, transform.TransformPoint(center));

            return closestDistance >= 0f && closestDistance <= range + GetBoundingSphereRadius;

        }

        public virtual bool CheckBoundingSphereDistanceCached(Vector3 o, Vector3 d, float range)
	    {
            float closestDistance = GetTValueAlongLine(o, o + d, cachedPosRot.position + cachedPosRot.rotation * center);

            return closestDistance >= 0f && closestDistance <= range + GetBoundingSphereRadius;

        }


        protected static bool ParametricRaycastSphereEntry(Vector3 c, float r, Vector3 o, Vector3 d, out Vector3 entryPoint, out Vector3 entryNormal, out float entryDistance)
        {
            float sol_a = d.x * d.x + d.y * d.y + d.z * d.z;                                                                                                        // quadratic
            float sol_b = (2 * o.x * d.x - 2 * c.x * d.x) + (2 * o.y * d.y - 2 * c.y * d.y) + (2 * o.z * d.z - 2 * c.z * d.z);                                      // linear
            float sol_c = (o.x * o.x + c.x * c.x - 2 * c.x * o.x) + (o.y * o.y + c.y * c.y - 2 * c.y * o.y) + (o.z * o.z + c.z * c.z - 2 * c.z * o.z) - r * r;      // constant

            //float exitDistance = quadForm(sol_a, sol_b, sol_c, true);
            entryDistance = quadForm(sol_a, sol_b, sol_c, false);


            if (!float.IsNaN(entryDistance))
            {
                entryPoint = o + entryDistance * d;
                entryNormal = (entryPoint - c).normalized;

                //Debug.DrawLine(o, entryPoint, Color.green);
                //Debug.Log("exitDistance " + exitDistance + " entryDistance " + entryDistance);

                return true;
            }
            else
            {
                entryPoint = Vector3.zero;
                entryNormal = Vector3.forward;

                //Debug.DrawLine(o, o + d * 10f, Color.red);

                return false;
            }

        }

        protected static bool ParametricRaycastSphereExit(Vector3 c, float r, Vector3 o, Vector3 d, out Vector3 exitPoint, out Vector3 exitNormal, out float exitDistance)
        {
            float sol_a = d.x * d.x + d.y * d.y + d.z * d.z;                                                                                                        // quadratic
            float sol_b = (2 * o.x * d.x - 2 * c.x * d.x) + (2 * o.y * d.y - 2 * c.y * d.y) + (2 * o.z * d.z - 2 * c.z * d.z);                                      // linear
            float sol_c = (o.x * o.x + c.x * c.x - 2 * c.x * o.x) + (o.y * o.y + c.y * c.y - 2 * c.y * o.y) + (o.z * o.z + c.z * c.z - 2 * c.z * o.z) - r * r;      // constant

            exitDistance = quadForm(sol_a, sol_b, sol_c, true);
            //float entryDistance = quadForm(sol_a, sol_b, sol_c, false);


            if (!float.IsNaN(exitDistance))
            {
                exitPoint = o + exitDistance * d;
                exitNormal = (exitPoint - c).normalized;

                //Debug.Log("exitDistance " + exitDistance + " entryDistance " + entryDistance);

                return true;
            }
            else
            {
                exitPoint = Vector3.zero;
                exitNormal = Vector3.forward;

                return false;
            }

        }

        protected static bool ParametricRaycastSphereBothSided(Vector3 c, float r, Vector3 o, Vector3 d, out Vector3 entryPoint, out Vector3 entryNormal, out float entryDistance, out Vector3 exitPoint, out Vector3 exitNormal, out float exitDistance)
        {
            float sol_a = d.x * d.x + d.y * d.y + d.z * d.z;                                                                                                        // quadratic
            float sol_b = (2 * o.x * d.x - 2 * c.x * d.x) + (2 * o.y * d.y - 2 * c.y * d.y) + (2 * o.z * d.z - 2 * c.z * d.z);                                      // linear
            float sol_c = (o.x * o.x + c.x * c.x - 2 * c.x * o.x) + (o.y * o.y + c.y * c.y - 2 * c.y * o.y) + (o.z * o.z + c.z * c.z - 2 * c.z * o.z) - r * r;      // constant


            // alternative writing of same formula
            //float sol_a = d.sqrMagnitude; // quadratic
            //Vector3 _helper = 2 * Vector3.Scale(o, d) - 2 * Vector3.Scale(c, d);
            //float sol_b = _helper.x + _helper.y + _helper.z; // linear
            //float sol_c = o.sqrMagnitude + c.sqrMagnitude  - 2 * c.x * o.x - 2 * c.y * o.y - 2 * c.z * o.z - r * r;      // constant


            exitDistance = quadForm(sol_a, sol_b, sol_c, true);
            entryDistance = quadForm(sol_a, sol_b, sol_c, false);


            if (!float.IsNaN(entryDistance))
            {
                entryPoint = o + entryDistance * d;
                entryNormal = (entryPoint - c) / r; // divide by radius to normalize

                exitPoint = o + exitDistance * d;
                exitNormal = (exitPoint - c) / r;

                //Debug.DrawLine(o, entryPoint, Color.green);
                //Debug.Log("exitDistance " + exitDistance + " entryDistance " + entryDistance);

                return true;
            }
            else
            {
                entryPoint = Vector3.zero;
                entryNormal = Vector3.forward;

                exitPoint = Vector3.zero;
                exitNormal = Vector3.forward;

                //Debug.DrawLine(o, o + d * 10f, Color.red);

                return false;
            }

        }


        /// <summary>
        /// get t-value along line defined by point "a" and "b" where the point "p" is closest to the line
        /// </summary>
        /// <returns></returns>
        protected static float GetTValueAlongLine(Vector3 a, Vector3 b, Vector3 p)
        {
            return -Vector3.Dot((a - p), (b - a)) / (b - a).sqrMagnitude;
        }

        /// <summary>
        /// Get square of the closest distance between a point "p" and a line defined by the origin "o" and direction "d"
        /// </summary>
        /// <param name="point"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected static float GetSquaredMinimumDistanceBetwenPointAndLine(Vector3 p, Vector3 o, Vector3 d)
	    {
            // https://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html


            return Vector3.Cross(d, o - p).sqrMagnitude / d.sqrMagnitude;
        }

        /// <summary>
        /// Return first or second solution to quadratic formula.
        /// </summary>
        /// <param name="a">Constant</param>
        /// <param name="b">Linear</param>
        /// <param name="c">Quadratic</param>
        /// <param name="pos">Positive or negative solution?</param>
        /// <returns></returns>
        protected static float quadForm(float a, float b, float c, bool pos)
        {
            var preRoot = b * b - 4 * a * c;
            if (preRoot < 0)
            {
                return float.NaN;
            }
            else
            {
                var sgn = pos ? 1.0f : -1.0f;
                return (sgn * Mathf.Sqrt(preRoot) - b) / (2.0f * a);
            }
        }

    

	    #endregion

	    #region Interpolation

	    /// <summary>
	    /// Add postion/rotation with timestamp to list. Call this after doing movement updates!
	    /// </summary>
	    public void AddFrame(double _localTime)
        {
            if (FrameTimes.Count >= FrameHistory) // remove oldest stored frame
            {
                FrameTimes.RemoveAt(0);
                FrameData.RemoveAt(0);
            }

            FrameData.Add(new TransformFrameData(){position = transform.position, rotation = transform.rotation});
            FrameTimes.Add(_localTime);
        }


        /// <summary>
        /// Caches interpolated position and transform at the given time. Call 'ColliderCastAtCachedPositionRotation()' to use this cached pos/rot
        /// </summary>
        /// <param name="simulationTime"></param>
        /// <param name="_storeCurrentTransform">First simulation of the frame should always sore the transform, following simulations in the same frame shouldn't.</param>
        public void CacheInterpolationPositionRotation(double simulationTime)
        {
            for (int i = FrameTimes.Count - 1; i >= 0; i--)
            {
                if (FrameTimes[i] <= simulationTime) // if the data at [i] is older than the desired simulation time
                {
                    if (i < FrameTimes.Count - 1) // if there is a newer frame
                    {
                        double fraction = Math.Clamp((simulationTime - FrameTimes[i]) / (FrameTimes[i + 1] - FrameTimes[i]), 0d, 1d);

                        cachedPosRot = TransformFrameData.Interpolate(FrameData[i], FrameData[i + 1], fraction);
                    }
                    else // there is no newer frame --> interpolate between this 'newest' frame and the current position!
				    {
                        double fraction = Math.Clamp((simulationTime - FrameTimes[i]) / (Time.timeAsDouble - FrameTimes[i]), 0d, 1d);

                        cachedPosRot = TransformFrameData.Interpolate(FrameData[i], new TransformFrameData(transform.position, transform.rotation), fraction); // getting current transform and rotation is more performance intensive than cached frame data
                    }

                    return;
                }
            }

        
        }


        #endregion



        #region Debug Draw


        /// <summary>
        /// Draw collider with Debug.DrawLine, for given duration, at the currently cached position
        /// </summary>
        /// <param name="_duration"></param>
        public void DebugDrawColliderCached(float _duration, Color _col)
        {
            DebugDraw(cachedPosRot.position, cachedPosRot.rotation, _duration, _col);
        }


        public virtual void DebugDraw(Vector3 position, Quaternion rotation, float _duration, Color _col)
        {

        }


        public void DebugDrawBoundingSphere(float _duration)
        {
            DebugDrawSphere(transform.TransformPoint(center), transform.rotation, GetBoundingSphereRadius, _duration, Color.yellow);
        }

        public void DebugDrawBoundingSphereAtTestPosition(float _duration)
        {
            DebugDrawSphere(cachedPosRot.position + cachedPosRot.rotation * center, cachedPosRot.rotation, GetBoundingSphereRadius, _duration, Color.yellow);
        }



        protected static void DebugDrawCircle(Vector3 _center, Vector3 _forward, Vector3 _right, float _radius, float _duration, Color _color, bool gizmo = false)
        {
            int _stepCount = 16;
            float _stepAngleInRadians = 2 * Mathf.PI / _stepCount;

            for (int i = 0; i < _stepCount; i++)
            {
                DrawLine(_center + _forward * Mathf.Cos(_stepAngleInRadians * i) * _radius + _right * Mathf.Sin(_stepAngleInRadians * i) * _radius,
                    _center + _forward * Mathf.Cos(_stepAngleInRadians * (i + 1)) * _radius + _right * Mathf.Sin(_stepAngleInRadians * (i + 1)) * _radius,
                    _color, _duration, gizmo);
            }
        }

        protected static void DebugDrawHalfCircle(Vector3 _center, Vector3 _forward, Vector3 _right, float _radius, float _duration, Color _color, bool gizmo = false)
        {
            int _stepCount = 8;
            float _stepAngleInRadians = Mathf.PI / _stepCount;

            for (int i = 0; i < _stepCount; i++)
            {
                DrawLine(_center + _forward * Mathf.Cos(_stepAngleInRadians * i) * _radius + _right * Mathf.Sin(_stepAngleInRadians * i) * _radius,
                    _center + _forward * Mathf.Cos(_stepAngleInRadians * (i + 1)) * _radius + _right * Mathf.Sin(_stepAngleInRadians * (i + 1)) * _radius,
                    _color, _duration, gizmo);
            }
        }

        protected static void DrawLine(Vector3 _start, Vector3 _stop, Color _color, float _duration, bool _gizmo = false)
        {
            if (_gizmo)
            {
                Gizmos.DrawLine(_start, _stop);
            }
            else
            {
                Debug.DrawLine(_start, _stop, _color, _duration);
            }
        }

        public static void DebugDrawSphere(Vector3 _centerGlobalPosition, Quaternion _rotation, float _radius, float _duration, Color _color, bool gizmo = false)
        {
            Vector3 _forward = _rotation * Vector3.forward;
            Vector3 _right = _rotation * Vector3.right;
            Vector3 _up = _rotation * Vector3.up;


            DebugDrawCircle(_centerGlobalPosition, _forward, _right, _radius, _duration, _color, gizmo);
            DebugDrawCircle(_centerGlobalPosition, _forward, _up, _radius, _duration, _color, gizmo);
            DebugDrawCircle(_centerGlobalPosition, _up, _right, _radius, _duration, _color, gizmo);
        }

        #endregion
    }



    /// <summary>
    /// Contains postion and rotation of collider
    /// </summary>
    public struct TransformFrameData
    {
        public Vector3 position;
        public Quaternion rotation;

        public TransformFrameData(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public static TransformFrameData Interpolate(TransformFrameData from, TransformFrameData to, double t)
        {
            // NOTE:
            // Vector3 & Quaternion components are float anyway, so we can
            // keep using the functions with 't' as float instead of double.
            return new TransformFrameData(
                // lerp position/rotation/scale unclamped in case we ever need
                // to extrapolate. atm SnapshotInterpolation never does.
                Vector3.LerpUnclamped(from.position, to.position, (float)t),
                // IMPORTANT: LerpUnclamped(0, 60, 1.5) extrapolates to ~86.
                //            SlerpUnclamped(0, 60, 1.5) extrapolates to 90!
                //            (0, 90, 1.5) is even worse. for Lerp.
                //            => Slerp works way better for our euler angles.
                Quaternion.SlerpUnclamped(from.rotation, to.rotation, (float)t)
            );
        }
    }

    public struct ColliderCastHit
    {
        /// <summary>
        /// Position where the cast enters the collider
        /// </summary>
        public Vector3 entryPoint;
        /// <summary>
        /// Normal vector of the entry
        /// </summary>
        public Vector3 entryNormal;
        /// <summary>
        /// Distance between origin and entry point
        /// </summary>
        public float entryDistance;


        /// <summary>
        /// Position where the cast exits the collider
        /// </summary>
        public Vector3 exitPoint;
        /// <summary>
        /// Normal vector of the exit
        /// </summary>
        public Vector3 exitNormal;
        /// <summary>
        /// Distance between origin and exit point
        /// </summary>
        public float exitDistance;


        /// <summary>
        /// No hit, entryDistance = Mathf.Infinity, exitDistance = Mathf.Infinity
        /// </summary>
        public static ColliderCastHit Zero
        {
            get { return new ColliderCastHit { entryPoint = Vector3.zero, entryNormal = Vector3.zero, entryDistance = Mathf.Infinity, exitPoint = Vector3.zero, exitNormal = Vector3.zero, exitDistance = Mathf.Infinity }; }
        }
    }



}