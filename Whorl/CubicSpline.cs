﻿//
// Author: Ryan Seghers
//
// Copyright (C) 2013-2014 Ryan Seghers
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the irrevocable, perpetual, worldwide, and royalty-free
// rights to use, copy, modify, merge, publish, distribute, sublicense, 
// display, perform, create derivative works from and/or sell copies of 
// the Software, both in source and object code form, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Whorl
{
	/// <summary>
	/// Cubic spline interpolation.
	/// Call Fit (or use the corrector constructor) to compute spline coefficients, then Eval to evaluate the spline at other X coordinates.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is implemented based on the wikipedia article:
	/// http://en.wikipedia.org/wiki/Spline_interpolation
	/// I'm not sure I have the right to include a copy of the article so the equation numbers referenced in 
	/// comments will end up being wrong at some point.
	/// </para>
	/// <para>
	/// This is not optimized, and is not MT safe.
	/// This can extrapolate off the ends of the splines.
	/// You must provide points in X sort order.
	/// </para>
	/// </remarks>
	public class CubicSpline
	{
		#region Fields

		// N-1 spline coefficients for N points
		private float[] a;
		private float[] b;

		// Save the original x and y for Eval
		private float[] xOrig;
		private float[] yOrig;

		#endregion

		#region Ctor

		/// <summary>
		/// Default ctor.
		/// </summary>
		public CubicSpline()
		{
		}

		/// <summary>
		/// Construct and call Fit.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		public CubicSpline(float[] x, float[] y, float startSlope = float.NaN, float endSlope = float.NaN, bool debug = false)
		{
			Fit(x, y, startSlope, endSlope, debug);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Throws if Fit has not been called.
		/// </summary>
		private void CheckAlreadyFitted()
		{
			if (a == null) throw new Exception("Fit must be called before you can evaluate.");
		}

		private int _lastIndex = 0;

		/// <summary>
		/// Find where in xOrig the specified x falls, by simultaneous traverse.
		/// This allows xs to be less than x[0] and/or greater than x[n-1]. So allows extrapolation.
		/// This keeps state, so requires that x be sorted and xs called in ascending order, and is not multi-thread safe.
		/// </summary>
		private int GetNextXIndex(float x)
		{
			if (x < xOrig[_lastIndex])
			{
				throw new ArgumentException("The X values to evaluate must be sorted.");
			}

			while ((_lastIndex < xOrig.Length - 2) && (x > xOrig[_lastIndex + 1]))
			{
				_lastIndex++;
			}

			return _lastIndex;
		}

		/// <summary>
		/// Evaluate the specified x value using the specified spline.
		/// </summary>
		/// <param name="x">The x value.</param>
		/// <param name="j">Which spline to use.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The y value.</returns>
		private float EvalSpline(float x, int j, bool debug = false)
		{
			float dx = xOrig[j + 1] - xOrig[j];
			float t = (x - xOrig[j]) / dx;
			float y = (1 - t) * yOrig[j] + t * yOrig[j + 1] + t * (1 - t) * (a[j] * (1 - t) + b[j] * t); // equation 9
			if (debug) Console.WriteLine("xs = {0}, j = {1}, t = {2}", x, j, t);
			return y;
		}

		#endregion

		#region Fit*

		/// <summary>
		/// Fit x,y and then eval at points xs and return the corresponding y's.
		/// This does the "natural spline" style for ends.
		/// This can extrapolate off the ends of the splines.
		/// You must provide points in X sort order.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="xs">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each xs.</returns>
		public float[] FitAndEval(float[] x, float[] y, float[] xs, float startSlope = float.NaN, float endSlope = float.NaN, bool debug = false)
		{
			Fit(x, y, startSlope, endSlope, debug);
			return Eval(xs, debug);
		}

		/// <summary>
		/// Compute spline coefficients for the specified x,y points.
		/// This does the "natural spline" style for ends.
		/// This can extrapolate off the ends of the splines.
		/// You must provide points in X sort order.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		public void Fit(float[] x, float[] y, float startSlope = float.NaN, float endSlope = float.NaN, bool debug = false)
		{
			if (Single.IsInfinity(startSlope) || Single.IsInfinity(endSlope))
			{
				throw new Exception("startSlope and endSlope cannot be infinity.");
			}

			// Save x and y for eval
			this.xOrig = x;
			this.yOrig = y;

			int n = x.Length;
			float[] r = new float[n]; // the right hand side numbers: wikipedia page overloads b

			TriDiagonalMatrixF m = new TriDiagonalMatrixF(n);
			float dx1, dx2, dy1, dy2;

			// First row is different (equation 16 from the article)
			if (float.IsNaN(startSlope))
			{
				dx1 = x[1] - x[0];
				m.C[0] = 1.0f / dx1;
				m.B[0] = 2.0f * m.C[0];
				r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);
			}
			else
			{
				m.B[0] = 1;
				r[0] = startSlope;
			}

			// Body rows (equation 15 from the article)
			for (int i = 1; i < n - 1; i++)
			{
				dx1 = x[i] - x[i - 1];
				dx2 = x[i + 1] - x[i];

				m.A[i] = 1.0f / dx1;
				m.C[i] = 1.0f / dx2;
				m.B[i] = 2.0f * (m.A[i] + m.C[i]);

				dy1 = y[i] - y[i - 1];
				dy2 = y[i + 1] - y[i];
				r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
			}

			// Last row also different (equation 17 from the article)
			if (float.IsNaN(endSlope))
			{
				dx1 = x[n - 1] - x[n - 2];
				dy1 = y[n - 1] - y[n - 2];
				m.A[n - 1] = 1.0f / dx1;
				m.B[n - 1] = 2.0f * m.A[n - 1];
				r[n - 1] = 3 * (dy1 / (dx1 * dx1));
			}
			else
			{
				m.B[n - 1] = 1;
				r[n - 1] = endSlope;
			}

			if (debug) Console.WriteLine("Tri-diagonal matrix:\n{0}", m.ToDisplayString(":0.0000", "  "));
			//if (debug) Console.WriteLine("r: {0}", ArrayUtil.ToString<float>(r));

			// k is the solution to the matrix
			float[] k = m.Solve(r);
			//if (debug) Console.WriteLine("k = {0}", ArrayUtil.ToString<float>(k));

			// a and b are each spline's coefficients
			this.a = new float[n - 1];
			this.b = new float[n - 1];

			for (int i = 1; i < n; i++)
			{
				dx1 = x[i] - x[i - 1];
				dy1 = y[i] - y[i - 1];
				a[i - 1] = k[i - 1] * dx1 - dy1; // equation 10 from the article
				b[i - 1] = -k[i] * dx1 + dy1; // equation 11 from the article
			}

			//if (debug) Console.WriteLine("a: {0}", ArrayUtil.ToString<float>(a));
			//if (debug) Console.WriteLine("b: {0}", ArrayUtil.ToString<float>(b));
		}

		#endregion

		#region Eval*

		/// <summary>
		/// Evaluate the spline at the specified x coordinates.
		/// This can extrapolate off the ends of the splines.
		/// You must provide X's in ascending order.
		/// The spline must already be computed before calling this, meaning you must have already called Fit() or FitAndEval().
		/// </summary>
		/// <param name="x">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each x.</returns>
		public float[] Eval(float[] x, bool debug = false)
		{
			CheckAlreadyFitted();

			int n = x.Length;
			float[] y = new float[n];
			_lastIndex = 0; // Reset simultaneous traversal in case there are multiple calls

			for (int i = 0; i < n; i++)
			{
				// Find which spline can be used to compute this x (by simultaneous traverse)
				int j = GetNextXIndex(x[i]);

				// Evaluate using j'th spline
				y[i] = EvalSpline(x[i], j, debug);
			}

			return y;
		}

		/// <summary>
		/// Evaluate (compute) the slope of the spline at the specified x coordinates.
		/// This can extrapolate off the ends of the splines.
		/// You must provide X's in ascending order.
		/// The spline must already be computed before calling this, meaning you must have already called Fit() or FitAndEval().
		/// </summary>
		/// <param name="x">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each x.</returns>
		public float[] EvalSlope(float[] x, bool debug = false)
		{
			CheckAlreadyFitted();

			int n = x.Length;
			float[] qPrime = new float[n];
			_lastIndex = 0; // Reset simultaneous traversal in case there are multiple calls

			for (int i = 0; i < n; i++)
			{
				// Find which spline can be used to compute this x (by simultaneous traverse)
				int j = GetNextXIndex(x[i]);

				// Evaluate using j'th spline
				float dx = xOrig[j + 1] - xOrig[j];
				float dy = yOrig[j + 1] - yOrig[j];
				float t = (x[i] - xOrig[j]) / dx;

				// From equation 5 we could also compute q' (qp) which is the slope at this x
				qPrime[i] = dy / dx
					+ (1 - 2 * t) * (a[j] * (1 - t) + b[j] * t) / dx
					+ t * (1 - t) * (b[j] - a[j]) / dx;

				if (debug) Console.WriteLine("[{0}]: xs = {1}, j = {2}, t = {3}", i, x[i], j, t);
			}

			return qPrime;
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Static all-in-one method to fit the splines and evaluate at X coordinates.
		/// </summary>
		/// <param name="x">Input. X coordinates to fit.</param>
		/// <param name="y">Input. Y coordinates to fit.</param>
		/// <param name="xs">Input. X coordinates to evaluate the fitted curve at.</param>
		/// <param name="startSlope">Optional slope constraint for the first point. Single.NaN means no constraint.</param>
		/// <param name="endSlope">Optional slope constraint for the final point. Single.NaN means no constraint.</param>
		/// <param name="debug">Turn on console output. Default is false.</param>
		/// <returns>The computed y values for each xs.</returns>
		public static float[] Compute(float[] x, float[] y, float[] xs, float startSlope = float.NaN, float endSlope = float.NaN, bool debug = false)
		{
			CubicSpline spline = new CubicSpline();
			return spline.FitAndEval(x, y, xs, startSlope, endSlope, debug);
		}

        /// <summary>
        /// Fit the input x,y points using the parametric approach, so that y does not have to be an explicit
        /// function of x, meaning there does not need to be a single value of y for each x.
        /// </summary>
        /// <param name="x">Input x coordinates.</param>
        /// <param name="y">Input y coordinates.</param>
        /// <param name="nOutputPoints">How many output points to create.</param>
        /// <param name="xs">Output (interpolated) x values.</param>
        /// <param name="ys">Output (interpolated) y values.</param>
        /// <param name="firstDx">Optionally specifies the first point's slope in combination with firstDy. Together they
        /// are a vector describing the direction of the parametric spline of the starting point. The vector does
        /// not need to be normalized. If either is NaN then neither is used.</param>
        /// <param name="firstDy">See description of dx0.</param>
        /// <param name="lastDx">Optionally specifies the last point's slope in combination with lastDy. Together they
        /// are a vector describing the direction of the parametric spline of the last point. The vector does
        /// not need to be normalized. If either is NaN then neither is used.</param>
        /// <param name="lastDy">See description of dxN.</param>
        public static void FitParametric(float[] x, float[] y, int nOutputPoints, out float[] xs, out float[] ys,
            float firstDx = Single.NaN, float firstDy = Single.NaN, float lastDx = Single.NaN, float lastDy = Single.NaN)
		{
			// Compute distances
			int n = x.Length;
			float[] dists = new float[n]; // cumulative distance
			dists[0] = 0;
			float totalDist = 0;

			for (int i = 1; i < n; i++)
			{
				float dx = x[i] - x[i - 1];
				float dy = y[i] - y[i - 1];
				float dist = (float)Math.Sqrt(dx * dx + dy * dy);
				totalDist += dist;
				dists[i] = totalDist;
			}

			// Create 'times' to interpolate to
			float dt = totalDist / (nOutputPoints - 1);
			float[] times = new float[nOutputPoints];
			times[0] = 0;

			for (int i = 1; i < nOutputPoints; i++)
			{
				times[i] = times[i - 1] + dt;
			}

            // Normalize the slopes, if specified
            NormalizeVector(ref firstDx, ref firstDy);
            NormalizeVector(ref lastDx, ref lastDy);

			// Spline fit both x and y to times
			CubicSpline xSpline = new CubicSpline();
			xs = xSpline.FitAndEval(dists, x, times, firstDx / dt, lastDx / dt);

			CubicSpline ySpline = new CubicSpline();
			ys = ySpline.FitAndEval(dists, y, times, firstDy / dt, lastDy / dt);
		}

        /// <summary>
        /// Added by Chris Jay. FitParametric version working with PointF arrays.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="nOutputPoints"></param>
        /// <returns></returns>
        public static PointF[] FitParametric(IEnumerable<PointF> points, int nOutputPoints)
        {
            float[] xS, yS;
            FitParametric(points.Select(p => p.X).ToArray(), points.Select(p => p.Y).ToArray(),
                          nOutputPoints, out xS, out yS);
            PointF[] pSpline = new PointF[xS.Length];
            for (int i = 0; i < xS.Length; i++)
                pSpline[i] = new PointF(xS[i], yS[i]);
            return pSpline;
        }

        /// <summary>
        /// Added by Chris Jay. FitParametric version working with PointF arrays.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="nOutputPoints"></param>
        /// <returns></returns>
        public static PointF[] FitParametricClosed(IEnumerable<PointF> points, 
                                                   int nOutputPoints)
        {
            float[] xS, yS;
            PointF[] aPoints = points.ToArray();
            int n = aPoints.Length;
            if (n < 3)
                throw new Exception("FitParametricClosed must have at least 3 points.");
            PointF diffStart = new PointF(aPoints[1].X - aPoints[0].X, 
                                          aPoints[1].Y - aPoints[0].Y);
            PointF diffEnd = new PointF(aPoints[n - 1].X - aPoints[n - 2].X,
                                        aPoints[n - 1].Y - aPoints[n - 2].Y);
            PointF dAvg = new PointF(diffStart.X + diffEnd.X / 2F, 
                                     diffStart.Y + diffEnd.Y / 2F);
            FitParametric(points.Select(p => p.X).ToArray(), points.Select(p => p.Y).ToArray(),
                          nOutputPoints, out xS, out yS, dAvg.X, dAvg.Y, dAvg.X, dAvg.Y);
            PointF[] pSpline = new PointF[xS.Length];
            for (int i = 0; i < xS.Length; i++)
                pSpline[i] = new PointF(xS[i], yS[i]);
            return pSpline;
        }

		public static List<PointF> FitParametricClosed(List<PointF> points, List<int> cornerIndices)
		{
			if (cornerIndices.Count == 0)
            {
				return FitParametricClosed(points, (int)Tools.PathLength(points)).ToList();
            }
			int ind1 = 0;
			var outputPoints = new List<PointF>();
			bool smoothJoin = cornerIndices.First() != 0 && cornerIndices.Last() != points.Count - 1;
			PointF dAvg;
			if (smoothJoin)
            {
				int n = points.Count;
				PointF diffStart = new PointF(points[1].X - points[0].X,
										      points[1].Y - points[0].Y);
				PointF diffEnd = new PointF(points[n - 1].X - points[n - 2].X,
											points[n - 1].Y - points[n - 2].Y);
				dAvg = new PointF(diffStart.X + diffEnd.X / 2F,
								  diffStart.Y + diffEnd.Y / 2F);
			}
            else
				dAvg = PointF.Empty;
			for (int i = 0; i <= cornerIndices.Count; i++)
            {
				int ind2 = i < cornerIndices.Count ? cornerIndices[i] : points.Count - 1;
				if (ind2 == 0)
					continue;
				PointF[] aPoints = points.Skip(ind1).Take(ind2 - ind1 + 1).ToArray();
				float[] xS, yS;
				int n = aPoints.Length;
				if (n <= 2)
                {
					outputPoints.AddRange(aPoints);
				}
                else
                {
					int nOutputPoints = (int)Tools.PathLength(aPoints);
					if (nOutputPoints == 0)
						continue;
					PointF d1 = new PointF(float.NaN, float.NaN);
					PointF d2 = new PointF(float.NaN, float.NaN);
					if (smoothJoin)
                    {
						if (ind1 == 0)
							d1 = dAvg;
						if (ind2 == points.Count - 1)
							d2 = dAvg;
                    }
					FitParametric(aPoints.Select(p => p.X).ToArray(), aPoints.Select(p => p.Y).ToArray(),
							      nOutputPoints, out xS, out yS, d1.X, d1.Y, d2.X, d2.Y);
					for (int j = 0; j < xS.Length; j++)
						outputPoints.Add(new PointF(xS[j], yS[j]));
				}
				ind1 = ind2;
			}
			return outputPoints;
		}

		private static void NormalizeVector(ref float dx, ref float dy)
        {
            if (!Single.IsNaN(dx) && !Single.IsNaN(dy))
            {
                float d = (float)Math.Sqrt(dx * dx + dy * dy);

                if (d > Single.Epsilon) // probably not conservative enough, but catches the (0,0) case at least
                {
                    dx = dx / d;
                    dy = dy / d;
                }
                else
                {
                    throw new ArgumentException("The input vector is too small to be normalized.");
                }
            }
            else
            {
                // In case one is NaN and not the other
                dx = dy = Single.NaN;
            }
        }

        #endregion
    }
}
