using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PG.LagCompensation;
using PG.LegacyLagCompensation;

public class TestNetworkTracker : MonoBehaviour
{
    private enum TestType { testPhysicsRaycast, testColliderCast, testBothWithLoop }


    [Header("Test Settings")]
    [SerializeField]
    private TestType testType;

    [Tooltip("Do check after X seconds")]
    public float doCheckAfterSeconds = 0.5f;
    [Tooltip("When check happends: Turn back time by X seconds")]
    public double catchUpTime = 0.3d;

    public int loopCount = 10;

    public float raycastRangePerFixedUpdate = 5f;

    public LayerMask mask;


    


    private bool destroy;


    

    [Header("Performance Test Results")]
    public double[] summedTime;



    private void Start()
    {
        if (catchUpTime > doCheckAfterSeconds)
		{
            Debug.LogError("catchUpTime is larger than doCheckAfterSeconds");
            return;
		}

        StartCoroutine(DelayedTest());
    }

    private IEnumerator DelayedTest()
    {
        yield return new WaitForSeconds(doCheckAfterSeconds);


        switch(testType)
		{
            case TestType.testBothWithLoop:
                double _t = Time.realtimeSinceStartupAsDouble;
                for (int i = 0; i < loopCount; i++)
                {
                    TestParametric(Time.timeAsDouble - catchUpTime);
                }
                summedTime[0] = Time.realtimeSinceStartupAsDouble - _t;

                _t = Time.realtimeSinceStartupAsDouble;
                for (int i = 0; i < loopCount; i++)
                {
                    TestPhysics(Time.timeAsDouble - catchUpTime);
                }
                summedTime[1] = Time.realtimeSinceStartupAsDouble - _t;

                break;

            case TestType.testPhysicsRaycast:
                TestParametric(Time.timeAsDouble - catchUpTime);
                break;
            case TestType.testColliderCast:
                TestPhysics(Time.timeAsDouble - catchUpTime);
                break;

        }




    }

    

    


    


    

    [ContextMenu("Test 1s ago")]
    private void TestPastSecond()
    {
        TestPhysics(Time.timeAsDouble - 1d);
    }

    /// <summary>
    /// Using standard physics colliders and Sync physics
    /// </summary>
    /// <param name="_serverTime"></param>
    private void TestPhysics(double _serverTime)
    {
        double _serverTimeTicker = _serverTime;  //  - GameManager.instance.shootDelayTime
        double _currentServerTime = Time.timeAsDouble;

        bool _firstLoop = true;

        NetworkTrackerSystem.SimulateStart(_serverTimeTicker, true);

        int _iterationCount = 0;

        while (!destroy && _serverTimeTicker < _currentServerTime)
        {
            float _delta = Mathf.Min(Time.fixedDeltaTime, (float)(_currentServerTime - _serverTimeTicker));

            if (_firstLoop)
            {
                _firstLoop = false;
            }
            else
            {
                NetworkTrackerSystem.SimulateStart(_serverTimeTicker, false);

            }

            if (PhysicsUpdateLoop(_iterationCount))
			{
                if (testType != TestType.testBothWithLoop)
                    NetworkTrackerSystem.DebugDrawColliders();
            }

            _serverTimeTicker += _delta; // either fixedDeltaTime or final step to be up to date
            _iterationCount++;

        }

        NetworkTrackerSystem.SimulateReset();


    }

    private bool PhysicsUpdateLoop(int _countDEBUG = 0)
    {
        float _dist = raycastRangePerFixedUpdate;
        Vector3 _pos = Vector3.zero;
        Vector3 _dir = Vector3.forward;


        Vector3 _start = _pos + _dir * _countDEBUG * _dist;

        if (Physics.Raycast(_start, _dir, out RaycastHit hit, _dist, mask))
        {
            if (testType != TestType.testBothWithLoop)
            {
                Debug.DrawLine(_start, hit.point, Color.green, 10f);
                Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.1f, Color.green, 10f);
            }
            return true;
        }
        else
        {
            if (testType != TestType.testBothWithLoop)
            {
                Vector3 _end = _pos + _dir * (_countDEBUG + 1) * _dist;

                Debug.DrawLine(_start, _end, Color.red, 10f);
                Debug.DrawLine(_end, _end + Vector3.up * 0.1f, Color.red, 10f);
            }

        }

        return false;
    }



    /// <summary>
    /// Using custom hit colliders
    /// </summary>
    /// <param name="_serverTime"></param>
    private void TestParametric(double _serverTime)
    {
        double _serverTimeTicker = _serverTime;  //  - GameManager.instance.shootDelayTime
        double _currentServerTime = Time.timeAsDouble;


        int _iterationCount = 0;

        while (!destroy && _serverTimeTicker < _currentServerTime)
        {
            float _delta = Mathf.Min(Time.fixedDeltaTime, (float)(_currentServerTime - _serverTimeTicker));

            ColliderCastSystem.Simulate(_serverTimeTicker);


            if (ParametricUpdateLoop(_iterationCount))
            {
                if (testType != TestType.testBothWithLoop)
                    ColliderCastSystem.DebugDrawColliders();
            }

            _serverTimeTicker += _delta; // either fixedDeltaTime or final step to be up to date
            _iterationCount++;

        }


    }


    private bool ParametricUpdateLoop(int _countDEBUG = 0)
    {
        float _dist = raycastRangePerFixedUpdate;
        Vector3 _pos = Vector3.zero;
        Vector3 _dir = Vector3.forward;


        Vector3 _start = _pos + _dir * _countDEBUG * _dist;

        
        if (ColliderCastSystem.ColliderCastInterpolatedFrameData(_start, _dir, _dist, out ColliderCastHit hit))
        {
            if (testType != TestType.testBothWithLoop)
            {
                Debug.DrawLine(_start, hit.entryPoint, Color.green, 10f);
                Debug.DrawLine(hit.entryPoint, hit.entryPoint + Vector3.up * 0.1f, Color.green, 10f);
            }
            return true;
        }
        else
        {
            if (testType != TestType.testBothWithLoop)
            {
                Vector3 _end = _pos + _dir * (_countDEBUG + 1) * _dist;

                Debug.DrawLine(_start, _end, Color.red, 10f);
                Debug.DrawLine(_end, _end + Vector3.up * 0.1f, Color.red, 10f);
            }

        }


        return false;
    }

}