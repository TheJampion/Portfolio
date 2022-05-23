using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

public struct Simplex
{
    public fp3[] points;
    public uint size;

    public Simplex(fp3[] points, uint size)
    {
        this.points = new fp3[] { fp3.zero, fp3.zero, fp3.zero, fp3.zero };
        this.size = 4;
    }
}
