using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Connection information model
    /// </summary>
    public class ConnectionInfo : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string ServerValue = string.Empty;

        public string Server

        {

            get { return this.ServerValue; }

            set { SetProperty(ref ServerValue, value); }

        }
        private string? DatabaseValue;

        public string? Database

        {

            get { return this.DatabaseValue; }

            set { SetProperty(ref DatabaseValue, value); }

        }
        private int? PortValue;

        public int? Port

        {

            get { return this.PortValue; }

            set { SetProperty(ref PortValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    /// <summary>
    /// Connection test request
    /// </summary>
    public class TestConnectionRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Connection test result
    /// </summary>
    public class ConnectionTestResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }

    /// <summary>
    /// Set current connection request
    /// </summary>
    public class SetCurrentConnectionRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Set current connection result
    /// </summary>
    public class SetCurrentConnectionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }

    /// <summary>
    /// Current connection response
    /// </summary>
    public class CurrentConnectionResponse : ModelEntityBase
    {
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Create connection request
    /// </summary>
    public class CreateConnectionRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }

        private string DatabaseTypeValue = string.Empty;


        [Required(ErrorMessage = "DatabaseType is required")]
        public string DatabaseType


        {


            get { return this.DatabaseTypeValue; }


            set { SetProperty(ref DatabaseTypeValue, value); }


        }

        private string ServerValue = string.Empty;


        [Required(ErrorMessage = "Server is required")]
        public string Server


        {


            get { return this.ServerValue; }


            set { SetProperty(ref ServerValue, value); }


        }

        private string? DatabaseValue;


        public string? Database


        {


            get { return this.DatabaseValue; }


            set { SetProperty(ref DatabaseValue, value); }


        }
        private int? PortValue;

        public int? Port

        {

            get { return this.PortValue; }

            set { SetProperty(ref PortValue, value); }

        }
        private string? UsernameValue;

        public string? Username

        {

            get { return this.UsernameValue; }

            set { SetProperty(ref UsernameValue, value); }

        }
        private string? PasswordValue;

        public string? Password

        {

            get { return this.PasswordValue; }

            set { SetProperty(ref PasswordValue, value); }

        }
        private bool CreateDatabaseValue;

        public bool CreateDatabase

        {

            get { return this.CreateDatabaseValue; }

            set { SetProperty(ref CreateDatabaseValue, value); }

        }
        private string? SchemaNameValue;

        public string? SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
    }

    /// <summary>
    /// Create connection result
    /// </summary>
    public class CreateConnectionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }

    /// <summary>
    /// Seed data request
    /// </summary>
    public class SeedDataRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required(ErrorMessage = "ConnectionName is required")]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }

        private List<string>? TableNamesValue;


        public List<string>? TableNames


        {


            get { return this.TableNamesValue; }


            set { SetProperty(ref TableNamesValue, value); }


        }
        private bool SkipExistingValue = true;

        public bool SkipExisting

        {

            get { return this.SkipExistingValue; }

            set { SetProperty(ref SkipExistingValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Seed data response
    /// </summary>
    public class SeedDataResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
        private int TablesSeededValue;

        public int TablesSeeded

        {

            get { return this.TablesSeededValue; }

            set { SetProperty(ref TablesSeededValue, value); }

        }
        private int RecordsInsertedValue;

        public int RecordsInserted

        {

            get { return this.RecordsInsertedValue; }

            set { SetProperty(ref RecordsInsertedValue, value); }

        }
    }

    /// <summary>
    /// Connection configuration for API requests
    /// </summary>
    public class ConnectionConfig : ModelEntityBase
    {
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string HostValue = string.Empty;

        public string Host

        {

            get { return this.HostValue; }

            set { SetProperty(ref HostValue, value); }

        }
        private int PortValue;

        public int Port

        {

            get { return this.PortValue; }

            set { SetProperty(ref PortValue, value); }

        }
        private string DatabaseValue = string.Empty;

        public string Database

        {

            get { return this.DatabaseValue; }

            set { SetProperty(ref DatabaseValue, value); }

        }
        private string? SchemaValue;

        public string? Schema

        {

            get { return this.SchemaValue; }

            set { SetProperty(ref SchemaValue, value); }

        }
        private string? UsernameValue;

        public string? Username

        {

            get { return this.UsernameValue; }

            set { SetProperty(ref UsernameValue, value); }

        }
        private string? PasswordValue;

        public string? Password

        {

            get { return this.PasswordValue; }

            set { SetProperty(ref PasswordValue, value); }

        }
        private string? ConnectionStringValue;

        public string? ConnectionString

        {

            get { return this.ConnectionStringValue; }

            set { SetProperty(ref ConnectionStringValue, value); }

        }

        /// <summary>
        /// Converts to ConnectionProperties for internal use
        /// </summary>
        public TheTechIdea.Beep.ConfigUtil.ConnectionProperties ToConnectionProperties()
        {
            var cp = new TheTechIdea.Beep.ConfigUtil.ConnectionProperties
            {
                ConnectionName = this.ConnectionName,
                Host = this.Host,
                Port = this.Port,
                Database = this.Database,
                UserID = this.Username ?? string.Empty,
                Password = this.Password ?? string.Empty,
                ConnectionString = this.ConnectionString ?? string.Empty
            };

            // Note: DatabaseType enum parsing commented out due to external dependency issues
            // Set database type based on string value if available through other means
            
            return cp;
        }
    }
}







