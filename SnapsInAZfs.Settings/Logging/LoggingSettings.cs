﻿// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

namespace SnapsInAZfs.Settings.Logging;

/// <summary>
///     Configuration for logging using NLog
/// </summary>
public static class LoggingSettings
{
    /// <summary>
    ///     Configures NLog using SnapsInAZfs.nlog.json
    /// </summary>
    public static void ConfigureLogger( )
    {
#pragma warning disable CA2000
        IConfigurationRoot nlogJsonConfigRoot = new ConfigurationManager( )
                                            #if WINDOWS
                                                .AddJsonFile("SnapsInAZfs.nlog.json", true, false)
                                            #else
                                                .AddJsonFile( "/usr/local/share/SnapsInAZfs/SnapsInAZfs.nlog.json", false, false )
                                                .AddJsonFile( "/etc/SnapsInAZfs/SnapsInAZfs.nlog.json", true, true )
                                            #endif
                                                .Build( );
#pragma warning restore CA2000
        LogManager.Configuration = new NLogLoggingConfiguration( nlogJsonConfigRoot.GetSection( "NLog" ) );
    }
    public static void OverrideConsoleLoggingLevel( LogLevel level )
    {
        if ( LogManager.Configuration == null )
        {
            return;
        }

        for ( int ruleIndex = 0; ruleIndex < LogManager.Configuration.LoggingRules.Count; ruleIndex++ )
        {
            LoggingRule? rule = LogManager.Configuration.LoggingRules[ ruleIndex ];
            rule?.SetLoggingLevels( level, LogLevel.Off );
        }
    }
}



