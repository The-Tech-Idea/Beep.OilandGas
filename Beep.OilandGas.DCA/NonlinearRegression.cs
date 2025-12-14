using System;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA
{
    /// <summary>
    /// Provides nonlinear regression methods for curve fitting using iterative optimization.
    /// </summary>
    public static class NonlinearRegression
    {
        /// <summary>
        /// Solves a nonlinear least-squares problem using the Gauss-Newton method.
        /// </summary>
        /// <param name="basis">Array of basis functions that depend on time and parameters.</param>
        /// <param name="y">Observed values (production rates).</param>
        /// <param name="x">Time values (DateTime array).</param>
        /// <param name="p0">Initial parameter guess.</param>
        /// <param name="maxIterations">Maximum number of iterations.</param>
        /// <param name="tolerance">Convergence tolerance.</param>
        /// <returns>Array of fitted parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input data is invalid.</exception>
        /// <exception cref="ConvergenceException">Thrown when the algorithm fails to converge.</exception>
        public static double[] Solve(
            Func<DateTime, double>[] basis, 
            double[] y, 
            DateTime[] x, 
            double[] p0, 
            int maxIterations, 
            double tolerance)
        {
            if (basis == null)
                throw new ArgumentNullException(nameof(basis));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (p0 == null)
                throw new ArgumentNullException(nameof(p0));

            if (basis.Length == 0)
                throw new Exceptions.InvalidDataException("Basis functions array cannot be empty.");
            if (y.Length == 0)
                throw new Exceptions.InvalidDataException("Observed values array cannot be empty.");
            if (x.Length != y.Length)
                throw new Exceptions.InvalidDataException(
                    $"Time array length ({x.Length}) must match observed values length ({y.Length}).");
            if (p0.Length != basis.Length)
                throw new Exceptions.InvalidDataException(
                    $"Initial guess length ({p0.Length}) must match basis functions length ({basis.Length}).");
            if (maxIterations <= 0)
                throw new Exceptions.InvalidDataException($"Maximum iterations must be positive. Provided: {maxIterations}.");
            if (tolerance <= 0)
                throw new Exceptions.InvalidDataException($"Tolerance must be positive. Provided: {tolerance}.");

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
                // Calculate the residuals and the Jacobian matrix
                for (int i = 0; i < n; i++)
                {
                    residuals[i] = y[i] - Evaluate(basis, x[i], p);
                    jacobian[i] = EvaluateJacobian(basis, x[i], p);
                }

                // Solve the linear system to obtain the parameter updates
                // Using normal equations: J^T * J * dp = J^T * r
                for (int i = 0; i < m; i++)
                {
                    double sum = 0.0;

                    for (int j = 0; j < n; j++)
                    {
                        sum += jacobian[j][i] * residuals[j];
                    }

                    dp[i] = sum;
                }

                // Update the parameters
                for (int i = 0; i < m; i++)
                {
                    p[i] += dp[i];
                }

                // Calculate the error and check for convergence
                err = dp.Max(d => Math.Abs(d));
                iter++;

                // Check for invalid parameter values
                if (p.Any(pi => double.IsNaN(pi) || double.IsInfinity(pi)))
                {
                    throw new ConvergenceException(
                        "Parameter values became invalid during optimization.",
                        iter,
                        err);
                }
            }

            if (iter == maxIterations && err > tolerance)
            {
                throw new ConvergenceException(
                    "Failed to converge within maximum iterations.",
                    iter,
                    err);
            }

            return p;
        }

        /// <summary>
        /// Evaluates the model function at a given time point with specified parameters.
        /// </summary>
        /// <param name="basis">Basis functions.</param>
        /// <param name="x">Time point.</param>
        /// <param name="p">Parameter values.</param>
        /// <returns>Evaluated function value.</returns>
        private static double Evaluate(Func<DateTime, double>[] basis, DateTime x, double[] p)
        {
            double y = 0.0;

            for (int i = 0; i < basis.Length; i++)
            {
                y += p[i] * basis[i](x);
            }

            return y;
        }

        /// <summary>
        /// Evaluates the Jacobian matrix (partial derivatives) at a given time point using finite differences.
        /// </summary>
        /// <param name="basis">Basis functions.</param>
        /// <param name="x">Time point.</param>
        /// <param name="p">Parameter values.</param>
        /// <returns>Jacobian vector (partial derivatives with respect to each parameter).</returns>
        private static double[] EvaluateJacobian(Func<DateTime, double>[] basis, DateTime x, double[] p)
        {
            double[] j = new double[p.Length];
            const double hFactor = 1E-8;

            for (int i = 0; i < p.Length; i++)
            {
                double h = Math.Max(Math.Abs(p[i]) * hFactor, Epsilon);
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
