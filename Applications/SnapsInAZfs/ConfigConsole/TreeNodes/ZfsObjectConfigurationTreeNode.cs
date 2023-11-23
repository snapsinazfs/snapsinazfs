#region MIT LICENSE
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// See https://opensource.org/license/MIT/
#endregion

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.ConfigConsole.TreeNodes;

/// <summary>
///     Represents a node in the tree of datasets in <see cref="ZfsConfigurationWindow" />
/// </summary>
public sealed class ZfsObjectConfigurationTreeNode : TreeNode
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private readonly object _childrenLock = new( );
    private readonly ConcurrentDictionary<string, IZfsProperty> _inheritedPropertiesSinceLastSave = new( );

    private readonly ConcurrentDictionary<string, IZfsProperty> _modifiedPropertiesSinceLastSave = new( );
    private readonly object _propertyChangeCollectionsLock = new( );

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
            lock ( _childrenLock )
            {
                List<ITreeNode> list = new( );
                foreach ( ( string childName, ZfsRecord child ) in TreeDataset.GetSortedChildDatasets( ) )
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
    }

    /// <summary>
    ///     Gets a boolean value indicating if any properties have been changed or explicitly inherited in the UI for this specific node,
    ///     and not just due to inheritance from a modified parent.
    /// </summary>
    public bool IsLocallyModified
    {
        get
        {
            lock ( _propertyChangeCollectionsLock )
            {
                return !_modifiedPropertiesSinceLastSave.IsEmpty || !_inheritedPropertiesSinceLastSave.IsEmpty;
            }
        }
    }

    /// <summary>
    ///     Gets a boolean value indicating if <see cref="TreeDataset" /> is not equal to <see cref="BaseDataset" />
    /// </summary>
    public bool IsModified => !TreeDataset.Equals( BaseDataset );

    /// <inheritdoc />
    /// <remarks>
    ///     Attempts to set are ignored. Getter returns the last element of <see cref="TreeDataset" />'s Name property.
    /// </remarks>
    public override string Text
    {
        get => TreeDataset.Name.GetLastPathElement( );
        set
        {
            // Just eat this
        }
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
    /// <param name="clearChangedPropertiesCollections"></param>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public void CopyBaseDatasetPropertiesToTreeDataset( bool clearChangedPropertiesCollections = true )
    {
        lock ( _propertyChangeCollectionsLock )
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
                    case ZfsProperty<string> prop:
                        TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                }

                throw new InvalidOperationException( $"An unexpected property ({propName}) was encountered in the modified properties collection" );
            }

            foreach ( string propName in _inheritedPropertiesSinceLastSave.Keys )
            {
                switch ( BaseDataset[ propName ] )
                {
                    case ZfsProperty<bool> prop:
                        TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                    case ZfsProperty<int> prop:
                        TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                    case ZfsProperty<string> prop:
                        TreeDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                }

                throw new InvalidOperationException( $"An unexpected property ({propName}) was encountered in the inherited properties collection" );
            }

            if ( clearChangedPropertiesCollections )
            {
                _modifiedPropertiesSinceLastSave.Clear( );
                _inheritedPropertiesSinceLastSave.Clear( );
            }
        }
    }

    /// <summary>
    ///     Copies the values and <see cref="IZfsProperty.IsLocal" /> property of all fields changed for the current object from
    ///     <see cref="TreeDataset" /> to <see cref="BaseDataset" />.<br />
    ///     This method should be called after saving an object, to avoid needing a refresh from ZFS.
    /// </summary>
    /// <param name="clearChangedPropertiesCollections"></param>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public void CopyTreeDatasetPropertiesToBaseDataset( bool clearChangedPropertiesCollections = true )
    {
        lock ( _propertyChangeCollectionsLock )
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
                    case ZfsProperty<string> prop:
                        BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                }

                throw new InvalidOperationException( $"An unexpected property ({propName}) was encountered in the modified properties collection" );
            }

            foreach ( string propName in _inheritedPropertiesSinceLastSave.Keys )
            {
                switch ( TreeDataset[ propName ] )
                {
                    case ZfsProperty<bool> prop:
                        BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                    case ZfsProperty<int> prop:
                        BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                    case ZfsProperty<string> prop:
                        BaseDataset.UpdateProperty( propName, prop.Value, prop.IsLocal );
                        continue;
                }

                throw new InvalidOperationException( $"An unexpected property ({propName}) was encountered in the inherited properties collection" );
            }

            if ( clearChangedPropertiesCollections )
            {
                _modifiedPropertiesSinceLastSave.Clear( );
                _inheritedPropertiesSinceLastSave.Clear( );
            }
        }
    }

    /// <summary>
    ///     Inherits the given property for <see cref="TreeDataset" /> from its parent and reports success or failure
    /// </summary>
    /// <param name="propertyName"></param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="propertyName" /> is not handled by this method</exception>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <returns>
    ///     A <see langword="bool" /> indicating if the requested property was inherited
    /// </returns>
    /// <remarks>
    ///     If the specified property has already been modified, this method first reverts the change to that property before inheriting
    ///     from parent.
    /// </remarks>
    public bool InheritPropertyFromParent( string propertyName )
    {
        if ( TreeDataset.IsPoolRoot )
        {
            Logger.Warn( "Invalid attempt to inherit a property on pool root {0}", TreeDataset.Name );
            return false;
        }

        switch ( propertyName )
        {
            case ZfsPropertyNames.EnabledPropertyName:
            case ZfsPropertyNames.TakeSnapshotsPropertyName:
            case ZfsPropertyNames.PruneSnapshotsPropertyName:
            {
                lock ( _propertyChangeCollectionsLock )
                {
                    RevertBooleanPropertyToBase( propertyName, _modifiedPropertiesSinceLastSave );

                    ZfsProperty<bool> boolProperty = TreeDataset.InheritBoolPropertyFromParent( propertyName );
                    if ( IsModified )
                    {
                        _inheritedPropertiesSinceLastSave[ propertyName ] = boolProperty;
                    }
                }
            }
                return true;
            case ZfsPropertyNames.RecursionPropertyName:
            case ZfsPropertyNames.TemplatePropertyName:
            {
                lock ( _propertyChangeCollectionsLock )
                {
                    RevertStringPropertyToBase( propertyName, _modifiedPropertiesSinceLastSave );

                    ZfsProperty<string> stringProperty = TreeDataset.InheritStringPropertyFromParent( propertyName );
                    if ( IsModified )
                    {
                        _inheritedPropertiesSinceLastSave[ propertyName ] = stringProperty;
                    }
                }
            }
                return true;
            case ZfsPropertyNames.SnapshotRetentionFrequentPropertyName:
            case ZfsPropertyNames.SnapshotRetentionHourlyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionDailyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionYearlyPropertyName:
            case ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName:
            {
                lock ( _propertyChangeCollectionsLock )
                {
                    RevertIntPropertyToBase( propertyName, _modifiedPropertiesSinceLastSave );

                    ZfsProperty<int> intProperty = TreeDataset.InheritIntPropertyFromParent( propertyName );
                    if ( IsModified )
                    {
                        _inheritedPropertiesSinceLastSave[ propertyName ] = intProperty;
                    }
                }
            }
                return true;
            case ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName:
            case ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName:
                Logger.Error( new ArgumentException( $"Cannot inherit {propertyName}. Last Snapshot Timestamp properties have no meaning to inheritors and are not valid for inheritance", nameof( propertyName ) ) );
                return false;
            default:
            {
                if ( IZfsProperty.AllKnownProperties.Contains( propertyName ) )
                {
                    Logger.Error( new ArgumentOutOfRangeException( $"Property {propertyName} cannot be inherited", new InvalidOperationException( $"Property {propertyName} is not currently handled by InheritPropertyFromParent" ) ) );
                    return false;
                }

                throw new ArgumentOutOfRangeException( nameof( propertyName ), $"Property {propertyName} cannot be inherited - unsupported property" );
            }
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
    /// <remarks>
    ///     If the specified property has already been inherited, this method first reverts the change to that property before making
    ///     this change
    /// </remarks>
    public ref readonly ZfsProperty<bool> UpdateTreeNodeProperty( string propertyName, in bool propertyValue )
    {
        lock ( _propertyChangeCollectionsLock )
        {
            RevertBooleanPropertyToBase( propertyName, _inheritedPropertiesSinceLastSave );
            _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<bool>( TreeDataset, propertyName, in propertyValue );
            return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
        }
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
    /// <remarks>
    ///     If the specified property has already been inherited, this method first reverts the change to that property before making
    ///     this change
    /// </remarks>
    public ref readonly ZfsProperty<int> UpdateTreeNodeProperty( string propertyName, in int propertyValue )
    {
        lock ( _propertyChangeCollectionsLock )
        {
            RevertIntPropertyToBase( propertyName, _inheritedPropertiesSinceLastSave );
            _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<int>( TreeDataset, propertyName, in propertyValue );
            return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
        }
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
    /// <remarks>
    ///     If the specified property has already been inherited, this method first reverts the change to that property before making
    ///     this change
    /// </remarks>
    public ref readonly ZfsProperty<string> UpdateTreeNodeProperty( string propertyName, in string propertyValue )
    {
        lock ( _propertyChangeCollectionsLock )
        {
            RevertStringPropertyToBase( propertyName, _inheritedPropertiesSinceLastSave );
            _modifiedPropertiesSinceLastSave[ propertyName ] = new ZfsProperty<string>( TreeDataset, propertyName, in propertyValue );
            return ref TreeDataset.UpdateProperty( propertyName, propertyValue );
        }
    }

    /// <summary>
    ///     Gets a list of the properties that have been changed from local to inherited from the parent for this node
    /// </summary>
    internal bool GetInheritedZfsProperties( [NotNullWhen( true )] out List<IZfsProperty>? inheritedProperties )
    {
        lock ( _propertyChangeCollectionsLock )
        {
            inheritedProperties = null;
            if ( _inheritedPropertiesSinceLastSave.IsEmpty )
            {
                return false;
            }

            inheritedProperties = _inheritedPropertiesSinceLastSave.Values.ToList( );
            return true;
        }
    }

    /// <summary>
    ///     Gets a list of the properties that have been changed for this node
    /// </summary>
    internal bool GetModifiedZfsProperties( [NotNullWhen( true )] out List<IZfsProperty>? modifiedProperties )
    {
        lock ( _propertyChangeCollectionsLock )
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

    /// <summary>
    ///     Reverts a boolean property back to its base state, if it existed in the specified
    ///     <paramref name="changedPropertyCollection" />
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="changedPropertyCollection"></param>
    private void RevertBooleanPropertyToBase( string propertyName, ConcurrentDictionary<string, IZfsProperty> changedPropertyCollection )
    {
        lock ( _propertyChangeCollectionsLock )
        {
            if ( !changedPropertyCollection.TryRemove( propertyName, out _ ) )
            {
                return;
            }

            if ( BaseDataset.TryGetBoolProperty( propertyName, out ZfsProperty<bool> baseProperty ) )
            {
                Logger.Trace( "Reverting previously-modified property {0} on {1} to original value {2}", propertyName, TreeDataset.Name, baseProperty.Value );
                TreeDataset.UpdateProperty( propertyName, baseProperty.Value, baseProperty.IsLocal );
            }
            else
            {
                Logger.Error( "Unexpected property type encountered when updating {0} on {1}", propertyName, TreeDataset.Name );
            }
        }
    }

    /// <summary>
    ///     Reverts an int property back to its base state, if it existed in the specified
    ///     <paramref name="changedPropertyCollection" />
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="changedPropertyCollection"></param>
    private void RevertIntPropertyToBase( string propertyName, ConcurrentDictionary<string, IZfsProperty> changedPropertyCollection )
    {
        lock ( _propertyChangeCollectionsLock )
        {
            if ( !changedPropertyCollection.TryRemove( propertyName, out _ ) )
            {
                return;
            }
        }

        if ( BaseDataset[ propertyName ] is ZfsProperty<int> baseProperty )
        {
            Logger.Trace( "Reverting previously-modified property {0} on {1} to original value", propertyName, TreeDataset.Name );
            TreeDataset.UpdateProperty( propertyName, baseProperty.Value, baseProperty.IsLocal );
        }
        else
        {
            Logger.Error( "Unexpected property type encountered when updating {0} on {1}", propertyName, TreeDataset.Name );
        }
    }

    /// <summary>
    ///     Reverts a string property back to its base state, if it existed in the specified
    ///     <paramref name="changedPropertyCollection" />
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="changedPropertyCollection"></param>
    private void RevertStringPropertyToBase( string propertyName, ConcurrentDictionary<string, IZfsProperty> changedPropertyCollection )
    {
        lock ( _propertyChangeCollectionsLock )
        {
            if ( !changedPropertyCollection.TryRemove( propertyName, out _ ) )
            {
                Logger.Warn( "Property {0} is not in the collection - cannot revert", propertyName );
                return;
            }
        }

        Logger.Trace( "Reverting previously-modified property {0} on {1} to original value", propertyName, TreeDataset.Name );
        switch ( propertyName )
        {
            case ZfsPropertyNames.RecursionPropertyName:
            {
                TreeDataset.UpdateProperty( propertyName, BaseDataset.Recursion.Value, BaseDataset.Recursion.IsLocal );
            }
                return;
            case ZfsPropertyNames.TemplatePropertyName:
            {
                TreeDataset.UpdateProperty( propertyName, BaseDataset.Template.Value, BaseDataset.Template.IsLocal );
            }
                return;
            default:
            {
                Logger.Error( "Unsupported string property when updating {0} on {1}", propertyName, TreeDataset.Name );
            }
                return;
        }
    }
}
