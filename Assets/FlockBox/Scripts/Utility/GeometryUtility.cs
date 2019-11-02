using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtility
{
    public static bool SphereOverlap(Vector3 centerA, float radiusA, Vector3 centerB, float radiusB)
    {
        return Vector3.SqrMagnitude(centerA - centerB) <= ((radiusA + radiusB) * (radiusA + radiusB));
    }

    //find closest point on line to center of sphere
    public static bool SphereLineOverlap(Vector3 center, float radius, Vector3 p1, Vector3 p2)
    {
        return (Vector3.Project(center - p1, p2 - p1) - center).sqrMagnitude <= radius * radius;
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

    public static bool RaySphereIntersection(Ray r, Vector3 center, float radius, ref float t)
    {
        Vector3 oc = r.origin - center;
        float a = Vector3.Dot(r.direction, r.direction);
        float b = 2f * Vector3.Dot(oc, r.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return false;
        }
        else
        {
            float numerator = -b - Mathf.Sqrt(discriminant);
            if (numerator > 0)
            {
                t= numerator / (2f * a);
                return true;
            }

            numerator = -b + Mathf.Sqrt(discriminant);
            if (numerator > 0)
            {
                t = numerator / (2f * a);
                return true;
            }
            else
            {
                return false;
            }
        }
    }



    // ray-cylinder intersetion (returns t and normal)
    public static bool RayCylinderIntersection(Ray ray, Vector3 pa, Vector3 pb, float ra, ref float t, ref Vector3 normal) // point a, point b, radius
    {
        // center the cylinder, normalize axis
        Vector3 cc = 0.5f * (pa + pb);
        float ch = Vector3.Magnitude(pb - pa);
        Vector3 ca = (pb - pa) / ch;
        ch *= 0.5f;

        Vector3 oc = ray.origin - cc;

        float card = Vector3.Dot(ca, ray.direction);
        float caoc = Vector3.Dot(ca, oc);

        float a = 1.0f - card * card;
        float b = Vector3.Dot(oc, ray.direction) - caoc * card;
        float c = Vector3.Dot(oc, oc) - caoc * caoc - ra * ra;
        float h = b * b - a * c;
        if (h < 0.0)
        {
            t = -1;
            normal = Vector3.zero;
            return false;
        }
        h = Mathf.Sqrt(h);
        float t1 = (-b - h) / a;
        //float t2 = (-b+h)/a; // exit point

        float y = caoc + t1 * card;

        // body
        if (Mathf.Abs(y) < ch)
        {
            t = t1;
            normal = (oc + t1 * ray.direction - ca * y).normalized;
            return true;
        }

        // caps
        float sy = Mathf.Sign(y);
        float tp = (sy * ch - caoc) / card;
        if (Mathf.Abs(b + a * tp) < h)
        {
            t = tp;
            normal = ca * sy;
            return true;
        }

        t = 1;
        normal = Vector3.zero;
        return false;
    }

}
