using System;

namespace Sanoid.Common;

public static class BaseClassExtensions
{
    public static string ToSnapshotDateTimeString(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd_HH\\:mm\\:ss");
    }
}