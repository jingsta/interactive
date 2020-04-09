﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.DotNet.Interactive.App.CommandLine;
using Xunit;

namespace Microsoft.DotNet.Interactive.App.Tests
{
    public class JupyterInstallCommandTests
    {
        [Fact]
        public async Task Appends_http_port_range_arguments()
        {
            var console = new TestConsole();
            var kernelSpec = new InMemoryJupyterKernelSpecInstaller(true, null);
            var jupyterCommandLine = new JupyterInstallCommand(console, kernelSpec, new PortRange(100, 400));

            await jupyterCommandLine.InvokeAsync();

            kernelSpec.InstalledKernelSpecs
                .Should()
                .HaveCount(3)
                .And
                .Match(s => s.All(k => k.Contains("--http-port-range")));

        }

        [Fact]
        public async Task Returns_error_when_jupyter_paths_could_not_be_obtained()
        {
            var console = new TestConsole();
            var installCommand = new JupyterInstallCommand(
                console,
                new InMemoryJupyterKernelSpecInstaller(false, message: "Could not find jupyter kernelspec module"));

            await installCommand.InvokeAsync();
            var consoleError = console.Error.ToString();
            using var scope = new AssertionScope();
            consoleError.Should().Contain("Failed to install \".NET (F#)\" kernel. The error was: Could not find jupyter kernelspec module.");
            consoleError.Should().Contain("Failed to install \".NET (C#)\" kernel. The error was: Could not find jupyter kernelspec module.");
            consoleError.Should().Contain("Failed to install \".NET (PowerShell)\" kernel. The error was: Could not find jupyter kernelspec module.");
        }

        [Fact]
        public async Task Prints_to_console_when_kernel_installation_succeeded()
        {
            var console = new TestConsole();
            var jupyterCommandLine = new JupyterInstallCommand(console, new InMemoryJupyterKernelSpecInstaller(true, null));

            await jupyterCommandLine.InvokeAsync();

            var consoleOut = console.Out.ToString();

            using var scope = new AssertionScope();

            consoleOut.Should().MatchEquivalentOf("*[InstallKernelSpec] Installed kernelspec .net-csharp in *.net-csharp*");
            consoleOut.Should().MatchEquivalentOf("*[InstallKernelSpec] Installed kernelspec .net-fsharp in *.net-fsharp*");
            consoleOut.Should().Contain("Installed \".NET (F#)\" kernel.");
            consoleOut.Should().Contain("Installed \".NET (C#)\" kernel.");
        }
    }
}