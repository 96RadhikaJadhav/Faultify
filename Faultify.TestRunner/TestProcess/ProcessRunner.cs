﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faultify.TestRunner.Shared;

namespace Faultify.TestRunner.TestProcess
{
    /// <summary>
    ///     Async process runner.
    /// </summary>
    public class ProcessRunner
    {
        private readonly ProcessStartInfo _processStartInfo;
        public StringBuilder Output;
        public StringBuilder Error;

        public ProcessRunner(ProcessStartInfo processStartInfo)
        {
            _processStartInfo = processStartInfo;
        }

        public async Task<Process> RunAsync(CancellationToken cancellationToken)
        {
            var process = new Process();

            var cancellationTokenRegistration = cancellationToken.Register(() => { process.Kill(true); });

            var taskCompletionSource = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (o, e) => { taskCompletionSource.TrySetResult(null); };
            process.StartInfo = _processStartInfo;

            Output = new StringBuilder();
            Error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                Output.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                Error.AppendLine(e.Data);
            };
            
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await taskCompletionSource.Task;
            await cancellationTokenRegistration.DisposeAsync();

            process.WaitForExit();

            return process;
        }
    }
}