namespace Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation
{
    /// <summary>
    /// Types of database scripts in PPDM39
    /// </summary>
    public enum ScriptType
    {
        /// <summary>
        /// Table creation scripts (TAB.sql or TABLE_TAB.sql)
        /// </summary>
        TAB,

        /// <summary>
        /// Primary key creation scripts (PK.sql or TABLE_PK.sql)
        /// </summary>
        PK,

        /// <summary>
        /// Check constraint scripts (CK.sql)
        /// </summary>
        CK,

        /// <summary>
        /// Foreign key creation scripts (FK.sql or TABLE_FK.sql)
        /// </summary>
        FK,

        /// <summary>
        /// Original units of measure foreign keys (OUOM.sql)
        /// </summary>
        OUOM,

        /// <summary>
        /// Units of measure foreign keys (UOM.sql)
        /// </summary>
        UOM,

        /// <summary>
        /// ROW_QUALITY foreign keys (RQUAL.sql)
        /// </summary>
        RQUAL,

        /// <summary>
        /// SOURCE foreign keys (RSRC.sql)
        /// </summary>
        RSRC,

        /// <summary>
        /// Table comments (TCM.sql) - Optional
        /// </summary>
        TCM,

        /// <summary>
        /// Column comments (CCM.sql) - Optional
        /// </summary>
        CCM,

        /// <summary>
        /// Synonyms (SYN.sql) - Optional
        /// </summary>
        SYN,

        /// <summary>
        /// GUID unique constraints (GUID.sql) - Optional
        /// </summary>
        GUID,

        /// <summary>
        /// Index creation scripts (IX.sql or TABLE_IX.sql)
        /// </summary>
        IX,

        /// <summary>
        /// Other/Unknown script type
        /// </summary>
        Other
    }
}









