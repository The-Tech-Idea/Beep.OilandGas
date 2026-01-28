using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ScriptInfo : ModelEntityBase
    {
        private string FileNameValue = string.Empty;

        public string FileName

        {

            get { return this.FileNameValue; }

            set { SetProperty(ref FileNameValue, value); }

        }
        private string FullPathValue = string.Empty;

        public string FullPath

        {

            get { return this.FullPathValue; }

            set { SetProperty(ref FullPathValue, value); }

        }
        private string RelativePathValue = string.Empty;

        public string RelativePath

        {

            get { return this.RelativePathValue; }

            set { SetProperty(ref RelativePathValue, value); }

        }
        private string ScriptTypeValue = string.Empty;

        public string ScriptType

        {

            get { return this.ScriptTypeValue; }

            set { SetProperty(ref ScriptTypeValue, value); }

        }
        private string? TableNameValue;

        public string? TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string? ModuleValue;

        public string? Module

        {

            get { return this.ModuleValue; }

            set { SetProperty(ref ModuleValue, value); }

        }
        private string? SubjectAreaValue;

        public string? SubjectArea

        {

            get { return this.SubjectAreaValue; }

            set { SetProperty(ref SubjectAreaValue, value); }

        }
        private bool IsConsolidatedValue;

        public bool IsConsolidated

        {

            get { return this.IsConsolidatedValue; }

            set { SetProperty(ref IsConsolidatedValue, value); }

        }
        private bool IsMandatoryValue;

        public bool IsMandatory

        {

            get { return this.IsMandatoryValue; }

            set { SetProperty(ref IsMandatoryValue, value); }

        }
        private bool IsOptionalValue;

        public bool IsOptional

        {

            get { return this.IsOptionalValue; }

            set { SetProperty(ref IsOptionalValue, value); }

        }
        private long FileSizeValue;

        public long FileSize

        {

            get { return this.FileSizeValue; }

            set { SetProperty(ref FileSizeValue, value); }

        }
        private DateTime LastModifiedValue;

        public DateTime LastModified

        {

            get { return this.LastModifiedValue; }

            set { SetProperty(ref LastModifiedValue, value); }

        }
        private int ExecutionOrderValue;

        public int ExecutionOrder

        {

            get { return this.ExecutionOrderValue; }

            set { SetProperty(ref ExecutionOrderValue, value); }

        }
        private List<string> DependenciesValue = new List<string>();

        public List<string> Dependencies

        {

            get { return this.DependenciesValue; }

            set { SetProperty(ref DependenciesValue, value); }

        }
        private string? CategoryValue;

        public string? Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private object NameValue;

        public object Name

        {

            get { return this.NameValue; }

            set { SetProperty(ref NameValue, value); }

        }
    }
}
