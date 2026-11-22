using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// The section facts an API 5C3 rating needs that the string outline does not carry (ENG-02).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The owner records a string's outside diameter, nominal weight, grade and set depth, and its own comment says the burst,
    /// collapse and tension arithmetic belongs to ENG-02. Two of the numbers that arithmetic needs were missing from it: the
    /// <b>wall thickness</b> (the rating is a function of OD and wall) and the <b>top of the string</b> (the tension load is the
    /// buoyed weight of the length between the top and the shoe).
    /// </para>
    /// <para>
    /// Both are additive and nullable, so a DEV-03 outline row stays valid and simply records neither. When the wall is absent
    /// the check falls back to the owner's own nominal-weight catalogue, and when neither is available the section is refused
    /// rather than rated on a guessed wall.
    /// </para>
    /// </remarks>
    public partial class DEVELOPMENT_CASING_STRING
    {
        /// <summary>
        /// The wall thickness in inches, the unit the API ratings are computed in. Null when only the nominal weight is known.
        /// </summary>
        public decimal? WALL_THICKNESS
        {
            get => WallThicknessValue;
            set => SetProperty(ref WallThicknessValue, value);
        }

        private decimal? WallThicknessValue;

        /// <summary>
        /// The top of the string, in <see cref="DEPTH_OUOM"/>. Null means the string hangs from surface, which the check states
        /// rather than assuming silently.
        /// </summary>
        public decimal? TOP_DEPTH
        {
            get => TopDepthValue;
            set => SetProperty(ref TopDepthValue, value);
        }

        private decimal? TopDepthValue;
    }
}
