// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

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
        ZfsProperty<T> propertyA = new( propertyAName, propertyAValue, ZfsPropertySourceConstants.Local );
        ZfsProperty<T> propertyB = propertyA with { Name = propertyBName, Value = propertyBValue };
        return propertyA == propertyB;
    }

    [Test]
    public void EqualityIsByRecordValueDateTimeOffset( )
    {
        // The same 4 equality tests as the generic version, but DateTimeOffset can't be constant, so we have to set them up individually
        // Also check that a straight copy of propertyA is equal to propertyA by value
        ZfsProperty<DateTimeOffset> propertyA = new( "sameDateTimeOffsetName", DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
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
    [TestCase( ZfsPropertySourceConstants.Local, ExpectedResult = false )]
    [TestCase( ZfsPropertySourceConstants.SnapsInAZfs, ExpectedResult = false )]
    [TestCase( ZfsPropertySourceConstants.Default, ExpectedResult = false )]
    [TestCase( ZfsPropertySourceConstants.None, ExpectedResult = false )]
    [TestCase( ZfsPropertySourceConstants.Received, ExpectedResult = false )]
    [TestCase( "inherited from somewhere", ExpectedResult = true )]
    [TestCase( "bogus source", ExpectedResult = false )]
    public bool IsInherited_AsExpected( string propertySource )
    {
        ZfsProperty<string> property = new( "unimportantName", "unimportantValue", propertySource );
        return property.IsInherited;
    }

    [Test]
    [TestCase( ZfsPropertySourceConstants.Local, ExpectedResult = true )]
    [TestCase( ZfsPropertySourceConstants.SnapsInAZfs, ExpectedResult = false )]
    [TestCase( ZfsPropertySourceConstants.Default, ExpectedResult = false )]
    [TestCase( ZfsPropertySourceConstants.None, ExpectedResult = false )]
    [TestCase( ZfsPropertySourceConstants.Received, ExpectedResult = false )]
    [TestCase( "inherited from somewhere", ExpectedResult = false )]
    [TestCase( "bogus source", ExpectedResult = false )]
    public bool IsLocal_AsExpected( string propertySource )
    {
        ZfsProperty<string> property = new( "unimportantName", "unimportantValue", propertySource );
        return property.IsLocal;
    }

    [Test]
    [TestCase( ZfsPropertyNames.EnabledPropertyName, ExpectedResult = true )]
    [TestCase( ZfsNativePropertyNames.Available, ExpectedResult = false )]
    public bool IsSnapsInAZfsProperty_CanDifferentiateProperties( string propertyName )
    {
        ZfsProperty<string> property = new( propertyName, "test value", ZfsPropertySourceConstants.Local );
        return property.IsSnapsInAZfsProperty;
    }

    [Test]
    [TestCase( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName )]
    [TestCase( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName )]
    [TestCase( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName )]
    [TestCase( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName )]
    [TestCase( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName )]
    [TestCase( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName )]
    [TestCase( ZfsPropertyNames.EnabledPropertyName )]
    [TestCase( ZfsPropertyNames.PruneSnapshotsPropertyName )]
    [TestCase( ZfsPropertyNames.RecursionPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotNamePropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotPeriodPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionDailyPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName )]
    [TestCase( ZfsPropertyNames.SnapshotTimestampPropertyName )]
    [TestCase( ZfsPropertyNames.TakeSnapshotsPropertyName )]
    [TestCase( ZfsPropertyNames.TemplatePropertyName )]
    public void IsSnapsInAZfsProperty_CanIdentifyKnownSiazProperties( string propertyName )
    {
        ZfsProperty<string> siazProperty = new( propertyName, "test value", ZfsPropertySourceConstants.Local );
        Assert.That( siazProperty.IsSnapsInAZfsProperty, Is.True );
    }

    [Test]
    public void ReferenceEqualityOfInterfaceFalse( )
    {
        ZfsProperty<DateTimeOffset> propertyA = new( "sameDateTimeOffsetName", DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
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
    public bool TryParseBool( string value )
    {
        RawProperty input = new( ZfsPropertyNames.EnabledPropertyName, value, ZfsPropertySourceConstants.Local );
        bool success = ZfsProperty<bool>.TryParse( input, out ZfsProperty<bool>? property );
        Assert.That( success, Is.True );
        Assert.That( property, Is.Not.Null );
        Assert.That( property, Is.InstanceOf<ZfsProperty<bool>>( ) );
        Assert.That( property.HasValue, Is.True );
        return property!.Value.Value;
    }
}
