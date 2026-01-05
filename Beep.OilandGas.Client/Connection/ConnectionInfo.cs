namespace Beep.OilandGas.Client.Connection
{
    /// <summary>
    /// Connection information DTO
    /// </summary>
    public class ConnectionInfo
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string? DatabaseType { get; set; }
        public string? Server { get; set; }
        public string? Database { get; set; }
        public int? Port { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }
}

