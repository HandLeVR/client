using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The evaluation cone consists of multiple evaluation rays starting at the muzzle of the spray gun.
/// The rays are used to estimate how much paint reaches an object.
/// </summary>
public class EvaluationCone : MonoBehaviour
{
    public Transform origin;
    public float height;
    public float width;
    public float maxDistance;
    public float minDistance;
    public EvaluationRayPoint evaluationRayPointPrefab;

    private List<EvaluationRayPoint> _evalRayPoints;

    private readonly int EVAL_RAY_RING_COUNT = 5;
    private readonly int MAX_EVAL_RAYS_PER_RING = 16;
    private readonly int MIN_EVAL_RAYS_PER_RING = 4;

    void Start()
    {
        _evalRayPoints = new List<EvaluationRayPoint>();
        GenerateEvalRayPoints();
    }

    /// <summary>
    /// Generates multiple target points in form of an ellipse. They will be evenly distributed over multiple rings
    /// in the ellipse.
    /// </summary>
    private void GenerateEvalRayPoints()
    {
        Vector3 originPos = origin.position;
        Vector3 coneGroundCenter = originPos + origin.forward.normalized * 2.5f;
        for (int i = 1; i <= EVAL_RAY_RING_COUNT; i++)
        {
            Ellipse ellipse = new Ellipse(coneGroundCenter, origin.up, origin.right, height / i, width / i);
            int ringNumber = Convert.ToInt32(MIN_EVAL_RAYS_PER_RING + (EVAL_RAY_RING_COUNT - i) /
                ((float)EVAL_RAY_RING_COUNT - 1) * (MAX_EVAL_RAYS_PER_RING - MIN_EVAL_RAYS_PER_RING));
            for (int j = 1; j <= ringNumber; j++)
            {
                Vector3 pointPosition = ellipse.PointAt(j / (float)ringNumber * (2 * Mathf.PI));
                _evalRayPoints.Add(Instantiate(evaluationRayPointPrefab, pointPosition, Quaternion.identity,
                    transform));
            }
        }
    }

    /// <summary>
    /// Estimates how much paint hit the workpiece by the number of rays hitting the workpiece.
    /// </summary>
    /// <returns></returns>
    public float GetSprayHit()
    {
        float aggregatedHitAmount = 0;
        Vector3 originPos = origin.position;
        foreach (var evalRayPoint in _evalRayPoints)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(originPos, evalRayPoint.transform.position - originPos, out hitInfo, maxDistance,
                1 << 11))
            {
                float distance = Vector3.Distance(originPos, hitInfo.point);
                float hitAmount = 1 - Mathf.Clamp((distance - minDistance) / (maxDistance - minDistance), 0, 1);
                aggregatedHitAmount += hitAmount;
            }
        }

        return aggregatedHitAmount / _evalRayPoints.Count;
    }

    public void DrawDebugRays()
    {
        Vector3 originPos = origin.position;
        foreach (var evalRayPoint in _evalRayPoints)
        {
            evalRayPoint.DrawDebugLine(originPos, maxDistance);
        }
    }
}