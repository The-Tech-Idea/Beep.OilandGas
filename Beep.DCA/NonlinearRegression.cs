using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.DCA
{
    public static class NonlinearRegression
    {
       
            public static double[] Solve(Func<DateTime, double>[] basis, double[] y, DateTime[] x, double[] p0, int maxIterations, double tolerance)
            {
                int n = y.Length;
                int m = p0.Length;
                double[][] jacobian = new double[n][];
                double[] residuals = new double[n];
                double[] p = (double[])p0.Clone();
                double[] dp = new double[m];
                double err = double.MaxValue;
                int iter = 0;

                while (iter < maxIterations && err > tolerance)
                {
                    // calculate the residuals and the Jacobian matrix
                    for (int i = 0; i < n; i++)
                    {
                        residuals[i] = y[i] - Evaluate(basis, x[i], p);
                        jacobian[i] = EvaluateJacobian(basis, x[i], p);
                    }

                    // solve the linear system to obtain the parameter updates
                    for (int i = 0; i < m; i++)
                    {
                        double sum = 0.0;

                        for (int j = 0; j < n; j++)
                        {
                            sum += jacobian[j][i] * residuals[j];
                        }

                        dp[i] = sum;
                    }

                    // update the parameters
                    for (int i = 0; i < m; i++)
                    {
                        p[i] += dp[i];
                    }
                    // calculate the error and check for convergence
                    err = dp.Max(d => Math.Abs(d));
                    iter++;
                }

                if (iter == maxIterations && err > tolerance)
                {
                    throw new InvalidOperationException("Failed to converge.");
                }

                return p;
            }

            private static double Evaluate(Func<DateTime, double>[] basis, DateTime x, double[] p)
            {
                double y = 0.0;

                for (int i = 0; i < basis.Length; i++)
                {
                    y += p[i] * basis[i](x);
                }

                return y;
            }

            private static double[] EvaluateJacobian(Func<DateTime, double>[] basis, DateTime x, double[] p)
            {
                double[] j = new double[p.Length];

                for (int i = 0; i < p.Length; i++)
                {
                    double h = p[i] * 1E-8;
                    double[] p1 = (double[])p.Clone();
                    p1[i] += h;

                    double[] p2 = (double[])p.Clone();
                    p2[i] -= h;

                    j[i] = (Evaluate(basis, x, p1) - Evaluate(basis, x, p2)) / (2.0 * h);
                }

                return j;
            }

      

    }
}
