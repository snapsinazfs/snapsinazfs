// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Common.Configuration.Datasets;

namespace Sanoid.Common.Configuration.Templates;

/// <summary>
///     A template of settings to use for processing <see cref="Dataset" /> items.
/// </summary>
/// <remarks>
///     All properties except for <see cref="Name" /> are nullable. If a property is null, its value is inherited from its
///     parent <see cref="Template" /> specified in <see cref="UseTemplate" />.<br />
/// </remarks>
public class Template
{
    /// <summary>
    ///     Creates a new instance of a <see cref="Template" /> with the specified name.
    /// </summary>
    /// <param name="templateName">The name, as configured in Sanoid.json, to assign to the <see cref="Name" /> property.</param>
    /// <param name="useTemplateName">The name of a template to use as a parent</param>
    public Template( string templateName, string useTemplateName )
    {
        if ( string.IsNullOrWhiteSpace( templateName ) )
        {
            throw new ArgumentNullException( nameof( templateName ), "All templates MUST have a non-null, non-whitespace, non-empty name" );
        }

        Name = templateName;
        UseTemplateName = useTemplateName;
    }

    private bool? _autoPrune;

    private bool? _autoSnapshot;

    private bool? _recursive;
    private bool? _skipChildren;
    private SnapshotRetention? _snapshotRetention;
    private SnapshotTiming? _snapshotTiming;

    /// <summary>
    ///     Gets or sets whether expired snapshots will be pruned for this template.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether expired snapshots will be pruned for this snapshot.
    /// </value>
    public bool? AutoPrune
    {
        get => _autoPrune ?? UseTemplate?.AutoPrune;
        set => _autoPrune = value;
    }

    /// <summary>
    ///     Gets or sets whether snapshots will be taken for this template.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether snapshots will be taken for this snapshot.
    /// </value>
    public bool? AutoSnapshot
    {
        get => _autoSnapshot ?? UseTemplate?.AutoSnapshot;
        set => _autoSnapshot = value;
    }

    /// <summary>
    ///     Gets or sets the name of the template.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> identifying the template, corresponding to a Template item in Sanoid.json.
    /// </value>
    /// <remarks>
    ///     The template MUST be defined in Sanoid.json for initial configuration parsing or an exception will be thrown.
    /// </remarks>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets whether recursive processing will be used for this template and its descendents.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether recursive processing will be used for this template and its
    ///     descendents.
    /// </value>
    public bool? Recursive
    {
        get => _recursive ?? UseTemplate?.Recursive;
        set => _recursive = value;
    }

    /// <summary>
    ///     Gets or sets whether to skip processing of children.
    /// </summary>
    /// <value>
    ///     A <see langword="bool?" /> indicating whether child datasets will be skipped
    ///     descendents.
    /// </value>
    /// <remarks>
    ///     Can be overridden by <see cref="Dataset.TemplateOverrides" />
    /// </remarks>
    public bool? SkipChildren
    {
        get => _skipChildren ?? UseTemplate?.SkipChildren;
        set => _skipChildren = value;
    }

    /// <summary>
    ///     Gets or sets the snapshot retention policy for this <see cref="Template" />
    /// </summary>
    /// <value>
    ///     A <see cref="SnapshotRetention" /> record specifying the snapshot retention policy for this <see cref="Template" />
    /// </value>
    public SnapshotRetention? SnapshotRetention
    {
        get => _snapshotRetention ?? UseTemplate?.SnapshotRetention;
        set => _snapshotRetention = value;
    }

    /// <summary>
    ///     Gets or sets the snapshot retention policy for this <see cref="Template" />
    /// </summary>
    /// <value>
    ///     A <see cref="SnapshotRetention" /> record specifying the snapshot retention policy for this <see cref="Template" />
    /// </value>
    public SnapshotTiming? SnapshotTiming
    {
        get => _snapshotTiming ?? UseTemplate?.SnapshotTiming;
        set => _snapshotTiming = value;
    }

    /// <summary>
    ///     Gets or sets another configured template to inherit settings from.
    /// </summary>
    /// <value>
    ///     A <see cref="Template" /> from which settings are inherited.
    /// </value>
    public Template? UseTemplate { get; set; }

    internal string UseTemplateName { get; init; }
}
