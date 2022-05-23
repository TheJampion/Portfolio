using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MythrenFighter
{
    public static class Extensions
    {
        public static fp3 noY(this fp3 v)
        {
            fp3 retValue = v;
            retValue.y = 0;
            return retValue;
        }

        public static Vector3 noX(this Vector3 v)
        {
            Vector3 retValue = v;
            retValue.x = 0;
            return retValue;
        }

        public static Vector3 noY(this Vector3 v)
        {
            Vector3 retValue = v;
            retValue.y = 0;
            return retValue;
        }

        public static Vector3 noZ(this Vector3 v)
        {
            Vector3 retValue = v;
            retValue.z = 0;
            return retValue;
        }

        public static fp ToFixedPoint(this float value)
        {
            fp fixedPoint;
            fixedPoint.value = (long)(value * 65536f);
            return fixedPoint;
        }

        public static Vector3 ToVector3(this fp3 v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public static fp3 ToFp3(this Vector3 v)
        {
            return new fp3(v.x.ToFixedPoint(), v.y.ToFixedPoint(), v.z.ToFixedPoint());
        }

        public static fp Magnitude(this fp3 v)
        {
            return fixmath.Magnitude(v);
        }

        public static fp3 Normalize(this fp3 v)
        {
            return fixmath.Normalize(v);
        }

        public static fp3 InterpolateFp3(fp3 current, fp3 goal, fp distance)
        {
            fp3 delta = goal - current;
            if (delta.Magnitude() < distance)
            {
                return goal;
            }
            delta = delta.Normalize() * distance;
            return current + delta;
        }

        public static byte[] SerializeData<T>(T data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, data);
                return ms.ToArray();
            }
        }

        public static T DeserializeData<T>(byte[] serializedData)
        {
            using (var ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                ms.Write(serializedData, 0, serializedData.Length);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)(bf.Deserialize(ms));
            }
        }

        public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
        {
            if (scriptableObject == null)
            {
                Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
                return (T)ScriptableObject.CreateInstance(typeof(T));
            }

            T instance = Object.Instantiate(scriptableObject);
            instance.name = scriptableObject.name; // remove (Clone) from name
            return instance;
        }
    }
}