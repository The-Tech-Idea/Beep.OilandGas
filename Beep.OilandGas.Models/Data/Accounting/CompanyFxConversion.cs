using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Numerics;
using System.Globalization;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed record CompanyFxConvertedComponent(string Purpose,decimal SourceAmount,decimal BaseTargetAmount,string FractionNumerator,string FractionDenominator,int AllocatedMinorUnits,decimal TargetAmount);
/// <summary>Exact rational directed conversion; total rounding and residual allocation are explicit frozen evidence.</summary>
public sealed record CompanyFxConversion(CompanyFxRateReview Rates,int SourceCurrencyDecimals,decimal SourceTotal,decimal TargetTotal,IReadOnlyList<CompanyFxConvertedComponent> Components,string Revision)
{
    public static CompanyFxConversion Create(CompanyFxRateReview rates,int sourceDecimals,decimal total,decimal revenue,decimal charges,decimal tax)
    {
        rates.Require(rates.Books.Company,rates.Books.ActorUserId,rates.BookId,rates.SourceCurrency,rates.ReviewDate);var rate=rates.RequireApplicable();var book=rates.Books.Books.Single(row=>row.BookId==rates.BookId);if(sourceDecimals is <0 or >8 || total<=0 || revenue<=0 || charges<0 || tax<0)throw new InvalidOperationException("Exact positive source receivable/revenue and explicit source precision are required.");
        var amounts=new[]{revenue,charges,tax};var purposes=new[]{"SALES_REVENUE","SALES_CHARGES","SALES_TAX"};var sourceUnits=ExactUnits(total,sourceDecimals);if(amounts.Aggregate(BigInteger.Zero,(sum,value)=>sum+ExactUnits(value,sourceDecimals))!=sourceUnits)throw new InvalidOperationException("Original source components must exactly equal the unconverted receivable total.");
        (BigInteger Whole,BigInteger Remainder,BigInteger Divisor) Convert(decimal value){var (a,sa)=Coefficient(value);var (b,sb)=Coefficient(rate.Rate);var numerator=a*b*BigInteger.Pow(10,book.CurrencyDecimals);var divisor=BigInteger.Pow(10,sa+sb);var whole=BigInteger.DivRem(numerator,divisor,out var remainder);return(whole,remainder,divisor);}
        var target=Convert(total);var rounded=target.Whole;var comparison=(target.Remainder*2).CompareTo(target.Divisor);if(comparison>0 || comparison==0 && (book.RoundingPolicy=="AWAY_FROM_ZERO" || !rounded.IsEven))rounded++;if(rounded<=0)throw new InvalidOperationException("The explicit functional book precision rounds the positive source total to zero; no zero journal is fabricated.");
        var values=amounts.Select(Convert).ToArray();var residual=rounded-values.Aggregate(BigInteger.Zero,(sum,value)=>sum+value.Whole);if(residual<0 || residual>values.Length)throw new InvalidOperationException("The explicit residual allocation cannot balance the source conversion.");
        var priority=Enumerable.Range(0,values.Length).OrderByDescending(index=>index,Comparer<int>.Create((left,right)=>(values[left].Remainder*values[right].Divisor).CompareTo(values[right].Remainder*values[left].Divisor))).ThenBy(index=>index).ToArray();var allocations=new int[values.Length];for(var index=0;index<(int)residual;index++)allocations[priority[index]]=1;
        var components=Enumerable.Range(0,values.Length).Select(index=>new CompanyFxConvertedComponent(purposes[index],amounts[index],Money(values[index].Whole,book.CurrencyDecimals),values[index].Remainder.ToString(CultureInfo.InvariantCulture),values[index].Divisor.ToString(CultureInfo.InvariantCulture),allocations[index],Money(values[index].Whole+allocations[index],book.CurrencyDecimals))).ToArray();
        var result=new CompanyFxConversion(CompanyInventorySourceReview.Copy(rates),sourceDecimals,total,Money(rounded,book.CurrencyDecimals),components,string.Empty);result=result with{Revision=Digest(result)};return CompanyInventorySourceReview.Copy(result);
    }
    public void Require(CompanyFxRateReview rates,int sourceDecimals,decimal total,decimal revenue,decimal charges,decimal tax){var expected=Create(rates,sourceDecimals,total,revenue,charges,tax);if(JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The exact recorded rate, original money, rounded functional total or residual allocation changed.");}
    private static (BigInteger Coefficient,int Scale) Coefficient(decimal value){if(value<0)throw new InvalidOperationException("Negative source amounts are outside this positive sales journal workflow.");var bits=decimal.GetBits(value);return((BigInteger)(uint)bits[0]+((BigInteger)(uint)bits[1]<<32)+((BigInteger)(uint)bits[2]<<64),(bits[3]>>16)&255);}
    internal static BigInteger ExactUnits(decimal value,int decimals){var (coefficient,scale)=Coefficient(value);if(scale<=decimals)return coefficient*BigInteger.Pow(10,decimals-scale);var divisor=BigInteger.Pow(10,scale-decimals);if(coefficient%divisor!=0)throw new InvalidOperationException("Source money cannot be rounded silently.");return coefficient/divisor;}
    private static decimal Money(BigInteger units,int decimals){var text=units.ToString(CultureInfo.InvariantCulture);if(decimals>0){text=text.PadLeft(decimals+1,'0');text=text.Insert(text.Length-decimals,".");}if(!decimal.TryParse(text,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var value) || ExactUnits(value,decimals)!=units)throw new InvalidOperationException("The exact functional amount exceeds supported native decimal capacity.");return value;}
    private static string Digest(CompanyFxConversion value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
