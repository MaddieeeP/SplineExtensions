using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineExtensionMethods
{
    public static void EvaluateInWorldSpace(this SplineContainer splineContainer, float t, out Vector3 position, out Vector3 forward, out Vector3 up)
    {
        splineContainer.Spline.Evaluate(t, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);
        position = splineContainer.transform.TransformPoint((Vector3)float3Pos);
        forward = splineContainer.transform.TransformVector((Vector3)float3Forward).normalized;
        up = splineContainer.transform.TransformVector((Vector3)float3Up).normalized;
    }

    public static Vector3 GetNearestPointInWorldSpace(this SplineContainer splineContainer, Vector3 position, out float t, out float distance, int resolution = 4, int iterations = 2)
    {
        Vector3 translatedPosition = splineContainer.transform.InverseTransformPoint(position);
        distance = SplineUtility.GetNearestPoint(splineContainer.Spline, translatedPosition.ToFloat3(), out float3 float3Pos, out t, resolution, iterations);
        Vector3 pointPosition = splineContainer.transform.TransformPoint((Vector3)float3Pos);

        return pointPosition;
    }
}