namespace Sanoid.Common.Snapshots
{
    /// <summary>
    /// Represents a snapshot template
    /// </summary>
    public class SnapshotTemplate : SnapshotTemplateBase
    {
        /// <summary>
        /// Gets or sets a parent template to apply to this template, for default settings
        /// </summary>
        /// <value>The <see langword="string"/> <see cref="SnapshotTemplateBase.Name">Name</see> of the parent template that this template inherits from</value>
        public string? UseTemplate { get; set; }
    }
}
