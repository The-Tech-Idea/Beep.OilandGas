using System;
using System.Xml.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Implementations;
using Beep.OilandGas.Drawing.DataLoaders;

namespace Beep.OilandGas.Drawing.DataLoaders
{
    /// <summary>
    /// Factory for creating data loaders.
    /// </summary>
    public static class DataLoaderFactory
    {
        /// <summary>
        /// Creates a log loader based on the data source type.
        /// </summary>
        /// <param name="dataSource">The data source (file path, connection string, etc.).</param>
        /// <param name="loaderType">The type of loader to create.</param>
        /// <param name="connectionFactory">Optional connection factory for database loaders.</param>
        /// <returns>A log loader instance.</returns>
        public static ILogLoader CreateLogLoader(
            string dataSource, 
            DataLoaderType loaderType,
            Func<System.Data.Common.DbConnection> connectionFactory = null)
        {
            // Every member is named (OILGAS-CATCH-01): a source type added later is a build error here until it is given
            // a log loader or refused, rather than falling into a catch-all.
            return loaderType switch
            {
                DataLoaderType.LasFile => new LasLogLoader(dataSource),
                DataLoaderType.Database => new DatabaseLogLoader(dataSource, connectionFactory),
                DataLoaderType.Dlis => new DlisLogLoader(dataSource),
                DataLoaderType.Witsml => new WitsmlLogLoader(dataSource),
                DataLoaderType.Csv => new CsvLogLoader(dataSource),
                DataLoaderType.Ppdm38 or DataLoaderType.SeaBed or DataLoaderType.Resqml or DataLoaderType.Prodml or DataLoaderType.Excel
                    => throw new ArgumentException($"Unsupported log loader type: {loaderType}", nameof(loaderType))
            };
        }

        /// <summary>
        /// Creates a schematic loader based on the data source type.
        /// </summary>
        /// <param name="dataSource">The data source (connection string, API endpoint, etc.).</param>
        /// <param name="loaderType">The type of loader to create.</param>
        /// <param name="connectionFactory">Optional connection factory for database loaders.</param>
        /// <returns>A schematic loader instance.</returns>
        public static ISchematicLoader CreateSchematicLoader(
            string dataSource, 
            DataLoaderType loaderType,
            Func<System.Data.Common.DbConnection> connectionFactory = null)
        {
            return loaderType switch
            {
                DataLoaderType.Ppdm38 => new Ppdm38SchematicLoader(dataSource, connectionFactory),
                DataLoaderType.SeaBed => new SeaBedSchematicLoader(dataSource),
                DataLoaderType.Database => new Ppdm38SchematicLoader(dataSource, connectionFactory), // Generic database
                DataLoaderType.LasFile or DataLoaderType.Witsml or DataLoaderType.Resqml or DataLoaderType.Dlis
                    or DataLoaderType.Prodml or DataLoaderType.Excel or DataLoaderType.Csv
                    => throw new ArgumentException($"Unsupported schematic loader type: {loaderType}", nameof(loaderType))
            };
        }

        /// <summary>
        /// Creates a log loader from a file path (auto-detects format).
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A log loader instance.</returns>
        public static ILogLoader CreateLogLoaderFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var extension = System.IO.Path.GetExtension(filePath)?.ToLower();
            return extension switch
            {
                ".las" => new LasLogLoader(filePath),
                ".txt" => new LasLogLoader(filePath), // Some LAS files use .txt
                ".dlis" => new DlisLogLoader(filePath),
                ".lis" => new DlisLogLoader(filePath), // Alternative DLIS extension
                ".xml" => new WitsmlLogLoader(filePath), // WITSML is the XML log format this library reads
                ".csv" => new CsvLogLoader(filePath),
                _ => throw new ArgumentException($"Unsupported file format: {extension}")
            };
        }

/// <summary>
        /// Creates a reservoir loader based on the data source type.
        /// </summary>
        /// <param name="dataSource">The data source (file path, connection string, etc.).</param>
        /// <param name="loaderType">The type of loader to create.</param>
        /// <returns>A reservoir loader instance.</returns>
        public static IReservoirLoader CreateReservoirLoader(string dataSource, DataLoaderType loaderType)
        {
            return loaderType switch
            {
                DataLoaderType.Resqml => new ResqmlReservoirLoader(dataSource),
                DataLoaderType.Ppdm38 => new ResqmlReservoirLoader(dataSource), // PPDM38 uses RESQML-compatible loader
                DataLoaderType.LasFile or DataLoaderType.Database or DataLoaderType.SeaBed or DataLoaderType.Witsml
                    or DataLoaderType.Dlis or DataLoaderType.Prodml or DataLoaderType.Excel or DataLoaderType.Csv
                    => throw new ArgumentException($"Unsupported reservoir loader type: {loaderType}", nameof(loaderType))
            };
        }

        /// <summary>
        /// Creates a reservoir loader from a file path (auto-detects format).
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A reservoir loader instance.</returns>
        public static IReservoirLoader CreateReservoirLoaderFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var extension = System.IO.Path.GetExtension(filePath)?.ToLower();
            return extension switch
            {
                ".epc" => new ResqmlReservoirLoader(filePath), // RESQML EPC file
                ".xml" => new ResqmlReservoirLoader(filePath), // RESQML XML file
                _ => throw new ArgumentException($"Unsupported reservoir file format: {extension}")
            };
        }
    }

        /// <summary>
        /// Types of data loaders.
        /// </summary>
        public enum DataLoaderType
        {
            /// <summary>
            /// LAS file format.
            /// </summary>
            LasFile,

            /// <summary>
            /// Generic database.
            /// </summary>
            Database,

            /// <summary>
            /// PPDM38 database.
            /// </summary>
            Ppdm38,

            /// <summary>
            /// SeaBed system.
            /// </summary>
            SeaBed,

            /// <summary>
            /// WITSML format.
            /// </summary>
            Witsml,

            /// <summary>
            /// RESQML format.
            /// </summary>
            Resqml,

            /// <summary>
            /// DLIS/RP66 binary format.
            /// </summary>
            Dlis,

            /// <summary>
            /// PRODML format.
            /// </summary>
            Prodml,

            /// <summary>
            /// Excel file.
            /// </summary>
            Excel,

            /// <summary>
            /// CSV file.
            /// </summary>
            Csv
        }
}

