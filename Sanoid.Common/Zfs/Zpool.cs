using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sanoid.Common.Zfs;
internal class Zpool : IZfsObject
{
    public Zpool( string name )
    {
        Name = name;
    }

    public string Name { get; set; }
    public IZfsObject? Parent => null;
}
