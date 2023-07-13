// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

public class ZfsObjectConfigurationTreeNode : TreeNode
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    private readonly ConcurrentDictionary<string, IZfsProperty> _modifiedPropertiesSinceLastSave = new( );

    public ZfsObjectConfigurationTreeNode( string name, ZfsRecord baseDataset, ZfsRecord treeDataset )
        : base( name )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
    }

    private List<ITreeNode>? _children;

    /// <summary>
    ///     The base instance of the <see cref="ZfsRecord" />.
    /// </summary>
    /// <remarks>
    ///     This object remains unmodified until the user saves an object.<br />
    ///     At that point, <see cref="CopyTreeDatasetPropertiesToBaseDataset" /> is called to copy the changes to this object.
    /// </remarks>
    public ZfsRecord BaseDataset { get; }

    /// <inheritdoc />
    public override IList<ITreeNode> Children
    {
        get
        {
            List<ITreeNode> list = new( );
            foreach ( ( string? childName, ZfsRecord? child ) in TreeDataset.GetSortedChildDatasets( ) )
            {
                ITreeNode node = new ZfsObjectConfigurationTreeNode( childName.GetLastPathElement( ), BaseDataset.GetChild( childName ), child );
                list.Add( node );
            }

            return _children ??= list;
        }
    }

    /// <summary>
    ///     Gets a boolean value indicating if any properties have been changed in the UI for this specific node, and not just due to
    ///     inheritance from a modified parent.
    /// </summary>
    public bool IsLocallyModified => _modifiedPropertiesSinceLastSave.Count > 0;

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
    ///     Copies the values and <see cref="IZfsProperty.IsLocal" /> property of all fields changed for the current object from
    ///     <see cref="BaseDataset" /> to <see cref="TreeDataset" />, effectively resetting <see cref="TreeDataset" /> to its original
    ///     state.
    /// </summary>
    /// <param name="clearModifiedPropertiesCollection"></param>
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
    /// <returns>
    ///     A <see langword="ref" /> <see cref="ZfsProperty{T}" /> where T is <see langword="bool" />, referring to the newly-updated
    ///     property
    /// </returns>
    public ref readonly ZfsProperty<bool> UpdateTreeNodeProperty( string propertyName, bool propertyValue )
    {
        _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<bool>( TreeDataset, propertyName, propertyValue );
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
    public ref readonly ZfsProperty<int> UpdateTreeNodeProperty( string propertyName, int propertyValue )
    {
        _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<int>( TreeDataset, propertyName, propertyValue );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    public ref readonly ZfsProperty<DateTimeOffset> UpdateTreeNodeProperty( string propertyName, DateTimeOffset propertyValue )
    {
        _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<DateTimeOffset>( TreeDataset, propertyName, propertyValue );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    public ref readonly ZfsProperty<string> UpdateTreeNodeProperty( string propertyName, string propertyValue )
    {
        _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<string>( TreeDataset, propertyName, propertyValue );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    internal List<IZfsProperty> GetModifiedZfsProperties( )
    {
        return _modifiedPropertiesSinceLastSave.Values.ToList( );
    }
}
