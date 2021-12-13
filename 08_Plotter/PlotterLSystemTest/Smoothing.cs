// Smoothing algorithms
// Initial code from https://github.com/xstos/PolylineSmoothCSharp.git
// Added CutCorners method
//
// 2021-12-12   PV

using System;
using System.Collections.Generic;

namespace LSystemTest;

internal static class Smoothing
{
    static internal List<PointD> GetCurveSmoothingChaikin(List<PointD> points, float tension, int nrOfIterations)
    {
        if (points.Count < 3)
            return points;

        if (nrOfIterations < 1)
            nrOfIterations = 1;
        else if (nrOfIterations > 10)
            nrOfIterations = 10;

        if (tension < 0)
            tension = 0;
        else if (tension > 1
         )
            tension = 1;

        // the tension factor defines a scale between corner cutting distance in segment half length, i.e. between 0.05 and 0.45
        // the opposite corner will be cut by the inverse (i.e. 1-cutting distance) to keep symmetry
        // with a tension value of 0.5 this amounts to 0.25 = 1/4 and 0.75 = 3/4 the original Chaikin values
        float cutdist = 0.05f + tension * 0.4f;

        // make a copy of the pointlist and feed it to the iteration
        var nl = new List<PointD>();
        var loopTo = points.Count - 1;
        for (int i = 0; i <= loopTo; i++)
            nl.Add(new PointD(points[i]));
        var loopTo1 = nrOfIterations;
        for (int i = 1; i <= loopTo1; i++)
            nl = GetSmootherChaikin(nl, cutdist);

        return nl;
    }

    static private List<PointD> GetSmootherChaikin(List<PointD> points, float cuttingDist)
    {
        PointD q, r;
        var loopTo = points.Count - 2;

        var nl = new List<PointD> { new PointD(points[0]) };    // always add the first point
        for (int i = 0; i <= loopTo; i++)
        {
            q = (1 - cuttingDist) * points[i] + cuttingDist * points[i + 1];
            r = cuttingDist * points[i] + (1 - cuttingDist) * points[i + 1];
            nl.Add(q);
            nl.Add(r);
        }
        nl.Add(new PointD(points[^1]));     // always add the last point

        return nl;
    }

    static internal List<PointD> GetSplineInterpolationCatmullRom(List<PointD> points, int nrOfInterpolatedPoints)
    {
        if (points.Count < 3)
            return points;

        // The Catmull-Rom Spline, requires at least 4 points so it is possible to extrapolate from 3 points, but not from 2.
        // you would get a straight line anyway
        //if (points.Count < 3)
        //    throw new Exception("Catmull-Rom Spline requires at least 3 points");

        // could throw an error on the following, but it is easily fixed implicitly
        if (nrOfInterpolatedPoints < 1)
            nrOfInterpolatedPoints = 1;

        // create a new pointlist to do splining on
        // if you don't do this, the original pointlist gets extended with the exptrapolated points
        var spoints = new List<PointD>();
        foreach (PointD p in points)
            spoints.Add(new PointD(p));

        // always extrapolate the first and last point out
        float dx = spoints[1].X - spoints[0].X;
        float dy = spoints[1].Y - spoints[0].Y;
        spoints.Insert(0, new PointD(spoints[0].X - dx, spoints[0].Y - dy));
        dx = spoints[^1].X - spoints[^2].X;
        dy = spoints[^1].Y - spoints[^2].Y;
        spoints.Insert(spoints.Count, new PointD(spoints[^1].X + dx, spoints[^1].Y + dy));

        // Note the nrOfInterpolatedPoints acts as a kind of tension factor between 0 and 1 because it is normalised
        // to 1/nrOfInterpolatedPoints. It can never be 0
        float t = 0;
        PointD spoint;
        var spline = new List<PointD>();
        var loopTo = spoints.Count - 4;
        for (int i = 0; i <= loopTo; i++)
        {
            var loopTo1 = nrOfInterpolatedPoints - 1;
            for (int intp = 0; intp <= loopTo1; intp++)
            {
                t = 1 / (float)nrOfInterpolatedPoints * intp;
                spoint = 0.5f * (2 * spoints[i + 1] + (-1 * spoints[i] + spoints[i + 2]) * t + (2 * spoints[i] - 5 * spoints[i + 1] + 4 * spoints[i + 2] - spoints[i + 3]) * (float)Math.Pow(t, 2) + (-1 * spoints[i] + 3 * spoints[i + 1] - 3 * spoints[i + 2] + spoints[i + 3]) * (float)Math.Pow(t, 3));
                spline.Add(new PointD(spoint));
            }
        }

        // add the last point, but skip the interpolated last point, so second last...
        spline.Add(spoints[^2]);
        return spline;
    }

    // My own method
    // Cuts lines at factor% of the center point left/right
    internal static List<PointD> GetCutCorners(List<PointD> points, float factor)
    {
        var newList = new List<PointD> { points[0] };        // First point kept as is
        for (int i = 1; i < points.Count - 1; i++)
        {
            var lp = points[i] + ((points[i - 1] + points[i]) / 2 - points[i]) * factor;
            newList.Add(lp);
            var rp = points[i] + ((points[i] + points[i + 1]) / 2 - points[i]) * factor;
            newList.Add(rp);
        }
        newList.Add(points[^1]);    // Last point kept as is

        return newList;
    }
}
