namespace Beep.OilandGas.DataManager.Core.Exceptions
{
    /// <summary>
    /// Base exception for DataManager operations
    /// </summary>
    public class DataManagerException : Exception
    {
        public DataManagerException() : base()
        {
        }

        public DataManagerException(string message) : base(message)
        {
        }

        public DataManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
