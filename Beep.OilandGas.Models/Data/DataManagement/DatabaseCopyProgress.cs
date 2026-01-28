using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class DatabaseCopyProgress : ProgressUpdate
    {
        private string SourceConnectionValue = string.Empty;

        public string SourceConnection

        {

            get { return this.SourceConnectionValue; }

            set { SetProperty(ref SourceConnectionValue, value); }

        }
        private string TargetConnectionValue = string.Empty;

        public string TargetConnection

        {

            get { return this.TargetConnectionValue; }

            set { SetProperty(ref TargetConnectionValue, value); }

        }
        private string CurrentTableValue = string.Empty;

        public string CurrentTable

        {

            get { return this.CurrentTableValue; }

            set { SetProperty(ref CurrentTableValue, value); }

        }
        private int TablesCopiedValue;

        public int TablesCopied

        {

            get { return this.TablesCopiedValue; }

            set { SetProperty(ref TablesCopiedValue, value); }

        }
        private int TotalTablesValue;

        public int TotalTables

        {

            get { return this.TotalTablesValue; }

            set { SetProperty(ref TotalTablesValue, value); }

        }
        private long RowsCopiedValue;

        public long RowsCopied

        {

            get { return this.RowsCopiedValue; }

            set { SetProperty(ref RowsCopiedValue, value); }

        }
        private long TotalRowsValue;

        public long TotalRows

        {

            get { return this.TotalRowsValue; }

            set { SetProperty(ref TotalRowsValue, value); }

        }
        private TimeSpan? ElapsedTimeValue;

        public TimeSpan? ElapsedTime

        {

            get { return this.ElapsedTimeValue; }

            set { SetProperty(ref ElapsedTimeValue, value); }

        }
        private TimeSpan? EstimatedTimeRemainingValue;

        public TimeSpan? EstimatedTimeRemaining

        {

            get { return this.EstimatedTimeRemainingValue; }

            set { SetProperty(ref EstimatedTimeRemainingValue, value); }

        }
    }
}
