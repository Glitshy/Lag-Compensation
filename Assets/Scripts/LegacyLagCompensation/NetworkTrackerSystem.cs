using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PG.LegacyLagCompensation
{

    /// <summary>
    /// Move all NetworkTracker components to the correct position
    /// </summary>
    public class NetworkTrackerSystem
    {
        public static List<NetworkTracker> SimulationObjects = new List<NetworkTracker>();
        public static List<int> Framekeys = new List<int>();

        public static void Simulate(double simulationTime)
        {
            //if (frameId == Time.frameCount)
            //    frameId = Framekeys[Framekeys.Count - 1];


            for (int i = 0; i < SimulationObjects.Count; i++)
            {
                SimulationObjects[i].SetStateTransform(simulationTime);
            }

            //action.Invoke(); do stuff

            for (int i = 0; i < SimulationObjects.Count; i++)
            {
                SimulationObjects[i].ResetStateTransform();
            }
        }

        /// <summary>
        /// Must call SimulateReset() at end of frame!
        /// </summary>
        /// <param name="simulationTime"></param>
        /// <param name="storeTransforms"></param>
        public static void SimulateStart(double simulationTime, bool storeTransforms = true)
        {
            for (int i = 0; i < SimulationObjects.Count; i++)
            {
                SimulationObjects[i].SetStateTransform(simulationTime, storeTransforms);
            }

            // Important: Colliders for the purpose of Raycasts and Physics are only updated on fixed update. 
            // https://docs.unity3d.com/ScriptReference/Physics-autoSyncTransforms.html
            Physics.SyncTransforms();
        }

        /// <summary>
        /// Reset after having used SumlateStart() this frame
        /// </summary>
        public static void SimulateReset()
        {
            for (int i = 0; i < SimulationObjects.Count; i++)
            {
                SimulationObjects[i].ResetStateTransform();
            }
        }

        /// <summary>
        /// Draw the colliders at theri current positions
        /// </summary>
        public static void DebugDrawColliders()
        {
            for (int i = 0; i < SimulationObjects.Count; i++)
            {
                SimulationObjects[i].DebugDrawColliders(5f);
            }

        }
    }


}