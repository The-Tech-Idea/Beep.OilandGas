using System;

namespace Beep.OilandGas.DCA.Exceptions
{
    /// <summary>
    /// Exception thrown when a numerical algorithm fails to converge.
    /// </summary>
    public class ConvergenceException : DCAException
    {
        /// <summary>
        /// Gets the number of iterations performed before convergence failure.
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Gets the final error value before convergence failure.
        /// </summary>
        public double FinalError { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvergenceException"/> class.
        /// </summary>
        public ConvergenceException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvergenceException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ConvergenceException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvergenceException"/> class with convergence details.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="iterations">The number of iterations performed.</param>
        /// <param name="finalError">The final error value.</param>
        public ConvergenceException(string message, int iterations, double finalError) 
            : base($"{message} (Iterations: {iterations}, Final Error: {finalError:E})")
        {
            Iterations = iterations;
            FinalError = finalError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvergenceException"/> class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConvergenceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

