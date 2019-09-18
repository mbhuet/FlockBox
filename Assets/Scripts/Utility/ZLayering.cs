using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZLayering {

	public static float groundTilt = 45;

    public static float YtoZPosition(float yPos)
    {
        //tilt 90 -> 0
        //tilt 0 -> max val
        return Mathf.Tan(groundTilt * Mathf.Deg2Rad) * yPos;
    }

    public static Vector3 GetZLayeredPosition(Vector3 sourcePos)
    {
        return new Vector3(sourcePos.x, sourcePos.y, ZLayering.YtoZPosition(sourcePos.y));

    }
}
