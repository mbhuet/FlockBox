using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtility
{
    public static bool SphereOverlap(Vector3 centerA, float radiusA, Vector3 centerB, float radiusB)
    {
        return Vector3.SqrMagnitude(centerA - centerB) <= ((radiusA + radiusB) * (radiusA + radiusB));
    }



    public static bool LineSegementsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float maxDistance, ref Vector3 pA, ref Vector3 pB)
    {

        Vector3 p13, p43, p21;
        float d1343, d4321, d1321, d4343, d2121;
        float numer, denom;

        p13.x = p1.x - p3.x;
        p13.y = p1.y - p3.y;
        p13.z = p1.z - p3.z;
        p43.x = p4.x - p3.x;
        p43.y = p4.y - p3.y;
        p43.z = p4.z - p3.z;
        if (Mathf.Abs(p43.x) < Mathf.Epsilon && Mathf.Abs(p43.y) < Mathf.Epsilon && Mathf.Abs(p43.z) < Mathf.Epsilon)
            return false;
        p21.x = p2.x - p1.x;
        p21.y = p2.y - p1.y;
        p21.z = p2.z - p1.z;
        if (Mathf.Abs(p21.x) < Mathf.Epsilon && Mathf.Abs(p21.y) < Mathf.Epsilon && Mathf.Abs(p21.z) < Mathf.Epsilon)
            return false;

        d1343 = p13.x * p43.x + p13.y * p43.y + p13.z * p43.z;
        d4321 = p43.x * p21.x + p43.y * p21.y + p43.z * p21.z;
        d1321 = p13.x * p21.x + p13.y * p21.y + p13.z * p21.z;
        d4343 = p43.x * p43.x + p43.y * p43.y + p43.z * p43.z;
        d2121 = p21.x * p21.x + p21.y * p21.y + p21.z * p21.z;

        denom = d2121 * d4343 - d4321 * d4321;
        if (Mathf.Abs(denom) < Mathf.Epsilon)
            return false;
        numer = d1343 * d4321 - d1321 * d4343;

        float mua = Mathf.Clamp01(numer / denom);
        float mub = Mathf.Clamp01((d1343 + d4321 * (mua)) / d4343);
        pA = Vector3.Lerp(p1, p2, mua);
        pB = Vector3.Lerp(p3, p4, mub);
        return (Vector3.SqrMagnitude(pA - pB) < maxDistance * maxDistance);


    }


    public static bool SphereLineOverlap(Vector3 sc, float r, Vector3 p1, Vector3 p2, out float mu1, out float mu2)
    {
        float a, b, c;
        float bb4ac;
        Vector3 dp;

        dp.x = p2.x - p1.x;
        dp.y = p2.y - p1.y;
        dp.z = p2.z - p1.z;
        a = dp.x * dp.x + dp.y * dp.y + dp.z * dp.z;
        b = 2 * (dp.x * (p1.x - sc.x) + dp.y * (p1.y - sc.y) + dp.z * (p1.z - sc.z));
        c = sc.x * sc.x + sc.y * sc.y + sc.z * sc.z;
        c += p1.x * p1.x + p1.y * p1.y + p1.z * p1.z;
        c -= 2 * (sc.x * p1.x + sc.y * p1.y + sc.z * p1.z);
        c -= r * r;
        bb4ac = b * b - 4 * a * c;
        if (Mathf.Abs(a) < Mathf.Epsilon || bb4ac < 0)
        {
            mu1 = 0;
            mu2 = 0;
            return false;
        }

        mu1 = (-b + Mathf.Sqrt(bb4ac)) / (2 * a);
        mu2 = (-b - Mathf.Sqrt(bb4ac)) / (2 * a);
        return true;
    }
}
