// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsPropertyTests;

[TestFixture]
[Category( "General" )]
[Category( "TypeChecks" )]
[FixtureLifeCycle( LifeCycle.SingleInstance )]
[Order( 10 )]
[TestOf( typeof( ZfsProperty<> ) )]
public class ZfsPropertyTests
{
    [Test]
    [TestCaseSource( nameof( BoolEqualityTestCaseData ) )]
    public bool Equals_BoolProperty_OtherObjectTypeT<T>( string propertyName, bool propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<bool> zfsProperty = ZfsProperty<bool>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty.Equals( tv ),
            ZfsProperty<int> tv => zfsProperty.Equals( tv ),
            bool tv => zfsProperty.Equals( tv ),
            ZfsProperty<bool> tv => zfsProperty.Equals( tv ),
            string tv => zfsProperty.Equals( tv ),
            ZfsProperty<string> tv => zfsProperty.Equals( tv ),
            DateTimeOffset tv => zfsProperty.Equals( tv ),
            ZfsProperty<DateTimeOffset> tv => zfsProperty.Equals( tv ),
            _ => WarnOnBadTypeAndReturnFalse( )
        };
    }

    [Test]
    [TestCaseSource( nameof( DateTimeOffsetEqualityTestCaseData ) )]
    public bool Equals_DateTimeOffsetProperty_OtherObjectTypeT<T>( string propertyName, DateTimeOffset propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<DateTimeOffset> zfsProperty = ZfsProperty<DateTimeOffset>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty.Equals( tv ),
            ZfsProperty<int> tv => zfsProperty.Equals( tv ),
            bool tv => zfsProperty.Equals( tv ),
            ZfsProperty<bool> tv => zfsProperty.Equals( tv ),
            string tv => zfsProperty.Equals( tv ),
            ZfsProperty<string> tv => zfsProperty.Equals( tv ),
            DateTimeOffset tv => zfsProperty.Equals( tv ),
            ZfsProperty<DateTimeOffset> tv => zfsProperty.Equals( tv ),
            _ => WarnOnBadTypeAndReturnFalse( )
        };
    }

    [Test]
    [TestCaseSource( nameof( IntEqualityTestCaseData ) )]
    public bool Equals_IntProperty_OtherObjectTypeT<T>( string propertyName, int propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<int> zfsProperty = ZfsProperty<int>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty.Equals( tv ),
            ZfsProperty<int> tv => zfsProperty.Equals( tv ),
            bool tv => zfsProperty.Equals( tv ),
            ZfsProperty<bool> tv => zfsProperty.Equals( tv ),
            string tv => zfsProperty.Equals( tv ),
            ZfsProperty<string> tv => zfsProperty.Equals( tv ),
            DateTimeOffset tv => zfsProperty.Equals( tv ),
            ZfsProperty<DateTimeOffset> tv => zfsProperty.Equals( tv ),
            _ => WarnOnBadTypeAndReturnFalse( )
        };
    }

    [Test]
    [TestCaseSource( nameof( StringEqualityTestCaseData ) )]
    public bool Equals_StringProperty_OtherObjectTypeT<T>( string propertyName, string propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<string> zfsProperty = ZfsProperty<string>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty.Equals( tv ),
            ZfsProperty<int> tv => zfsProperty.Equals( tv ),
            bool tv => zfsProperty.Equals( tv ),
            ZfsProperty<bool> tv => zfsProperty.Equals( tv ),
            string tv => zfsProperty.Equals( tv ),
            ZfsProperty<string> tv => zfsProperty.Equals( tv ),
            DateTimeOffset tv => zfsProperty.Equals( tv ),
            ZfsProperty<DateTimeOffset> tv => zfsProperty.Equals( tv ),
            _ => WarnOnBadTypeAndReturnFalse( )
        };
    }

    [Test]
    public void GetHashCode_PropertyUseableAsDictionaryKey( )
    {
        Dictionary<ZfsProperty<string>, string> dict = new( );
        ZfsProperty<string> property1 = ZfsProperty<string>.CreateWithoutParent( "property1", "key 1" );
        ZfsProperty<string> property2 = ZfsProperty<string>.CreateWithoutParent( "property2", "key 2" );
        dict.Add( property1, "value 1" );
        dict.Add( property2, "value 2" );

        Assert.Multiple( ( ) =>
        {
            Assert.That( dict[ property1 ], Is.EqualTo( "value 1" ) );
            Assert.That( dict[ property2 ], Is.EqualTo( "value 2" ) );
        } );
    }

    [Test]
    public void InheritedFrom_Correct_WhenIsLocalFalseInheritingOneGeneration( )
    {
        ZfsRecord gen1Record = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        ZfsRecord gen2Record = gen1Record.CreateChildDataset( $"{gen1Record.Name}/gen2", ZfsPropertyValueConstants.FileSystem );
        ZfsProperty<bool> gen1Enabled = gen1Record.Enabled;
        ZfsProperty<bool> gen2Enabled = gen2Record.Enabled;
        Assume.That( gen1Enabled.IsLocal, Is.True );
        Assume.That( gen2Enabled.IsLocal, Is.False );
        Assert.That( gen2Enabled.InheritedFrom, Is.EqualTo( "gen1" ) );
    }

    [Test]
    public void InheritedFrom_Correct_WhenIsLocalFalseInheritingTwoGenerations( )
    {
        ZfsRecord gen1Record = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        ZfsRecord gen2Record = gen1Record.CreateChildDataset( $"{gen1Record.Name}/gen2", ZfsPropertyValueConstants.FileSystem );
        ZfsRecord gen3Record = gen2Record.CreateChildDataset( $"{gen2Record.Name}/gen3", ZfsPropertyValueConstants.FileSystem );
        ZfsProperty<bool> gen1Enabled = gen1Record.Enabled;
        ZfsProperty<bool> gen2Enabled = gen2Record.Enabled;
        ZfsProperty<bool> gen3Enabled = gen3Record.Enabled;
        Assume.That( gen1Enabled.IsLocal, Is.True );
        Assume.That( gen2Enabled.IsLocal, Is.False );
        Assume.That( gen3Enabled.IsLocal, Is.False );
        Assert.That( gen3Enabled.InheritedFrom, Is.EqualTo( "gen1" ) );
    }

    [Test]
    public void InheritedFrom_Correct_WhenIsLocalTrue( )
    {
        ZfsRecord gen1Record = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        ZfsProperty<bool> gen1Enabled = gen1Record.Enabled;
        Assert.That( gen1Enabled.InheritedFrom, Is.EqualTo( ZfsPropertySourceConstants.Local ) );
    }

    [Test]
    public void InheritedFrom_Correct_WhenIsLocalTrueAndParentInheriting( )
    {
        ZfsRecord gen1Record = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        ZfsRecord gen2Record = gen1Record.CreateChildDataset( $"{gen1Record.Name}/gen2", ZfsPropertyValueConstants.FileSystem );
        ZfsRecord gen3Record = gen2Record.CreateChildDataset( $"{gen2Record.Name}/gen3", ZfsPropertyValueConstants.FileSystem );
        ZfsProperty<bool> gen1Enabled = gen1Record.Enabled;
        ZfsProperty<bool> gen2Enabled = gen2Record.Enabled;
        ZfsProperty<bool> gen3Enabled = gen3Record.UpdateProperty( ZfsPropertyNames.EnabledPropertyName, gen3Record.Enabled.Value );
        Assume.That( gen1Enabled.IsLocal, Is.True );
        Assume.That( gen2Enabled.IsLocal, Is.False );
        Assume.That( gen2Enabled.Source, Is.EqualTo( "inherited from gen1" ) );
        Assume.That( gen3Enabled.IsLocal, Is.True );
        Assert.That( gen3Enabled.InheritedFrom, Is.EqualTo( ZfsPropertySourceConstants.Local ) );
    }

    [Test]
    [TestCase( true, ExpectedResult = false )]
    [TestCase( false, ExpectedResult = true )]
    public bool IsInherited_AsExpected( bool isLocal )
    {
        ZfsProperty<string> property = ZfsProperty<string>.CreateWithoutParent( "unimportantName", "unimportantValue", isLocal );
        return property.IsInherited;
    }

    [Test]
    [TestCase( true, ExpectedResult = true )]
    [TestCase( false, ExpectedResult = false )]
    public bool IsLocal_AsExpected( bool isLocal )
    {
        ZfsProperty<string> property = ZfsProperty<string>.CreateWithoutParent( "unimportantName", "unimportantValue", isLocal );
        return property.IsLocal;
    }

    [Test]
    [TestCaseSource( nameof( BoolEqualityTestCaseData ) )]
    public bool OperatorEquals_BoolProperty_OtherObjectTypeT<T>( string propertyName, bool propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<bool> zfsProperty = ZfsProperty<bool>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty == tv,
            ZfsProperty<int> tv => zfsProperty == tv,
            bool tv => zfsProperty == tv,
            ZfsProperty<bool> tv => zfsProperty == tv,
            string tv => zfsProperty == tv,
            ZfsProperty<string> tv => zfsProperty == tv,
            DateTimeOffset tv => zfsProperty == tv,
            ZfsProperty<DateTimeOffset> tv => zfsProperty == tv,
            _ => false
        };
    }

    [Test]
    [TestCaseSource( nameof( DateTimeOffsetEqualityTestCaseData ) )]
    public bool OperatorEquals_DateTimeOffsetProperty_OtherObjectTypeT<T>( string propertyName, DateTimeOffset propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<DateTimeOffset> zfsProperty = ZfsProperty<DateTimeOffset>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty == tv,
            ZfsProperty<int> tv => zfsProperty == tv,
            bool tv => zfsProperty == tv,
            ZfsProperty<bool> tv => zfsProperty == tv,
            string tv => zfsProperty == tv,
            ZfsProperty<string> tv => zfsProperty == tv,
            DateTimeOffset tv => zfsProperty == tv,
            ZfsProperty<DateTimeOffset> tv => zfsProperty == tv,
            _ => false
        };
    }

    [Test]
    [TestCaseSource( nameof( IntEqualityTestCaseData ) )]
    public bool OperatorEquals_IntProperty_OtherObjectTypeT<T>( string propertyName, int propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<int> zfsProperty = ZfsProperty<int>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty == tv,
            ZfsProperty<int> tv => zfsProperty == tv,
            bool tv => zfsProperty == tv,
            ZfsProperty<bool> tv => zfsProperty == tv,
            string tv => zfsProperty == tv,
            ZfsProperty<string> tv => zfsProperty == tv,
            DateTimeOffset tv => zfsProperty == tv,
            ZfsProperty<DateTimeOffset> tv => zfsProperty == tv,
            _ => false
        };
    }

    [Test]
    [TestCaseSource( nameof( StringEqualityTestCaseData ) )]
    public bool OperatorEquals_StringProperty_OtherObjectTypeT<T>( string propertyName, string propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<string> zfsProperty = ZfsProperty<string>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => zfsProperty == tv,
            ZfsProperty<int> tv => zfsProperty == tv,
            bool tv => zfsProperty == tv,
            ZfsProperty<bool> tv => zfsProperty == tv,
            string tv => zfsProperty == tv,
            ZfsProperty<string> tv => zfsProperty == tv,
            DateTimeOffset tv => zfsProperty == tv,
            ZfsProperty<DateTimeOffset> tv => zfsProperty == tv,
            _ => false
        };
    }

    [Test]
    [TestCaseSource( nameof( BoolEqualityTestCaseData ) )]
    public bool OperatorNotEquals_BoolProperty_OtherObjectTypeT<T>( string propertyName, bool propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<bool> zfsProperty = ZfsProperty<bool>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => !( zfsProperty != tv ),
            ZfsProperty<int> tv => !( zfsProperty != tv ),
            bool tv => !( zfsProperty != tv ),
            ZfsProperty<bool> tv => !( zfsProperty != tv ),
            string tv => !( zfsProperty != tv ),
            ZfsProperty<string> tv => !( zfsProperty != tv ),
            DateTimeOffset tv => !( zfsProperty != tv ),
            ZfsProperty<DateTimeOffset> tv => !( zfsProperty != tv ),
            _ => false
        };
    }

    [Test]
    [TestCaseSource( nameof( DateTimeOffsetEqualityTestCaseData ) )]
    public bool OperatorNotEquals_DateTimeOffsetProperty_OtherObjectTypeT<T>( string propertyName, DateTimeOffset propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<DateTimeOffset> zfsProperty = ZfsProperty<DateTimeOffset>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => !( zfsProperty != tv ),
            ZfsProperty<int> tv => !( zfsProperty != tv ),
            bool tv => !( zfsProperty != tv ),
            ZfsProperty<bool> tv => !( zfsProperty != tv ),
            string tv => !( zfsProperty != tv ),
            ZfsProperty<string> tv => !( zfsProperty != tv ),
            DateTimeOffset tv => !( zfsProperty != tv ),
            ZfsProperty<DateTimeOffset> tv => !( zfsProperty != tv ),
            _ => false
        };
    }

    [Test]
    [TestCaseSource( nameof( IntEqualityTestCaseData ) )]
    public bool OperatorNotEquals_IntProperty_OtherObjectTypeT<T>( string propertyName, int propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<int> zfsProperty = ZfsProperty<int>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => !( zfsProperty != tv ),
            ZfsProperty<int> tv => !( zfsProperty != tv ),
            bool tv => !( zfsProperty != tv ),
            ZfsProperty<bool> tv => !( zfsProperty != tv ),
            string tv => !( zfsProperty != tv ),
            ZfsProperty<string> tv => !( zfsProperty != tv ),
            DateTimeOffset tv => !( zfsProperty != tv ),
            ZfsProperty<DateTimeOffset> tv => !( zfsProperty != tv ),
            _ => false
        };
    }

    [Test]
    [TestCaseSource( nameof( StringEqualityTestCaseData ) )]
    public bool OperatorNotEquals_StringProperty_OtherObjectTypeT<T>( string propertyName, string propertyValue, T testValue ) where T : notnull
    {
        ZfsProperty<string> zfsProperty = ZfsProperty<string>.CreateWithoutParent( propertyName, propertyValue );
        return testValue switch
        {
            int tv => !( zfsProperty != tv ),
            ZfsProperty<int> tv => !( zfsProperty != tv ),
            bool tv => !( zfsProperty != tv ),
            ZfsProperty<bool> tv => !( zfsProperty != tv ),
            string tv => !( zfsProperty != tv ),
            ZfsProperty<string> tv => !( zfsProperty != tv ),
            DateTimeOffset tv => !( zfsProperty != tv ),
            ZfsProperty<DateTimeOffset> tv => !( zfsProperty != tv ),
            _ => false
        };
    }

    [Test]
    public void ReferenceEqualityOfInterfaceFalse( )
    {
        ZfsProperty<DateTimeOffset> propertyA = ZfsProperty<DateTimeOffset>.CreateWithoutParent( "sameDateTimeOffsetName", DateTimeOffset.UnixEpoch );
        ZfsProperty<DateTimeOffset> propertyB = propertyA with { Name = "sameDateTimeOffsetName", Value = DateTimeOffset.UnixEpoch };
        IZfsProperty propertyABoxed = propertyA;
        // Another boxed instance of the SAME object. Should not have the same reference.
        IZfsProperty propertyABoxedCopy = propertyA;
        IZfsProperty propertyBBoxed = propertyB;
        Assert.That( propertyABoxed, Is.Not.SameAs( propertyBBoxed ) );
        Assert.That( propertyABoxed, Is.Not.SameAs( propertyABoxedCopy ) );
    }

    [Test]
    public void Source_Correct_WhenIsLocalFalseAndParentIsAlsoInheriting( )
    {
        ZfsRecord gen1Record = ZfsRecordTestHelpers.GetNewTestRootFileSystem( "gen1" );
        ZfsRecord gen2Record = gen1Record.CreateChildDataset( $"{gen1Record.Name}/gen2", ZfsPropertyValueConstants.FileSystem );
        ZfsRecord gen3Record = gen2Record.CreateChildDataset( $"{gen2Record.Name}/gen3", ZfsPropertyValueConstants.FileSystem );
        ZfsProperty<bool> gen1Enabled = gen1Record.Enabled;
        ZfsProperty<bool> gen2Enabled = gen2Record.Enabled;
        ZfsProperty<bool> gen3Enabled = gen3Record.Enabled;
        Assume.That( gen1Enabled.IsLocal, Is.True );
        Assume.That( gen2Enabled.IsLocal, Is.False );
        Assume.That( gen2Enabled.Source, Is.EqualTo( "inherited from gen1" ) );
        Assume.That( gen3Enabled.IsLocal, Is.False );
        Assert.That( gen3Enabled.Source, Is.EqualTo( "inherited from gen1" ) );
    }

    [Test]
    [TestCase( "true", ExpectedResult = true )]
    [TestCase( "false", ExpectedResult = false )]
    public bool TryParse_Bool_OutputsExpectedValue( string value )
    {
        RawProperty input = new( ZfsPropertyNames.EnabledPropertyName, value, ZfsPropertySourceConstants.Local );
        bool success = ZfsProperty<bool>.TryParse( input, out ZfsProperty<bool>? property );
        Assert.Multiple( ( ) =>
        {
            Assert.That( success, Is.True );
            Assert.That( property, Is.Not.Null );
            Assert.That( property, Is.InstanceOf<ZfsProperty<bool>>( ) );
            Assert.That( property.HasValue, Is.True );
        } );
        return property!.Value.Value;
    }

    [Test]
    [TestCase( "5" )]
    [TestCase( "a" )]
    [TestCase( "" )]
    [TestCase( " " )]
    [TestCase( null )]
    public void TryParse_Bool_ReturnsFalseOnBadInput( string value )
    {
        RawProperty input = new( ZfsPropertyNames.EnabledPropertyName, value, ZfsPropertySourceConstants.Local );
        bool success = ZfsProperty<bool>.TryParse( input, out ZfsProperty<bool>? property );
        Assert.Multiple( ( ) =>
        {
            Assert.That( success, Is.False );
            Assert.That( property, Is.Null );
            Assert.That( property.HasValue, Is.False );
        } );
    }

    [Test]
    public void TryParse_DateTimeOffset_OutputsExpectedValue( )
    {
        const string valueString = "2023-01-01T12:34:56.7891234";
        DateTimeOffset value = DateTimeOffset.Parse( valueString );
        RawProperty input = new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, valueString, ZfsPropertySourceConstants.Local );
        bool success = ZfsProperty<DateTimeOffset>.TryParse( input, out ZfsProperty<DateTimeOffset>? property );
        Assert.Multiple( ( ) =>
        {
            Assert.That( success, Is.True );
            Assert.That( property, Is.Not.Null );
            Assert.That( property, Is.InstanceOf<ZfsProperty<DateTimeOffset>>( ) );
            Assert.That( property.HasValue, Is.True );
            Assert.That( property!.Value.Value, Is.EqualTo( value ) );
        } );
    }

    [Test]
    [TestCase( "true" )]
    [TestCase( "abcdefg" )]
    [TestCase( "1" )]
    [TestCase( "!" )]
    [TestCase( " " )]
    [TestCase( "" )]
    [TestCase( null )]
    public void TryParse_DateTimeOffset_ReturnsFalseOnBadInput( string value )
    {
        RawProperty input = new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, value, ZfsPropertySourceConstants.Local );
        bool success = ZfsProperty<DateTimeOffset>.TryParse( input, out ZfsProperty<DateTimeOffset>? property );
        Assert.Multiple( ( ) =>
        {
            Assert.That( success, Is.False );
            Assert.That( property, Is.Null );
            Assert.That( property.HasValue, Is.False );
        } );
    }

    [Test]
    [TestCase( "-100", ExpectedResult = -100 )]
    [TestCase( "-1", ExpectedResult = -1 )]
    [TestCase( "0", ExpectedResult = 0 )]
    [TestCase( "1", ExpectedResult = 1 )]
    [TestCase( "100", ExpectedResult = 100 )]
    public int TryParse_Int_OutputsExpectedValue( string value )
    {
        RawProperty input = new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, value, ZfsPropertySourceConstants.Local );
        bool success = ZfsProperty<int>.TryParse( input, out ZfsProperty<int>? property );
        Assert.Multiple( ( ) =>
        {
            Assert.That( success, Is.True );
            Assert.That( property, Is.Not.Null );
            Assert.That( property, Is.InstanceOf<ZfsProperty<int>>( ) );
            Assert.That( property.HasValue, Is.True );
        } );
        return property!.Value.Value;
    }

    [Test]
    [TestCase( "true" )]
    [TestCase( "abcdefg" )]
    [TestCase( "!" )]
    [TestCase( " " )]
    [TestCase( "" )]
    [TestCase( null )]
    public void TryParse_Int_ReturnsFalseOnBadInput( string value )
    {
        RawProperty input = new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, value, ZfsPropertySourceConstants.Local );
        bool success = ZfsProperty<int>.TryParse( input, out ZfsProperty<int>? property );
        Assert.Multiple( ( ) =>
        {
            Assert.That( success, Is.False );
            Assert.That( property, Is.Null );
            Assert.That( property.HasValue, Is.False );
        } );
    }

    [Test]
    public void ValueString_AsExpected_BoolProperty( [Values] bool testValue )
    {
        ZfsProperty<bool> zfsProperty = ZfsProperty<bool>.CreateWithoutParent( "someName", testValue );
        Assert.That( zfsProperty.ValueString, Is.EqualTo( testValue.ToString( ).ToLowerInvariant( ) ) );
    }

    [Test]
    public void ValueString_AsExpected_DateTimeOffsetProperty( )
    {
        DateTimeOffset testValue = DateTimeOffset.Now;
        ZfsProperty<DateTimeOffset> zfsProperty = ZfsProperty<DateTimeOffset>.CreateWithoutParent( "someName", testValue );
        Assert.That( zfsProperty.ValueString, Is.EqualTo( testValue.ToString( "O" ) ) );
    }

    [Test]
    public void ValueString_AsExpected_IntProperty( [Values( -1, 0, 1 )] int testValue )
    {
        ZfsProperty<int> zfsProperty = ZfsProperty<int>.CreateWithoutParent( "someName", testValue );
        Assert.That( zfsProperty.ValueString, Is.EqualTo( testValue.ToString( ) ) );
    }

    [Test]
    public void ValueString_AsExpected_StringProperty( [Values( "string 1", "string 2" )] string testValue )
    {
        ZfsProperty<string> zfsProperty = ZfsProperty<string>.CreateWithoutParent( "someName", testValue );
        Assert.That( zfsProperty.ValueString, Is.EqualTo( testValue ) );
    }

    private static TestCaseData[] BoolEqualityTestCaseData( )
    {
        return new[]
        {
            new TestCaseData( "nameString", true, true ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nameString", true, false ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, "same string" ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, "different string 2" ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, DateTimeOffset.UnixEpoch ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, 0 ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, ZfsProperty<string>.CreateWithoutParent( "nameString", "string" ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, ZfsProperty<int>.CreateWithoutParent( "nameString", 1234 ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, ZfsProperty<DateTimeOffset>.CreateWithoutParent( "nameString", DateTimeOffset.UnixEpoch ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", true, ZfsProperty<bool>.CreateWithoutParent( "nameString", true ) ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nameString", true, ZfsProperty<bool>.CreateWithoutParent( "nameString", false ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, true ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, false ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nameString", false, "same string" ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, "different string 2" ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, DateTimeOffset.UnixEpoch ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, 0 ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, ZfsProperty<string>.CreateWithoutParent( "nameString", "string" ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, ZfsProperty<int>.CreateWithoutParent( "nameString", 1234 ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, ZfsProperty<DateTimeOffset>.CreateWithoutParent( "nameString", DateTimeOffset.UnixEpoch ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, ZfsProperty<bool>.CreateWithoutParent( "nameString", true ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", false, ZfsProperty<bool>.CreateWithoutParent( "nameString", false ) ) { ExpectedResult = true, HasExpectedResult = true }
        };
    }

    private static TestCaseData[] DateTimeOffsetEqualityTestCaseData( )
    {
        return new[]
        {
            new TestCaseData( "trueCase", DateTimeOffset.UnixEpoch, true ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "falseCase", DateTimeOffset.UnixEpoch, false ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "stringCase", DateTimeOffset.UnixEpoch, "string" ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "unixEpochCase", DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nowCase", DateTimeOffset.UnixEpoch, DateTimeOffset.Now ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "integerCase", DateTimeOffset.UnixEpoch, 0 ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "trueZfsPropertyCase", DateTimeOffset.UnixEpoch, ZfsProperty<bool>.CreateWithoutParent( "trueZfsPropertyCase", true ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "falseZfsPropertyCase", DateTimeOffset.UnixEpoch, ZfsProperty<bool>.CreateWithoutParent( "falseZfsPropertyCase", false ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "stringZfsPropertyCase", DateTimeOffset.UnixEpoch, ZfsProperty<string>.CreateWithoutParent( "stringZfsPropertyCase", "string" ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "unixEpochZfsPropertyCase", DateTimeOffset.UnixEpoch, ZfsProperty<DateTimeOffset>.CreateWithoutParent( "unixEpochZfsPropertyCase", DateTimeOffset.UnixEpoch ) ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nowZfsPropertyCase", DateTimeOffset.UnixEpoch, ZfsProperty<DateTimeOffset>.CreateWithoutParent( "nowZfsPropertyCase", DateTimeOffset.Now ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "integerZfsPropertyCase", DateTimeOffset.UnixEpoch, ZfsProperty<int>.CreateWithoutParent( "integerZfsPropertyCase", 1234 ) ) { ExpectedResult = false, HasExpectedResult = true }
        };
    }

    private static TestCaseData[] IntEqualityTestCaseData( )
    {
        return new[]
        {
            new TestCaseData( "nameString", 12345, 12345 ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nameString", 12345, ZfsProperty<int>.CreateWithoutParent( "nameString", 12345 ) ) { ExpectedResult = true, HasExpectedResult = true },

            new TestCaseData( "nameString", 12345, 0 ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", 12345, ZfsProperty<int>.CreateWithoutParent( "nameString", 0 ) ) { ExpectedResult = false, HasExpectedResult = true },

            new TestCaseData( "nameString", 12345, "string" ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", 12345, ZfsProperty<string>.CreateWithoutParent( "nameString", "string" ) ) { ExpectedResult = false, HasExpectedResult = true },

            new TestCaseData( "nameString", 12345, DateTimeOffset.UnixEpoch ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", 12345, ZfsProperty<DateTimeOffset>.CreateWithoutParent( "nameString", DateTimeOffset.UnixEpoch ) ) { ExpectedResult = false, HasExpectedResult = true },

            new TestCaseData( "nameString", 12345, true ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", 12345, ZfsProperty<bool>.CreateWithoutParent( "nameString", true ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", 12345, false ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", 12345, ZfsProperty<bool>.CreateWithoutParent( "nameString", false ) ) { ExpectedResult = false, HasExpectedResult = true }
        };
    }

    private static TestCaseData[] StringEqualityTestCaseData( )
    {
        return new[]
        {
            new TestCaseData( "nameString", "string", true ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", false ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", "string" ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", "different string" ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", DateTimeOffset.UnixEpoch ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", 0 ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", ZfsProperty<string>.CreateWithoutParent( "nameString", "string" ) ) { ExpectedResult = true, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", ZfsProperty<string>.CreateWithoutParent( "nameString", "different string" ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", ZfsProperty<int>.CreateWithoutParent( "nameString", 1234 ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", ZfsProperty<DateTimeOffset>.CreateWithoutParent( "nameString", DateTimeOffset.UnixEpoch ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", ZfsProperty<bool>.CreateWithoutParent( "nameString", true ) ) { ExpectedResult = false, HasExpectedResult = true },
            new TestCaseData( "nameString", "string", ZfsProperty<bool>.CreateWithoutParent( "nameString", false ) ) { ExpectedResult = false, HasExpectedResult = true }
        };
    }

    private static bool WarnOnBadTypeAndReturnFalse( )
    {
        Assert.Warn( "Bad test - type would throw a compile error" );
        return false;
    }
}
