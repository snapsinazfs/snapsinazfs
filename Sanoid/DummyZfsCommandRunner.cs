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
    public DummyZfsCommandRunner( )
    {
        Logger.Warn( "USING DUMMY ZFS COMMAND RUNNER. NO REAL ZFS COMMANDS WILL BE EXECUTED." );
    }
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( ); 
    public void ZfsSnapshot( IConfigurationSection config, IZfsObject snapshotParent )
    {
        throw new NotImplementedException(  );
    }
}
