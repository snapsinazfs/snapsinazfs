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
    /// <summary>
    ///     Creates a new instance of a dummy command runner that never actually runs ZFS commands.
    /// </summary>
    /// <param name="ignoredConfigurationSection">IGNORED</param>
    public DummyZfsCommandRunner( IConfigurationSection? ignoredConfigurationSection )
    {
        Logger.Warn( "USING DUMMY ZFS COMMAND RUNNER. NO REAL ZFS COMMANDS WILL BE EXECUTED." );
    }
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( ); 
    public void ZfsSnapshot( IConfigurationSection config, IZfsObject snapshotParent )
    {
        throw new NotImplementedException(  );
    }
}
