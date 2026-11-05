namespace TheTechIdea.Data.OilGas;

/// <summary>Presence checks for one visible WELL record; this is not an overall quality score.</summary>
public sealed record CompanyWellQualityReview(CompanyScope Company, string WellId, string Status, CompanyWellQualityField[] Fields);
public sealed record CompanyWellQualityField(string Name, bool Present);
public sealed record CompanyWellQualityBatchRequest(string[] WellIds);
/// <summary>Individual observations for the explicitly requested wells, not a table-wide assessment.</summary>
public sealed record CompanyWellQualityBatchReview(CompanyScope Company, CompanyWellQualityReview[] Wells);
/// <summary>A bounded visible selection, not the full well population.</summary>
public sealed record CompanyWellQualitySample(CompanyScope Company, string[] WellIds);
