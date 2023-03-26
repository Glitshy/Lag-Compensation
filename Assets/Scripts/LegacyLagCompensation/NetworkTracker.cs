using System;
using System.Collections.Generic;
using UnityEngine;
using PG.LagCompensation;


namespace PG.LegacyLagCompensation
{

    /// <summary>
    /// Track and store transforms of this object in a list and rewind time for porjectile caluclations. Should only run on server, disabled on clients
    /// </summary>
    public class NetworkTracker : MonoBehaviour
    {

        public static int FrameHistory = 40;
        public static float updateIntervalTime = 0.2f;

        private List<TransformFrameData> FrameData = new List<TransformFrameData>();
        public List<double> FrameTimes = new List<double>();


        /// <summary>
        /// Last postion and rotation before rewinding
        /// </summary>
        private TransformFrameData savedFrameData = new TransformFrameData();

        private BoxCollider boxCollider;
        private CapsuleCollider capsuleCollider;

        void Start()
        {
            NetworkTrackerSystem.SimulationObjects.Add(this);

            boxCollider = transform.GetComponent<BoxCollider>();
            capsuleCollider = transform.GetComponent<CapsuleCollider>();


        }


        void OnDestroy()
        {
            NetworkTrackerSystem.SimulationObjects.Remove(this);
        }


        /// <summary>
        /// Add postion/rotation with timestamp to list. Call this after doing movement updates!
        /// </summary>
        public void AddFrame(double _time)
        {
            if (FrameTimes.Count >= FrameHistory) // remove oldest stored frame
            {
                FrameTimes.RemoveAt(0);
                FrameData.RemoveAt(0);
            }

            FrameData.Add(new TransformFrameData() { position = transform.position, rotation = transform.rotation });
            FrameTimes.Add(_time);
        }


        /// <summary>
        /// Set transform corresponding to desired simulation time. Can optionally be forced NOT to save current state, useful to save performance when simulation multiple positions in a singel frame. 
        /// Always call ResetStateTransform() at the end of the frame
        /// </summary>
        /// <param name="simulationTime"></param>
        /// <param name="_storeCurrentTransform">First simulation of the frame should always sore the transform, following simulations in the same frame shouldn't.</param>
        public void SetStateTransform(double simulationTime, bool _storeCurrentTransform = true)
        {
            if (_storeCurrentTransform)
            {
                savedFrameData.position = transform.position; // store current
                savedFrameData.rotation = transform.rotation; // store current
            }


            for (int i = FrameTimes.Count - 1; i >= 0; i--)
            {
                if (FrameTimes[i] <= simulationTime) // if the data at [i] is older than the desired simulation time
                {
                    double timeOlder = FrameTimes[i];
                    TransformFrameData _interpolatedFrame;

                    if (i < FrameTimes.Count - 1) // if there is a newer frame
                    {
                        double timeNewer = FrameTimes[i + 1];

                        double fraction = Math.Clamp((simulationTime - timeOlder) / (timeNewer - timeOlder), 0d, 1d);
                        _interpolatedFrame = TransformFrameData.Interpolate(FrameData[i], FrameData[i + 1], fraction);
                    }
                    else // there is no newer frame --> interpolate between this 'newest' frame and the current position!
                    {
                        double fraction = Math.Clamp((simulationTime - timeOlder) / (Time.timeAsDouble - timeOlder), 0d, 1d);

                        _interpolatedFrame = TransformFrameData.Interpolate(FrameData[i], new TransformFrameData(transform.position, transform.rotation), fraction);
                    }

                    transform.SetPositionAndRotation(_interpolatedFrame.position, _interpolatedFrame.rotation);

                    return;
                }
            }

        }

        /// <summary>
        /// Re-apply previous position and rotation
        /// </summary>
        public void ResetStateTransform()
        {
            transform.SetPositionAndRotation(savedFrameData.position, savedFrameData.rotation);
        }

        /// <summary>
        /// Draw collider with Debug.DrawLine, for given duration
        /// </summary>
        /// <param name="_duration"></param>
        public void DebugDrawColliders(float _duration)
        {
            if (capsuleCollider != null)
			{
                HitColliderCapsule.DebugDrawCapsule(transform.TransformPoint(capsuleCollider.center), transform.rotation, capsuleCollider.direction, capsuleCollider.height, capsuleCollider.radius, _duration, Color.red);
			}
            else if (boxCollider)
			{
                HitColliderBox.DebugDrawBox(transform.position, transform.rotation, boxCollider.size, boxCollider.center, _duration, Color.red);
            }
        }



        private void OnDrawGizmosSelected()
        {

            if (FrameTimes.Count > 0) // Application.isPlaying
            {
                if (boxCollider != null || capsuleCollider != null)
                {
                    SetStateTransform(Time.timeAsDouble - 1d);

                    if (boxCollider != null)
                    {
                        Vector3 centerInGlobalSpace = transform.TransformPoint(boxCollider.center);

                        float _radius = 0.05f;

                        Gizmos.DrawSphere(centerInGlobalSpace, _radius);

                        Gizmos.DrawSphere(centerInGlobalSpace + transform.right * (boxCollider.size.x / 2f - _radius) + transform.up * (boxCollider.size.y / 2f - _radius) + transform.forward * (boxCollider.size.z / 2f - _radius), _radius);
                        Gizmos.DrawSphere(centerInGlobalSpace - transform.right * (boxCollider.size.x / 2f - _radius) + transform.up * (boxCollider.size.y / 2f - _radius) + transform.forward * (boxCollider.size.z / 2f - _radius), _radius);
                        Gizmos.DrawSphere(centerInGlobalSpace - transform.right * (boxCollider.size.x / 2f - _radius) - transform.up * (boxCollider.size.y / 2f - _radius) + transform.forward * (boxCollider.size.z / 2f - _radius), _radius);
                        Gizmos.DrawSphere(centerInGlobalSpace - transform.right * (boxCollider.size.x / 2f - _radius) - transform.up * (boxCollider.size.y / 2f - _radius) - transform.forward * (boxCollider.size.z / 2f - _radius), _radius);
                        Gizmos.DrawSphere(centerInGlobalSpace + transform.right * (boxCollider.size.x / 2f - _radius) - transform.up * (boxCollider.size.y / 2f - _radius) + transform.forward * (boxCollider.size.z / 2f - _radius), _radius);
                        Gizmos.DrawSphere(centerInGlobalSpace + transform.right * (boxCollider.size.x / 2f - _radius) - transform.up * (boxCollider.size.y / 2f - _radius) - transform.forward * (boxCollider.size.z / 2f - _radius), _radius);
                        Gizmos.DrawSphere(centerInGlobalSpace + transform.right * (boxCollider.size.x / 2f - _radius) + transform.up * (boxCollider.size.y / 2f - _radius) - transform.forward * (boxCollider.size.z / 2f - _radius), _radius);
                        Gizmos.DrawSphere(centerInGlobalSpace - transform.right * (boxCollider.size.x / 2f - _radius) + transform.up * (boxCollider.size.y / 2f - _radius) - transform.forward * (boxCollider.size.z / 2f - _radius), _radius);

                        //Gizmos.DrawCube(boxCollider.transform.position, boxCollider.size);

                    }
                    else if (capsuleCollider != null)
                    {
                        Vector3 centerInGlobalSpace = transform.TransformPoint(capsuleCollider.center);

                        Gizmos.DrawSphere(centerInGlobalSpace + transform.up * (capsuleCollider.height / 2f - capsuleCollider.radius), capsuleCollider.radius);
                        Gizmos.DrawSphere(centerInGlobalSpace, capsuleCollider.radius);
                        Gizmos.DrawSphere(centerInGlobalSpace - transform.up * (capsuleCollider.height / 2f - capsuleCollider.radius), capsuleCollider.radius);


                    }

                    ResetStateTransform();
                }



            }

        }




    }




}