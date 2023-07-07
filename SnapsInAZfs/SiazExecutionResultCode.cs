// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

namespace SnapsInAZfs;

internal enum SiazExecutionResultCode
{
    None = 0,
    Completed,
    CancelledByToken,
    ConfigConsole_CleanExit,
    ZfsPropertyCheck_AllPropertiesPresent,
    ZfsPropertyCheck_MissingProperties,
    ZfsPropertyCheck_MissingProperties_Fatal,
    ZfsPropertyUpdate_Succeeded,
    ZfsPropertyUpdate_Failed
}
