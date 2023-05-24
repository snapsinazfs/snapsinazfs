using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanoid.Interop.Concurrency;
public static class Mutexes
{
    private static Mutex? _sanoidMutex;
    public static Mutex GetSanoidMutex(out bool createdNew )
    {
        createdNew = false;
        return _sanoidMutex ??= new( true, "Global\\Sanoid.net", out createdNew );
    }
}
