namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Generic interface for mapping PPDM39 entities to domain models and vice versa.
    /// </summary>
    /// <typeparam name="TPPDM39">PPDM39 entity type.</typeparam>
    /// <typeparam name="TDomain">Domain model type.</typeparam>
    public interface IPPDM39Mapper<TPPDM39, TDomain>
        where TPPDM39 : class
        where TDomain : class
    {
        /// <summary>
        /// Maps a PPDM39 entity to a domain model.
        /// </summary>
        /// <param name="ppdm39Entity">The PPDM39 entity to map.</param>
        /// <returns>The mapped domain model.</returns>
        TDomain MapToDomain(TPPDM39 ppdm39Entity);

        /// <summary>
        /// Maps a domain model to a PPDM39 entity.
        /// </summary>
        /// <param name="domainModel">The domain model to map.</param>
        /// <param name="existingPPDM39Entity">Optional existing PPDM39 entity to update.</param>
        /// <returns>The mapped PPDM39 entity.</returns>
        TPPDM39 MapToPPDM39(TDomain domainModel, TPPDM39? existingPPDM39Entity = null);

        /// <summary>
        /// Maps a collection of PPDM39 entities to domain models.
        /// </summary>
        /// <param name="ppdm39Entities">The PPDM39 entities to map.</param>
        /// <returns>The mapped domain models.</returns>
        IEnumerable<TDomain> MapToDomain(IEnumerable<TPPDM39> ppdm39Entities);

        /// <summary>
        /// Maps a collection of domain models to PPDM39 entities.
        /// </summary>
        /// <param name="domainModels">The domain models to map.</param>
        /// <returns>The mapped PPDM39 entities.</returns>
        IEnumerable<TPPDM39> MapToPPDM39(IEnumerable<TDomain> domainModels);
    }
}

