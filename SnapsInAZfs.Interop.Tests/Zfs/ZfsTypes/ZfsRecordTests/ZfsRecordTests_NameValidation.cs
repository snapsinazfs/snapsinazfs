// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using SnapsInAZfs.Interop.Zfs.ZfsTypes;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[TestFixture]
[Order( 20 )]
[Category( "General" )]
[FixtureLifeCycle( LifeCycle.SingleInstance )]
[NonParallelizable]
[TestOf( typeof( ZfsRecord ) )]
public class ZfsRecordTests_NameValidation
{
    // ReSharper disable StringLiteralTypo
    [Test]
    [TestCase( "OTRFlFY1", true )]
    [TestCase( ":pqAJA.1", true )]
    [TestCase( "W.LrTV.1", true )]
    [TestCase( "ND_MEek1", true )]
    [TestCase( "q.JQbvt1", true )]
    [TestCase( "COgpYSs1", true )]
    [TestCase( "OTRFlFY1/imwOIBk2", true )]
    [TestCase( ":pqAJA.1/XUPWMIm2", true )]
    [TestCase( "W.LrTV.1/YYeUjKU2", true )]
    [TestCase( "ND_MEek1/m.AKOTf2", true )]
    [TestCase( "q.JQbvt1/OOhYkAE2", true )]
    [TestCase( "COgpYSs1/sLAUTSQ2", true )]
    [TestCase( "OTRFlFY1/imwOIBk2/_qDLsqT3", true )]
    [TestCase( ":pqAJA.1/XUPWMIm2/mkdIMJZ3", true )]
    [TestCase( "W.LrTV.1/YYeUjKU2/m_dYLfi3", true )]
    [TestCase( "ND_MEek1/m.AKOTf2/fI-BiLF3", true )]
    [TestCase( "q.JQbvt1/OOhYkAE2/mIauVdp3", true )]
    [TestCase( "COgpYSs1/sLAUTSQ2/N-.vDpC3", true )]
    [TestCase( "OTRFlFY1/imwOIBk2/_qDLsqT3/DdpBVIf4", true )]
    [TestCase( ":pqAJA.1/XUPWMIm2/mkdIMJZ3/Pw_hNIU4", true )]
    [TestCase( "W.LrTV.1/YYeUjKU2/m_dYLfi3/MFjkIo.4", true )]
    [TestCase( "ND_MEek1/m.AKOTf2/fI-BiLF3/FdoVFKC4", true )]
    [TestCase( "q.JQbvt1/OOhYkAE2/mIauVdp3/:eCiTJG4", true )]
    [TestCase( "COgpYSs1/sLAUTSQ2/N-.vDpC3/EL.qdAB4", true )]
    [TestCase( "OTRFlFY1/imwOIBk2/_qDLsqT3/DdpBVIf4/memizzb5", true )]
    [TestCase( ":pqAJA.1/XUPWMIm2/mkdIMJZ3/Pw_hNIU4/vTlZbC-5", true )]
    [TestCase( "W.LrTV.1/YYeUjKU2/m_dYLfi3/MFjkIo.4/.aKp:GR5", true )]
    [TestCase( "ND_MEek1/m.AKOTf2/fI-BiLF3/FdoVFKC4/sx:X:oh5", true )]
    [TestCase( "q.JQbvt1/OOhYkAE2/mIauVdp3/:eCiTJG4/YjXERCt5", true )]
    [TestCase( "COgpYSs1/sLAUTSQ2/N-.vDpC3/EL.qdAB4/.bMgkBG5", true )]
    [TestCase( "OTRûFlFY", false, Description = "Illegal character at this level is û at index 3" )]
    [TestCase( "pqAJA.ÿb", false, Description = "Illegal character at this level is ÿ at index 6" )]
    [TestCase( "LrTVò.bN", false, Description = "Illegal character at this level is ò at index 4" )]
    [TestCase( "MEekzÐq.", false, Description = "Illegal character at this level is Ð at index 5" )]
    [TestCase( "bvít-COg", false, Description = "Illegal character at this level is í at index 2" )]
    [TestCase( "Sszi™mwO", false, Description = "Illegal character at this level is ™ at index 4" )]
    [TestCase( "OTRFlFY1/kXUP+WM1", false, Description = "Illegal character at this level is + at index 13" )]
    [TestCase( ":pqAJA.1/YYeUjáK1", false, Description = "Illegal character at this level is á at index 14" )]
    [TestCase( "W.LrTV.1/.AKOTÇf1", false, Description = "Illegal character at this level is Ç at index 14" )]
    [TestCase( "ND_MEek1/hYkA•Es1", false, Description = "Illegal character at this level is • at index 13" )]
    [TestCase( "q.JQbvt1/UTS¼Q_q1", false, Description = "Illegal character at this level is ¼ at index 12" )]
    [TestCase( "COgpYSs1/sqTmÂkd1", false, Description = "Illegal character at this level is Â at index 13" )]
    [TestCase( "OTRFlFY1/imwOIBk2/JZm_§dY3", false, Description = "Illegal character at this level is § at index 22" )]
    [TestCase( ":pqAJA.1/XUPWMIm2/ifI-©Bi3", false, Description = "Illegal character at this level is © at index 22" )]
    [TestCase( "W.LrTV.1/YYeUjKU2/mÄIauVd3", false, Description = "Illegal character at this level is Ä at index 19" )]
    [TestCase( "ND_MEek1/m.AKOTf2/-.v|DpC3", false, Description = "Illegal character at this level is | at index 21" )]
    [TestCase( "q.JQbvt1/OOhYkAE2/pBòVIfP3", false, Description = "Illegal character at this level is ò at index 20" )]
    [TestCase( "COgpYSs1/sLAUTSQ2/h$NIUMF3", false, Description = "Illegal character at this level is $ at index 19" )]
    [TestCase( "OTRFlFY1/imwOIBk2/_qDLsqT3/Io.Fdño4", false, Description = "Illegal character at this level is ñ at index 32" )]
    [TestCase( ":pqAJA.1/XUPWMIm2/mkdIMJZ3/KC:eCüi4", false, Description = "Illegal character at this level is ü at index 32" )]
    [TestCase( "W.LrTV.1/YYeUjKU2/m_dYLfi3/GELæ.qd4", false, Description = "Illegal character at this level is æ at index 30" )]
    [TestCase( "ND_MEek1/m.AKOTf2/fI-BiLF3/émemizz4", false, Description = "Illegal character at this level is é at index 27" )]
    [TestCase( "q.JQbvt1/OOhYkAE2/mIauVdp3/TlZbC-ó4", false, Description = "Illegal character at this level is ó at index 33" )]
    [TestCase( "COgpYSs1/sLAUTSQ2/N-.vDpC3/Kpú:GRs4", false, Description = "Illegal character at this level is ú at index 29" )]
    [TestCase( "OTRFlFY1/imwOIBk2/_qDLsqT3/DdpBVIf4/X:ohYjø5", false, Description = "Illegal character at this level is ø at index 42" )]
    [TestCase( ":pqAJA.1/XUPWMIm2/mkdIMJZ3/Pw_hNIU4/&RCt.bM5", false, Description = "Illegal character at this level is & at index 36" )]
    [TestCase( "W.LrTV.1/YYeUjKU2/m_dYLfi3/MFjkIo.4/²BGornR5", false, Description = "Illegal character at this level is ² at index 36" )]
    [TestCase( "ND_MEek1/m.AKOTf2/fI-BiLF3/FdoVFKC4/FKtbêfX5", false, Description = "Illegal character at this level is ê at index 40" )]
    [TestCase( "q.JQbvt1/OOhYkAE2/mIauVdp3/:eCiTJG4/Koo·AkX5", false, Description = "Illegal character at this level is · at index 39" )]
    [TestCase( "COgpYSs1/sLAUTSQ2/N-.vDpC3/EL.qdAB4/run.Iaå5", false, Description = "Illegal character at this level is å at index 42" )]
    // ReSharper restore StringLiteralTypo
    [Category( "General" )]
    [Category( "ZFS" )]
    public void CheckDatasetNameValidation( string name, bool valid )
    {
        Assume.That( name, Has.Length.LessThan( 255 ) );
        if ( name.Length >= 255 )
        {
            Assert.Ignore( "Total path depth requested would exceed valid ZFS identifier length (255)" );
        }

        string nameToTest = name;
        bool fsValidationResult = ZfsRecord.ValidateName( ZfsPropertyValueConstants.FileSystem, nameToTest ) == valid;
        bool volValidationResult = ZfsRecord.ValidateName( ZfsPropertyValueConstants.Volume, nameToTest ) == valid;
        Assert.Multiple( ( ) =>
        {
            Assert.That( fsValidationResult, Is.True );
            Assert.That( volValidationResult, Is.True );
        } );
    }

    [Test]
    [Category( "General" )]
    [Category( "ZFS" )]
    [Category( "TypeChecks" )]
    public void CheckNameValidationThrowsOnNameTooLong( [Values( 256, 512 )] int pathLength, [Range( 1, 5 )] int pathDepth )
    {
        string path = $"{ZfsRecordTestHelpers.GetValidZfsPathComponent( pathLength / pathDepth )}";
        for ( int i = 1; i < pathDepth; i++ )
        {
            path = $"{path}/{ZfsRecordTestHelpers.GetValidZfsPathComponent( pathLength / pathDepth )}";
        }

        Assert.That( ( ) => { ZfsRecord.ValidateName( ZfsPropertyValueConstants.FileSystem, path ); }, Throws.TypeOf<ArgumentOutOfRangeException>( ) );
    }

    [Test]
    [TestCaseSource( typeof( ZfsRecordTestHelpers ), nameof( ZfsRecordTestHelpers.GetValidSnapshotCases ), new object?[] { 8, 12, 5 } )]
    [TestCaseSource( typeof( ZfsRecordTestHelpers ), nameof( ZfsRecordTestHelpers.GetIllegalSnapshotCases ), new object?[] { 8, 12, 5 } )]
    [Category( "General" )]
    [Category( "ZFS" )]
    public void CheckSnapshotNameValidation( NameValidationTestCase testCase )
    {
        if ( testCase.Name.Length >= 255 )
        {
            Assert.Ignore( "Total path depth requested would exceed valid ZFS identifier length (255)" );
        }

        string nameToTest = testCase.Name;
        bool snapshotValidationResult = ZfsRecord.ValidateName( ZfsPropertyValueConstants.Snapshot, nameToTest ) == testCase.Valid;
        Assert.That( snapshotValidationResult, Is.True );
    }
}
