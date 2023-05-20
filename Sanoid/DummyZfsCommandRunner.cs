using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sanoid.Common.Zfs;

namespace Sanoid;

internal class DummyZfsCommandRunner : IZfsCommandRunner
{
    public void ZfsSnapshot( IConfigurationSection config, IZfsObject snapshotParent )
    {
        throw new NotImplementedException(  );
    }
}
