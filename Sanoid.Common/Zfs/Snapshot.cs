using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanoid.Common.Zfs;
/// <summary>
/// A ZFS snapshot
/// </summary>
public class Snapshot : IZfsObject
{
    public Snapshot( string name, IZfsObject parent )
    {
        Name = name;
        Parent = parent;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IZfsObject? Parent { get; }
}
