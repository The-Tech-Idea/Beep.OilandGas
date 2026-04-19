using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CrudeOilClassification : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the crude oil type.
        /// </summary>
        private CrudeOilType TypeValue;

        public CrudeOilType Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the API gravity range.
        /// </summary>
        public (decimal Min, decimal Max) ApiGravityRange { get; set; }

        /// <summary>
        /// Gets or sets the typical sulfur content range.
        /// </summary>
        public (decimal Min, decimal Max)? SulfurContentRange { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }

        /// <summary>
        /// Gets standard classifications.
        /// </summary>
        public static CrudeOilClassification[] GetStandardClassifications()
        {
            return new[]
            {
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Condensate,
                    ApiGravityRange = (45m, 100m),
                    Description = "Very light, high API gravity, typically low sulfur"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Light,
                    ApiGravityRange = (31.1m, 45m),
                    Description = "Light crude, easy to refine, typically sweet"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Medium,
                    ApiGravityRange = (22.3m, 31.1m),
                    Description = "Medium crude, moderate refining complexity"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Heavy,
                    ApiGravityRange = (10m, 22.3m),
                    Description = "Heavy crude, requires specialized refining"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.ExtraHeavy,
                    ApiGravityRange = (0m, 10m),
                    Description = "Extra heavy crude, bitumen-like, very difficult to refine"
                }
            };
        }
    }
}
