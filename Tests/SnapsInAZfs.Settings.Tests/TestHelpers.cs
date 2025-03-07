using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapsInAZfs.Settings.Tests;
internal static class TestHelpers
{
    internal static IEnumerable<string> StandardEmptyAndWhitespaceStringTestInputs = [ string.Empty, " ", "\t", "\n", "\r", "\r\n" ];
}
