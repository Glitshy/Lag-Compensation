using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PG.LagCompensation;

public class HitColliderTester : MonoBehaviour
{
    public HitColliderGeneric[] colliders;

    public float maxDistance = 10f;





    [Header("Performance Test Settings")]
    public bool doPerformanceTest = false;
    public int loopCount = 10;

    [Header("Performance Test Results")]
    public double[] summedTime;

    // Start is called before the first frame update
    void Start()
    {
        if (!doPerformanceTest)
            return;

        StartCoroutine(DelayedAction());
    }



    private IEnumerator DelayedAction()
    {
        yield return new WaitForSeconds(0.5f);

		for (int i = 0; i < colliders.Length; i++)
		{
            double t = Time.realtimeSinceStartupAsDouble;

			for (int k = 0; k < loopCount; k++)
			{
                colliders[i].ColliderCast(transform.position, transform.forward, maxDistance, out ColliderCastHit _hit);

            }


            summedTime[i] = Time.realtimeSinceStartupAsDouble - t;
        }


        
    }


    // Update is called once per frame
    void Update()
    {
        if (doPerformanceTest)
            return;

        ColliderCastHit _hit;

        Vector3 o = transform.position;
        Vector3 d = transform.forward;

        if (ColliderCastSystem.ColliderCastTransform(o, d, maxDistance, out _hit, out HitColliderCollection collection, out int hitColIndex))
		{
            Debug.DrawLine(_hit.entryPoint, _hit.entryPoint + _hit.entryNormal, Color.yellow);
            Debug.DrawLine(o, _hit.entryPoint, Color.green);

            Debug.DrawLine(_hit.exitPoint, _hit.exitPoint + _hit.exitNormal, Color.magenta);
            Debug.DrawLine(_hit.entryPoint, _hit.exitPoint, Color.grey);
        }
        else
		{
            Debug.DrawLine(o, o + d * maxDistance, Color.red);
        }

        /*

        if (colliders.Length >= 1)
		{
            ColliderCastHit _hit;

            Vector3 o = transform.position;
            Vector3 d = transform.forward;

            if (colliders[0].ColliderCast(o, d, maxDistance, out _hit))
			{
                Debug.DrawLine(_hit.entryPoint, _hit.entryPoint + _hit.entryNormal, Color.yellow);
                Debug.DrawLine(o, _hit.entryPoint, Color.green);

                Debug.DrawLine(_hit.exitPoint, _hit.exitPoint + _hit.exitNormal, Color.magenta);
                Debug.DrawLine(_hit.entryPoint, _hit.exitPoint, Color.grey);
            }
            else
			{
                Debug.DrawLine(o, o + d * maxDistance, Color.red);
			}

        }

        */
    }
}
