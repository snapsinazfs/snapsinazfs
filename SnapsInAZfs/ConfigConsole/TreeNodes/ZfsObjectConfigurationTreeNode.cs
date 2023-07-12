// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;

using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

public class ZfsObjectConfigurationTreeNode : TreeNode
{
    public ZfsObjectConfigurationTreeNode( string name, ZfsRecord baseDataset, ZfsRecord treeDataset )
        : base( name )
    {
        TreeDataset = treeDataset;
        BaseDataset = baseDataset;
        TreeSnapshots = TreeDataset.Snapshots.Values.SelectMany( periodCollection => periodCollection.Values, ( _, snap ) => new SnapshotListViewEntry( snap.Name, snap, snap with { } ) ).ToList( );
        BaseSnapshots = BaseDataset.Snapshots.Values.SelectMany( periodCollection => periodCollection.Values, ( _, snap ) => new SnapshotListViewEntry( snap.Name, snap, snap with { } ) ).ToList( );
    }

    /// <summary>
    /// Gets or sets the base (unmodified) <see cref="ZfsRecord"/> for this entry.
    /// </summary>
    public ZfsRecord BaseDataset { get; private set; }
    public List<SnapshotListViewEntry> BaseSnapshots { get; set; }
    private readonly ConcurrentDictionary<string, IZfsProperty> _modifiedPropertiesSinceLastSaveForCurrentItem = new( );

    public void ResetTreeDataset( )
    {
        RestorePreviousSelectedZfsTreeNode( );
        _modifiedPropertiesSinceLastSaveForCurrentItem.Clear( );
    }

    public void CopyTreeDatasetToBaseDataset( )
    {
        BaseDataset = TreeDataset.DeepCopyClone( BaseDataset.IsPoolRoot ? null : BaseDataset.ParentDataset );
    }

    /// <summary>
    /// Gets a boolean value indicating if <see cref="TreeDataset"/> is not equal to <see cref="BaseDataset"/>
    /// </summary>
    public bool IsModified => TreeDataset != BaseDataset;

    public ZfsRecord TreeDataset { get; private set; }
    public List<SnapshotListViewEntry> TreeSnapshots { get; set; }

    public ref readonly ZfsProperty<bool> UpdateTreeNodeProperty( string propertyName, bool propertyValue )
    {
        _modifiedPropertiesSinceLastSaveForCurrentItem[ propertyName ] = new ZfsProperty<bool>( propertyName, propertyValue, ZfsPropertySourceConstants.Local );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    public ref readonly ZfsProperty<int> UpdateTreeNodeProperty( string propertyName, int propertyValue )
    {
        _modifiedPropertiesSinceLastSaveForCurrentItem[ propertyName ] = new ZfsProperty<int>( propertyName, propertyValue, ZfsPropertySourceConstants.Local );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    public ref readonly ZfsProperty<DateTimeOffset> UpdateTreeNodeProperty( string propertyName, DateTimeOffset propertyValue )
    {
        _modifiedPropertiesSinceLastSaveForCurrentItem[ propertyName ] = new ZfsProperty<DateTimeOffset>( propertyName, propertyValue, ZfsPropertySourceConstants.Local );
        return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    public IZfsProperty UpdateTreeNodeProperty( string propertyName, string propertyValue )
    {
        _modifiedPropertiesSinceLastSaveForCurrentItem[ propertyName ] = new ZfsProperty<string>( propertyName, propertyValue, ZfsPropertySourceConstants.Local );
        return TreeDataset.UpdateProperty( propertyName, propertyValue );
    }

    public ref readonly ZfsProperty<string> UpdateTreeNodeStringProperty( string propertyName, string propertyValue )
    {
        _modifiedPropertiesSinceLastSaveForCurrentItem[ propertyName ] = new ZfsProperty<string>( propertyName, propertyValue, ZfsPropertySourceConstants.Local );
        return ref TreeDataset.UpdateStringProperty( propertyName, propertyValue );
    }
    /// <summary>
    ///     Performs a depth-first recursion on the entire tree, updating all boolean properties appropriately
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="prop"></param>
    /// <param name="source"></param>
    private static void UpdateDescendentsBooleanPropertyInheritance( ZfsObjectConfigurationTreeNode currentNode, ZfsProperty<bool> prop, string source )
    {
        foreach ( ZfsObjectConfigurationTreeNode child in currentNode.Children.Cast<ZfsObjectConfigurationTreeNode>( ).Where( child => !child.TreeDataset[ prop.Name ].IsLocal ) )
        {
            UpdateDescendentsBooleanPropertyInheritance( child, prop, source );
        }

        // This is the final base case, for the node we started from
        if ( currentNode.TreeDataset[ prop.Name ].IsLocal )
        {
            return;
        }

        // For everyone that makes it this far, we need to inherit, so update the tree and base copies of the property
        currentNode.TreeDataset.UpdateProperty( prop.Name, prop.Value, source );
        currentNode.BaseDataset.UpdateProperty( prop.Name, prop.Value, source );
    }


    internal List<IZfsProperty> GetModifiedZfsProperties( )
    {
        return _modifiedPropertiesSinceLastSaveForCurrentItem.Values.ToList( );
    }

    private void RestorePreviousSelectedZfsTreeNode( )
    {
        foreach ( string propName in _modifiedPropertiesSinceLastSaveForCurrentItem.Keys )
        {
            switch ( BaseDataset[propName] )
            {
                case ZfsProperty<bool> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.Source );
                    continue;
                case ZfsProperty<int> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.Source );
                    continue;
                case ZfsProperty<DateTimeOffset> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.Source );
                    continue;
                case ZfsProperty<string> prop:
                    TreeDataset.UpdateProperty( propName, prop.Value, prop.Source );
                    continue;
            }
        }

        _modifiedPropertiesSinceLastSaveForCurrentItem.Clear( );
    }

}
