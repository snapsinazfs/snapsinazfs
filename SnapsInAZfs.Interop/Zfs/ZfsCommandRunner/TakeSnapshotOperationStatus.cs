// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;

[Flags]
public enum TakeSnapshotOperationStatus
{
    Success = 0,
    Failure = 1,
    DryRun = 2,
    NameValidationFailed = 4 | Failure,
    ZfsProcessFailure = 8 | Failure
}
