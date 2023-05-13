using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

namespace Sanoid.Common.Tests;

[TestFixture(Description = "These tests are for basic system-level prerequisites before we even get into bothering with anything specific to Sanoid.net")]
[Order(1)]
[Parallelizable(ParallelScope.Children)]
public class BasicPrerequisiteTests
{
    [Test]
    [Parallelizable(ParallelScope.None)]
    [Order(1)]
    public void CheckPathEnvironmentVariableIsDefined( )
    {
        string? pathVariable = Environment.GetEnvironmentVariable( "PATH" );
        Assert.That( pathVariable, Is.Not.Null);
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    [Order(2)]
    public void CheckPathEnvironmentVariableIsNotEmpty( )
    {
        string pathVariable = Environment.GetEnvironmentVariable( "PATH" )!;
        Assert.That( pathVariable, Is.Not.Empty );
    }

    [Test]
    [Order(3)]
    public void CheckRuntimeVersionIsSupported( )
    {
        string? frameworkName = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        Console.WriteLine( $"Runtime version is {frameworkName}" );
        Assert.Multiple( ( ) =>
        {
            Assert.That( frameworkName, Is.Not.Null );
            Assert.That( frameworkName, Is.Not.Empty );
            Assert.That( frameworkName, Does.Contain( "v7." ) );
            Assert.That( frameworkName, Does.Contain( "v7." ) );
        } );
    }
}
