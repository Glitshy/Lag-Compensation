using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PG.LagCompensation;
using PG.LegacyLagCompensation;

public class TestCharacter : MonoBehaviour
{

    public NetworkTracker[] trackers;

    public HitColliderCollection hitColCollection;

    private float nextTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextTime)
        {
            nextTime += NetworkTracker.updateIntervalTime;
            StoreNetworkTrackerFrameData();
            StoreHitColliderFrameData();
        }
    }

    /// <summary>
    /// Call 'AddFrame()' on every NetworkTracker of this player. Call this after movement updates!
    /// </summary>
    private void StoreNetworkTrackerFrameData()
    {
        for (int i = 0; i < trackers.GetLength(0); i++)
        {
            trackers[i].AddFrame(Time.timeAsDouble);
        }
    }

    /// <summary>
    /// Call 'AddFrame()' on every HitCollider of this player. Call this after movement updates!
    /// </summary>
    private void StoreHitColliderFrameData()
    {
        hitColCollection.AddFrameAll(Time.timeAsDouble);
    }

    [ContextMenu("Get trackers")]
    private void GetTrackers()
    {
        trackers = GetComponentsInChildren<NetworkTracker>();
    }

    [ContextMenu("Add HitColliders Components")]
    private void AddHitCols()
    {
        for (int i = 0; i < trackers.Length; i++)
        {
            Collider col = trackers[i].transform.GetComponent<Collider>();


            if (col is CapsuleCollider)
            {
                HitColliderGeneric hitCol = trackers[i].gameObject.AddComponent<HitColliderCapsule>();
                hitCol.TryGetParametersFromPhysicsCollider();
            }
            if (col is SphereCollider)
            {
                HitColliderGeneric hitCol = trackers[i].gameObject.AddComponent<HitColliderSphere>();
                hitCol.TryGetParametersFromPhysicsCollider();
            }
            if (col is BoxCollider)
            {
                HitColliderGeneric hitCol = trackers[i].gameObject.AddComponent<HitColliderBox>();
                hitCol.TryGetParametersFromPhysicsCollider();
            }
        }
    }
}
