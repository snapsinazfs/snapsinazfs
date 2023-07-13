// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

/// <summary>
///     Represents a node in the tree of datasets in <see cref="ZfsConfigurationWindow" />
/// </summary>
public class ZfsObjectConfigurationTreeNode : TreeNode
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private readonly ConcurrentDictionary<string, IZfsProperty> _inheritedPropertiesSinceLastSave = new( );

    private readonly ConcurrentDictionary<string, IZfsProperty> _modifiedPropertiesSinceLastSave = new( );

    /// <summary>
    ///     Creates a new instance of a <see cref="ZfsObjectConfigurationTreeNode" /> having the specified <paramref name="name" /> (used
    ///     for display text), <paramref name="baseDataset" />, and <paramref name="treeDataset" />
    /// </summary>
    /// <param name="name">The name of this tree node. Used for display text in the UI only</param>
    /// <param name="baseDataset">
    ///     A reference to the base copy of the <see cref="ZfsRecord" /> this tree node will point to
    /// </param>
    /// <param name="treeDataset">
    ///     A reference to the tree copy of the <see cref="ZfsRecord" /> this tree node will point to. This is the object used for
    ///     display.
    /// </param>
    public ZfsObjectConfigurationTreeNode( string name, ZfsRecord baseDataset, ZfsRecord treeDataset )
        : base( name )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
    }

    private List<ITreeNode>? _children;

    /// <inheritdoc />
    public override IList<ITreeNode> Children
    {
        get
        {
            List<ITreeNode> list = new( );
            foreach ( ( string? childName, ZfsRecord child ) in TreeDataset.GetSortedChildDatasets( ) )
            {
                if ( !BaseDataset.GetChild( childName, out ZfsRecord? baseDataset ) )
                {
                    Logger.Warn( "Dataset {0} not found in parent {1}", childName, BaseDataset.Name );
                    continue;
                }

                ITreeNode node = new ZfsObjectConfigurationTreeNode( childName.GetLastPathElement( ), baseDataset, child );
                list.Add( node );
            }

            return _children ??= list;
        }
    }

    /// <summary>
    ///     Gets a boolean value indicating if any properties have been changed or explicitly inherited in the UI for this specific node,
    ///     and not just due to
    ///     inheritance from a modified parent.
    /// </summary>
    public bool IsLocallyModified => !_modifiedPropertiesSinceLastSave.IsEmpty || !_inheritedPropertiesSinceLastSave.IsEmpty;

    /// <summary>
    ///     Gets a boolean value indicating if <see cref="TreeDataset" /> is not equal to <see cref="BaseDataset" />
    /// </summary>
    public bool IsModified => !TreeDataset.Equals( BaseDataset );

    /// <inheritdoc />
    public override string Text
    {
        get => TreeDataset.Name.GetLastPathElement( );
        set => Logger.Warn( "Illegal attempt to set text of a tree node to {0}", value );
    }

    /// <summary>
    ///     The tree representation of the <see cref="ZfsRecord" />.
    /// </summary>
    /// <remarks>
    ///     This is the object that gets modified by changes to the input fields.<br />
    ///     When an object is saved, the changes are copied to <see cref="BaseDataset" />
    /// </remarks>
    public ZfsRecord TreeDataset { get; }

    /// <summary>
    ///     The base instance of the <see cref="ZfsRecord" />.
    /// </summary>
    /// <remarks>
    ///     This object remains unmodified until the user saves an object.<br />
    ///     At that point, <see cref="CopyTreeDatasetPropertiesToBaseDataset" /> is called to copy the changes to this object.<br />
    ///     This object should not be directly accessed by user code.
    /// </remarks>
    private ZfsRecord BaseDataset { get; }

    /// <summary>
    ///     Copies the values and <see cref="IZfsProperty.IsLocal" /> property of all fields changed for the current object from
    ///     <see cref="BaseDataset" /> to <see cref="TreeDataset" />, effectively resetting <see cref="TreeDataset" /> to its original
    ///     state.
    /// </summary>
    /// <param name="clearModifiedPropertiesCollection"></param>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public void CopyBaseDatasetPropertiesToTreeDataset( bool clearModifiedPropertiesCollection = true )
    {
        foreach ( string propName in _modifiedPropertiesSinceLastSave.Keys )
        {
            switch ( BaseDataset[ propName ] )
            {
                case ZfsProperty<bool> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
                case ZfsProperty<int> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
                case ZfsProperty<DateTimeOffset> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
                case ZfsProperty<string> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
            }
        }

        if ( clearModifiedPropertiesCollection )
        {
            _modifiedPropertiesSinceLastSave.Clear( );
        }
    }

    /// <summary>
    ///     Copies the values and <see cref="IZfsProperty.IsLocal" /> property of all fields changed for the current object from
    ///     <see cref="TreeDataset" /> to <see cref="BaseDataset" />.<br />
    ///     This method should be called after saving an object, to avoid needing a refresh from ZFS.
    /// </summary>
    /// <param name="clearModifiedPropertiesCollection"></param>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public void CopyTreeDatasetPropertiesToBaseDataset( bool clearModifiedPropertiesCollection = true )
    {
        foreach ( string propName in _modifiedPropertiesSinceLastSave.Keys )
        {
            switch ( TreeDataset[ propName ] )
            {
                case ZfsProperty<bool> prop:
                    BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
                case ZfsProperty<int> prop:
                    BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
                case ZfsProperty<DateTimeOffset> prop:
                    BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
                case ZfsProperty<string> prop:
                    BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                    continue;
            }
        }

        if ( clearModifiedPropertiesCollection )
        {
            _modifiedPropertiesSinceLastSave.Clear( );
        }
    }

    /// <summary>
    ///     Inherits the given property for <see cref="TreeDataset" /> from its parent
    /// </summary>
    /// <param name="propertyName"></param>
    public void InheritPropertyFromParent( string propertyName )
    {
        if ( TreeDataset.IsPoolRoot )
        {
            Logger.Warn( "Invalid attempt to inherit a property on pool root {0}", TreeDataset.Name );
            return;
        }

        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
            {
                ZfsProperty<bool> newProperty = TreeDataset.InheritBoolPropertyFromParent( propertyName );
                _inheritedPropertiesSinceLastSave[ propertyName ] = newProperty;
            }
                break;
            case ZfsPropertyNames.RecursionPropertyName:
            case ZfsPropertyNames.TemplatePropertyName:
            {
                ZfsProperty<string> newProperty = TreeDataset.InheritStringPropertyFromParent( propertyName );
                _inheritedPropertiesSinceLastSave[ propertyName ] = newProperty;
            }
                break;
        }
    }

    /// <inheritdoc />
    public override string ToString( )
    {
        return TreeDataset.Name.GetLastPathElement( );
    }

    /// <summary>
    ///     Updates a boolean property of the <see cref="TreeDataset" />, tracks the modified property, and returns a reference to the
    ///     newly-updated property.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <param name="isLocal">
    ///     If this property is set locally on this node, <see langword="true" /> (default); Otherwise, false
    /// </param>
    /// <returns>
    ///     A <see langword="ref" /> <see cref="ZfsProperty{T}" /> where T is <see langword="bool" />, referring to the newly-updated
    ///     property
    /// </returns>
    public ref readonly ZfsProperty<bool> UpdateTreeNodeProperty( string propertyName, bool propertyValue, bool isLocal = true )
    {
        _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<bool>( TreeDataset, propertyName, propertyValue, isLocal );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    /// <summary>
    ///     Updates an integer property of the <see cref="TreeDataset" />, tracks the modified property, and returns a reference to the
    ///     newly-updated property.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <returns>
    ///     A <see langword="ref" /> <see cref="ZfsProperty{T}" /> where T is <see langword="int" />, referring to the newly-updated
    ///     property
    /// </returns>
    public ref readonly ZfsProperty<int> UpdateTreeNodeProperty( string propertyName, int propertyValue, bool isLocal = true )
    {
        _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<int>( TreeDataset, propertyName, propertyValue, isLocal );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    /// <summary>
    ///     Updates a string property of the <see cref="TreeDataset" />, tracks the modified property, and returns a reference to the
    ///     newly-updated property.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <returns>
    ///     A <see langword="ref" /> <see cref="ZfsProperty{T}" /> where T is <see langword="string" />, referring to the newly-updated
    ///     property
    /// </returns>
    public ref readonly ZfsProperty<string> UpdateTreeNodeProperty( string propertyName, string propertyValue, bool isLocal = true )
    {
        _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<string>( TreeDataset, propertyName, propertyValue, isLocal );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    /// <summary>
    ///     Gets a list of the properties that have been changed from local to inherited from the parent for this node
    /// </summary>
    internal bool GetInheritedZfsProperties( [NotNullWhen( true )] out List<IZfsProperty>? inheritedProperties )
    {
        inheritedProperties = null;
        if ( _inheritedPropertiesSinceLastSave.IsEmpty )
        {
            return false;
        }

        inheritedProperties = _inheritedPropertiesSinceLastSave.Values.ToList( );
        return true;
    }

    /// <summary>
    ///     Gets a list of the properties that have been changed for this node
    /// </summary>
    internal bool GetModifiedZfsProperties( [NotNullWhen( true )] out List<IZfsProperty>? modifiedProperties )
    {
        modifiedProperties = null;
        if ( _modifiedPropertiesSinceLastSave.IsEmpty )
        {
            return false;
        }

        modifiedProperties = _modifiedPropertiesSinceLastSave.Values.ToList( );
        return true;
    }
}
