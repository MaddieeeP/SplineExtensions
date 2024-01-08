using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineExtensionMethods
{
    public static Vector3 GetPoint(this SplineContainer splineContainer, int splineIndex, float progress, out Vector3 forward, out Vector3 upVector)
    {
        splineContainer.Evaluate(splineIndex, progress, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);
        forward = ((Vector3)float3Forward).normalized;
        upVector = ((Vector3)float3Up).normalized;
        return (Vector3)float3Pos;
    }

    public static Vector3 GetPoint(this SplineContainer splineContainer, int splineIndex, float progress)
    {
        splineContainer.Evaluate(splineIndex, progress, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);
        return (Vector3)float3Pos;
    }

    public static Vector3 GetForward(this SplineContainer splineContainer, int splineIndex, float progress)
    {
        splineContainer.Evaluate(splineIndex, progress, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);
        return (Vector3)float3Forward;
    }

    public static Vector3 GetUp(this SplineContainer splineContainer, int splineIndex, float progress)
    {
        splineContainer.Evaluate(splineIndex, progress, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);
        return (Vector3)float3Up;
    }

    public static Vector3 GetNormal(this SplineContainer splineContainer, int splineIndex, float progress, Vector3 attackVector)
    {
        splineContainer.Evaluate(splineIndex, progress, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);

        Vector3 forward = (Vector3)float3Forward;
        attackVector = (Quaternion.Euler(forward) * attackVector).RemoveComponentAlongAxis(forward).normalized;
        Vector3 normal = Quaternion.Euler(attackVector).DivideBy(Quaternion.Euler(-1f * Vector3.up)) * (Vector3)float3Up;

        return normal.normalized;
    }

    public static List<Vector3> GetPointPositionsWithOffsets(this SplineContainer splineContainer, int splineIndex, float progress, List<Vector3> offsets)
    {
        offsets = offsets.OrderByXYArgument();

        splineContainer.Evaluate(splineIndex, progress, out float3 float3Pos, out float3 float3Forward, out float3 float3Up);

        Vector3 postition = (Vector3)float3Pos;
        Vector3 up = (Vector3)float3Up;
        Vector3 forward = (Vector3)float3Forward;
        Vector3 right = Vector3.Cross(up, forward).normalized;

        List<Vector3> positions = new List<Vector3>();
        foreach (Vector3 offset in offsets)
        {
            positions.Add(postition + offset.x * right + offset.y * up + offset.z * forward);
        }

        return positions;
    }

    public static List<List<Vector3>> GetVerts(this SplineContainer splineContainer, List<Vector3> offsets, int resolution, float portion = 1f)
    {
        List<List<Vector3>> verts = new List<List<Vector3>>();

        int pointCount = resolution * (int)Math.Ceiling(splineContainer.CalculateLength());

        float step = portion / pointCount;
        for (int i = 0; i <= pointCount; i++)
        {
            float t = step * i;
            verts.Add(splineContainer.GetPointPositionsWithOffsets(0, t, offsets));
        }

        return verts;
    }

    public static Vector3 GetClosestEndPoint(this SplineContainer splineContainer, Vector3 position, out float progress, out float distance)
    {
        Vector3 end1 = splineContainer.GetPoint(0, 0f);
        Vector3 end2 = splineContainer.GetPoint(0, 1f);
        float dist1 = Vector3.Distance(position, end1);
        float dist2 = Vector3.Distance(position, end2);
        if (dist1 > dist2)
        {
            progress = 1f;
            distance = dist2;
            return end2;
        }
        progress = 0f;
        distance = dist1;
        return end1;
    }

    public static Vector3 GetClosestPoint(this SplineContainer splineContainer, Vector3 position, out float progress, out float distance, int resolution = 4, int iterations = 2)
    {
        Spline spline = splineContainer.Spline;
        Vector3 translatedPosition = splineContainer.transform.InverseTransformPoint(position);
        distance = SplineUtility.GetNearestPoint<Spline>(spline, translatedPosition.ToFloat3(), out float3 float3Pos, out progress, resolution, iterations);
        Vector3 pointPosition = splineContainer.transform.TransformPoint((Vector3)float3Pos);

        return pointPosition;
    }
}