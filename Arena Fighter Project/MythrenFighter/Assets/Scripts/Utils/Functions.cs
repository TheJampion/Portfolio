using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class Functions
{
    public static Vector3 getSlopeVector(Vector3 groundNormal, Vector3 direction)
    {
        float slopeAngle = (Vector3.Angle(groundNormal, direction) - 90) * Mathf.PI / 180;
        return Vector3.RotateTowards(direction, Vector3.up, slopeAngle, 0);
    }

    public static int modulus(int a, int b)
    {
        return (Mathf.Abs(a * b) + a) % b;
    }

    public static Vector3 theRealVector3Lerp(Vector3 startPos, Vector3 endPos, ref float currentLerpTime, float lerpTime)
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > lerpTime)
        {
            currentLerpTime = lerpTime;
        }

        float t = currentLerpTime / lerpTime;
        t = t * t * (3f - 2f * t);
        return Vector3.Lerp(startPos, endPos, t);
    }

    public static float theRealLerp(float startPos, float endPos, ref float currentLerpTime, float lerpTime)
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > lerpTime)
        {
            currentLerpTime = lerpTime;
        }

        float t = currentLerpTime / lerpTime;
        t = t * t * (3f - 2f * t);
        return Mathf.Lerp(startPos, endPos, t);
    }

    public static Vector3 correctJoystickInput(Vector3 moveInput)
    {
        float xAbs = Mathf.Abs(moveInput.x);
        float zAbs = Mathf.Abs(moveInput.z);
        if (xAbs >= zAbs && zAbs != 0)
        {
            float ratio = moveInput.magnitude / new Vector3(1, 0, zAbs / xAbs).magnitude;
            moveInput = moveInput.normalized * ratio;
        }
        else if (zAbs > xAbs)
        {
            float ratio = moveInput.magnitude / new Vector3(xAbs / zAbs, 0, 1).magnitude;
            moveInput = moveInput.normalized * ratio;
        }
        return moveInput;
    }

    public static byte[] serializeData<T>(T data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, data);
            return ms.ToArray();
        }
    }

    public static T deserializeData<T>(byte[] serializedData)
    {
        using (var ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            ms.Write(serializedData, 0, serializedData.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)(bf.Deserialize(ms));
        }
    }

    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }

    public static Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos, Vector3 screenOffset)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos + screenOffset, parentCanvas.worldCamera, out movePos);
        //Convert the local point to world point
        return parentCanvas.transform.TransformPoint(movePos);
    }

    public static bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
    {
        bool isInside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
            (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }

    // Interpolates a float by step, step should be positive
    public static float interpolateFloat(float current, float goal, float step)
    {
        if (current == goal)
        {
            return goal;
        }
        if (current > goal)
        {
            step *= -1;
        }
        float retValue = current + step;
        if (Mathf.Abs(retValue - goal) < step)
        {
            return goal;
        }
        return retValue;
    }

    public static Vector3 interpolateVector3Rotation(Vector3 current, Vector3 goal, float angleStep)
    {
        return Vector3.RotateTowards(current, goal, angleStep, 1);
    }

    public static Vector3 interpolateVector3(Vector3 current, Vector3 goal, float distance)
    {
        Vector3 delta = goal - current;
        if (delta.magnitude < distance)
        {
            return goal;
        }
        delta = delta.normalized * distance;
        return current + delta;
    }
}
