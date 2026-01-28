using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ListDemoDatabasesResponse : ModelEntityBase
    {
        private List<DemoDatabaseMetadata> DatabasesValue = new();

        public List<DemoDatabaseMetadata> Databases

        {

            get { return this.DatabasesValue; }

            set { SetProperty(ref DatabasesValue, value); }

        }
        private int TotalCountValue;

        public int TotalCount

        {

            get { return this.TotalCountValue; }

            set { SetProperty(ref TotalCountValue, value); }

        }
        private int ExpiredCountValue;

        public int ExpiredCount

        {

            get { return this.ExpiredCountValue; }

            set { SetProperty(ref ExpiredCountValue, value); }

        }
    }
}
