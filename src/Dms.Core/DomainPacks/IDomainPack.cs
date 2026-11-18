namespace Dms.Core.DomainPacks;

/// <summary>
/// A per-collection bundle of domain knowledge. A collection selects at most one pack through
/// <see cref="Documents.Collection.DomainPack"/>; without one, the general behaviour applies.
/// </summary>
/// <remarks>
/// Members are added when the types they expose exist: chunking options, enrichers, classifier
/// thresholds and reference patterns with the processing pipeline (M2), agent instructions and tools
/// with the agent (M5). See the spec, "Szakterületi csomagok".
/// </remarks>
public interface IDomainPack
{
    /// <summary>
    /// Stable identifier stored in <c>collection.domain_pack</c>, e.g. <c>automotive</c>.
    /// Lowercase letters and digits separated by hyphens.
    /// </summary>
    string Id { get; }
}
