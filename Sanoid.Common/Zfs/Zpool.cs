using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Sanoid.Common.Configuration.Templates;

namespace Sanoid.Common.Zfs;
internal class Zpool : IZfsObject
{
    public Zpool( string name )
    {
        Name = name;
    }

    public string Name { get; set; }
    public IZfsObject? Parent => null;
    public Template? Template { get; set; }
}
