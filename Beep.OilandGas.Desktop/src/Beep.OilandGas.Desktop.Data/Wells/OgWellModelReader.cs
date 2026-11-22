using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>
/// Builds the engine's <see cref="WellModel"/> from the PPDM and LifeCycle tables, once, for every feature that needs one (the map, the
/// 3D scene and the print packs).
/// </summary>
/// <remarks>
/// The header and its wellhead position, the depth datum and its elevation, the survey and the formation picks - read through the
/// entity models and joined in memory. The wellhead coordinates are told to the model as EPSG:4326 explicitly, because the mapping engine
/// skips a well whose CRS it cannot resolve rather than guessing one, and a wrong datum can move a well hundreds of metres.
/// </remarks>
public static class OgWellModelReader
{
    /// <summary>The CRS the PPDM wellhead coordinates are recorded in.</summary>
    public const string WellheadCrs = "EPSG:4326";

    /// <summary>
    /// The wells in scope, as engine models: an active well, optionally of one field, up to <paramref name="maxWells"/> of them, ordered
    /// by UWI so two runs of the same scope build the same list.
    /// </summary>
    public static async Task<List<WellModel>> ReadAsync(
        IDataSource source,
        string? field,
        int maxWells,
        IProgress<(int done, int total)>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var wellbores = await OgUow.ReadAllAsync<WELLBORE>(source).ConfigureAwait(false);
        var stations = await OgUow.ReadAllAsync<WELL_DIR_SRVY_STATION>(source).ConfigureAwait(false);
        var picks = await OgUow.ReadAllAsync<STRAT_WELL_SECTION>(source).ConfigureAwait(false);
        var units = await OgUow.ReadAllAsync<STRAT_UNIT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var scope = wells
            .Where(well => !string.Equals(well.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(well => string.IsNullOrWhiteSpace(field) ||
                           string.Equals(well.ASSIGNED_FIELD?.Trim(), field.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(well => well.UWI, StringComparer.Ordinal)
            .Take(maxWells > 0 ? maxWells : int.MaxValue)
            .ToList();

        var unitNames = units
            .GroupBy(unit => (unit.STRAT_UNIT_ID ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => string.IsNullOrWhiteSpace(group.First().LONG_NAME) ? group.Key : group.First().LONG_NAME!.Trim(),
                StringComparer.OrdinalIgnoreCase);

        var models = new List<WellModel>();
        var done = 0;

        foreach (var well in scope)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var uwi = (well.UWI ?? string.Empty).Trim();
            var bore = wellbores.FirstOrDefault(row => string.Equals(row.UWI?.Trim(), uwi, StringComparison.OrdinalIgnoreCase));

            var model = new WellModel();
            model.Header.Name = string.IsNullOrWhiteSpace(well.WELL_NAME) ? uwi : well.WELL_NAME!.Trim();
            model.Header.Uwi = uwi;
            model.Header.Field = well.ASSIGNED_FIELD;
            model.Header.Crs = WellheadCrs;

            if (bore?.SURFACE_LATITUDE is { } latitude && bore.SURFACE_LONGITUDE is { } longitude &&
                (double)latitude != 0d && (double)longitude != 0d)
            {
                model.Header.Latitude = (double)latitude;
                model.Header.Longitude = (double)longitude;
            }

            var datum = string.IsNullOrWhiteSpace(well.DEPTH_DATUM) ? "MSL" : well.DEPTH_DATUM!.Trim();
            model.Header.DepthDatum = datum;

            if ((double)well.DEPTH_DATUM_ELEV != 0d)
            {
                model.Header.Datums.Add(new DatumElevation
                {
                    Code = datum,
                    Reference = well.ELEV_REF_DATUM ?? datum,
                    Elevation = new Quantity((double)well.DEPTH_DATUM_ELEV, UnitOf(well.DEPTH_DATUM_ELEV_OUOM)),
                });
            }

            foreach (var station in stations
                         .Where(row => string.Equals(row.UWI?.Trim(), uwi, StringComparison.OrdinalIgnoreCase))
                         .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(row => row.STATION_MD))
            {
                model.Survey.Add(new SurveyStation
                {
                    Md = new Quantity((double)station.STATION_MD, UnitOf(station.STATION_MD_OUOM)),
                    Inclination = (double)station.INCLINATION,
                    Azimuth = (double)station.AZIMUTH,
                });
            }

            foreach (var pick in picks
                         .Where(row => string.Equals(row.UWI?.Trim(), uwi, StringComparison.OrdinalIgnoreCase))
                         .OrderBy(row => row.PICK_DEPTH))
            {
                var id = (pick.STRAT_UNIT_ID ?? string.Empty).Trim();
                var name = unitNames.TryGetValue(id, out var longName) ? longName : id;

                model.Tops.Add(new FormationTop
                {
                    Name = name,
                    UnitId = id,
                    Depth = new Quantity((double)pick.PICK_DEPTH, UnitOf(pick.PICK_DEPTH_OUOM)),
                    Interpreter = pick.INTERPRETER,
                });
            }

            models.Add(model);
            done++;

            progress?.Report((done, scope.Count));
        }

        return models;
    }

    private static string UnitOf(string? uom) =>
        string.IsNullOrWhiteSpace(uom) ? WellboreEnvironment.Current.Units.DepthUnit : uom.Trim();
}
