using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Inventory
{
    public enum ValuationMethod
    {
        FIFO,           // First In, First Out
        LIFO,           // Last In, First Out
        WeightedAverage, // Weighted Average Cost
        LCM             // Lower of Cost or Market
    }
}
