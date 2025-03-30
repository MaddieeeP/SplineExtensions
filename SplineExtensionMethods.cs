using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineExtensionMethods
{
    public static void Evaluate(this ISpline spline, float t, out Vector3 position, out Vector3 forward, out Vector3 up) //Returns Vector3 equivalents of the standard Evaluate method, normalized where appropriate
    {
        spline.Evaluate(t, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);
        position = (Vector3)float3Pos;
        forward = ((Vector3)float3Forward).normalized;
        up = ((Vector3)float3Up).normalized;
    }

    public static Vector3 GetClosestEndPoint(this ISpline spline, Vector3 position, out float progress, out Vector3 forward, out Vector3 up, out float distance)
    {
        spline.Evaluate(0f, out Vector3 end1, out Vector3 forward1, out Vector3 up1);
        spline.Evaluate(1f, out Vector3 end2, out Vector3 forward2, out Vector3 up2);
        float dist1 = Vector3.Distance(position, end1);
        float dist2 = Vector3.Distance(position, end2);
        if (dist1 > dist2)
        {
            progress = 1f;
            forward = forward2;
            up = up2;
            distance = dist2;
            return end2;
        }
        progress = 0f;
        forward = forward1;
        up = up1;
        distance = dist1;
        return end1;
    }

    public static Vector3 GetClosestPoint(this SplineContainer splineContainer, Vector3 position, out float progress, out float distance, int resolution = 4, int iterations = 2)
    {
        Vector3 translatedPosition = splineContainer.transform.InverseTransformPoint(position);
        distance = SplineUtility.GetNearestPoint(splineContainer.Spline, translatedPosition.ToFloat3(), out float3 float3Pos, out progress, resolution, iterations);
        Vector3 pointPosition = splineContainer.transform.TransformPoint((Vector3)float3Pos);

        return pointPosition;
    }

    public static List<Vector3> GetPointPositionsWithOffsets(this ISpline spline, int splineIndex, float progress, List<Vector3> offsets)
    {
        offsets = offsets.OrderByXYArgument();

        spline.Evaluate(progress, out Vector3 position, out Vector3 forward, out Vector3 up);

        Vector3 right = Vector3.Cross(up, forward).normalized;

        List<Vector3> positions = new List<Vector3>();
        foreach (Vector3 offset in offsets)
        {
            positions.Add(position + offset.x * right + offset.y * up + offset.z * forward);
        }

        return positions;
    }

    public static List<List<Vector3>> GetVerts(this ISpline spline, List<Vector3> offsets, int resolution, float portion = 1f) //FIX - rename or refactor maybe
    {
        List<List<Vector3>> verts = new List<List<Vector3>>();

        int pointCount = resolution * (int)Math.Ceiling(spline.GetLength());

        float step = portion / pointCount;
        for (int i = 0; i <= pointCount; i++)
        {
            float t = step * i;
            verts.Add(spline.GetPointPositionsWithOffsets(0, t, offsets));
        }

        return verts;
    }
}