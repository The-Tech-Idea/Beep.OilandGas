using System;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Repositories;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    public partial class ProductionAccountingService
    {
        public string DefaultConnectionName => ConnectionName;

        public PPDMGenericRepository GetRepository(Type entityType, string? connectionName, string tableName)
        {
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                entityType,
                connectionName ?? ConnectionName,
                tableName);
        }

        public static decimal CalculateAmortization(decimal netCapitalizedCosts, decimal totalProvedReservesBoe, decimal productionBoe)
        {
            if (totalProvedReservesBoe <= 0m)
                return 0m;

            return netCapitalizedCosts * (productionBoe / totalProvedReservesBoe);
        }

        public static decimal CalculateInterestCapitalization(InterestCapitalizationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var rate = data.InterestRate != 0m ? data.InterestRate : data.WACC;
            return data.ConstructionCost * rate * Math.Max(data.Periods, 0);
        }

        public static decimal ConvertProductionToBOE(ProductionData production)
        {
            if (production == null)
                throw new ArgumentNullException(nameof(production));

            return production.OilVolume + (production.GasVolume / 6m);
        }

        public static decimal ConvertReservesToBOE(ProvedReserves reserves)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            return reserves.ProvedOilReserves + (reserves.ProvedGasReserves / 6m);
        }
    }
}