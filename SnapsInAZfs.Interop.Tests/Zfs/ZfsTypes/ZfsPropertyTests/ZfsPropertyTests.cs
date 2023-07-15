// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

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
    [TestCase( "sameStringName", "sameString", "sameStringName", "sameString", ExpectedResult = true )]
    [TestCase( "sameStringName", "differentStringA", "sameStringName", "differentStringB", ExpectedResult = false )]
    [TestCase( "differentStringNameA", "sameString", "differentStringNameB", "sameString", ExpectedResult = false )]
    [TestCase( "differentStringNameA", "differentStringA", "differentStringNameB", "differentStringB", ExpectedResult = false )]
    [TestCase( "sameIntName", 1, "sameIntName", 1, ExpectedResult = true )]
    [TestCase( "sameIntName", 1, "sameIntName", 2, ExpectedResult = false )]
    [TestCase( "differentIntNameA", 1, "differentIntNameB", 1, ExpectedResult = false )]
    [TestCase( "differentIntNameA", 1, "differentIntNameB", 2, ExpectedResult = false )]
    [TestCase( "sameLongName", 1L, "sameLongName", 1L, ExpectedResult = true )]
    [TestCase( "sameLongName", 1L, "sameLongName", 2L, ExpectedResult = false )]
    [TestCase( "differentLongNameA", 1L, "differentLongNameB", 1L, ExpectedResult = false )]
    [TestCase( "differentLongNameA", 1L, "differentLongNameB", 2L, ExpectedResult = false )]
    [TestCase( "sameBoolName", true, "sameBoolName", true, ExpectedResult = true )]
    [TestCase( "sameBoolName", true, "sameBoolName", false, ExpectedResult = false )]
    [TestCase( "differentBoolNameA", true, "differentBoolNameB", true, ExpectedResult = false )]
    [TestCase( "differentBoolNameA", true, "differentBoolNameB", false, ExpectedResult = false )]
    public bool EqualityIsByRecordValue<T>( string propertyAName, T propertyAValue, string propertyBName, T propertyBValue ) where T : notnull
    {
        ZfsProperty<T> propertyA =ZfsProperty<T>.CreateWithoutParent( propertyAName, propertyAValue );
        ZfsProperty<T> propertyB = propertyA with { Name = propertyBName, Value = propertyBValue };
        return propertyA == propertyB;
    }

    [Test]
    public void EqualityIsByRecordValueDateTimeOffset( )
    {
        // The same 4 equality tests as the generic version, but DateTimeOffset can't be constant, so we have to set them up individually
        // Also check that a straight copy of propertyA is equal to propertyA by value
        ZfsProperty<DateTimeOffset> propertyA = ZfsProperty<DateTimeOffset>.CreateWithoutParent( "sameDateTimeOffsetName", DateTimeOffset.UnixEpoch );
        ZfsProperty<DateTimeOffset> propertyB = propertyA with { Name = "sameDateTimeOffsetName", Value = DateTimeOffset.UnixEpoch };
        ZfsProperty<DateTimeOffset> propertyC = propertyA with { Name = "differentDateTimeOffsetName", Value = DateTimeOffset.UnixEpoch };
        ZfsProperty<DateTimeOffset> propertyD = propertyA with { Name = "sameDateTimeOffsetName", Value = DateTimeOffset.UnixEpoch.AddHours( -1 ) };
        ZfsProperty<DateTimeOffset> propertyE = propertyA with { Name = "differentDateTimeOffsetName", Value = DateTimeOffset.UnixEpoch.AddHours( -1 ) };
        Assert.Multiple( ( ) =>
        {
            //Don't use Is.EqualTo, because it boxes them
            Assert.That( propertyA == propertyB, Is.True );
            Assert.That( propertyA == propertyA with { }, Is.True );
            Assert.That( propertyA == propertyC, Is.False );
            Assert.That( propertyA == propertyD, Is.False );
            Assert.That( propertyA == propertyE, Is.False );
        } );
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
    //todo: Need to test proper resolution of the source property

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
}
