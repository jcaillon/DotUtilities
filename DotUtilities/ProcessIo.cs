#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProcessIo.cs) is part of DotUtilities.
//
// DotUtilities is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// DotUtilities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with DotUtilities. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DotUtilities.Extensions;
using DotUtilities.Process;

namespace DotUtilities {

    /// <summary>
    /// Wrapper around process.
    /// </summary>
    public class ProcessIo {

        /// <summary>
        /// The full path to the executable used
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Subscribe to this event called when the process exits
        /// </summary>
        public event EventHandler<EventArgs> OnProcessExit;

        /// <summary>
        /// Event called when the process received data from the output or standard stream.
        /// <see cref="RedirectOutput"/> must be activated.
        /// </summary>
        public event EventHandler<ProcessOutputEventArgs> OnOutputReceived;

        /// <summary>
        /// The working directory to use for this process
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Choose to redirect the standard/error output or no, default to true
        /// </summary>
        public bool RedirectOutput { get; set; } = true;

        /// <summary>
        /// Choose the encoding for the standard/error output
        /// </summary>
        public Encoding RedirectedOutputEncoding { get; set; }

        /// <summary>
        /// A dictionnary containing key values for environment variables to pass to the process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// The user that will start the process (defaults to current if null).
        /// </summary>
        public ProcessOwner ProcessOwner { get; set; }

        /// <summary>
        /// Cancellation token.
        /// </summary>
        public CancellationToken? CancelToken {
            get => _cancelToken;
            set {
                _cancelToken = value;
                if (_cancelRegistration == null) {
                    _cancelRegistration = _cancelToken?.Register(OnCancellation);
                }
            }
        }

        /// <summary>
        /// A logger.
        /// </summary>
        public ILog Log { get; set; }

        /// <summary>
        /// Use <see cref="string.Trim()"/> on each output line in standard and error string output.
        /// <see cref="StandardOutputArray"/> and <see cref="ErrorOutputArray"/> still have the original value.
        /// </summary>
        public bool TrimOutputLine { get; set; }

        /// <summary>
        /// Log each line of the standard output in debug level in <see cref="Log"/>.
        /// </summary>
        public bool LogStandardOutput { get; set; } = true;

        /// <summary>
        /// Log each line of the standard output in debug level in <see cref="Log"/>.
        /// </summary>
        public bool LogErrorOutput { get; set; } = true;

        /// <summary>
        /// Standard output, to be called after the process exits
        /// </summary>
        public StringBuilder StandardOutput {
            get {
                if (_standardOutput == null || _process != null && !_process.HasExited) {
                    _standardOutput = new StringBuilder();
                    foreach (var s in StandardOutputArray) {
                        _standardOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                    }
                    _standardOutput.TrimEnd();
                }
                return _standardOutput;
            }
        }

        /// <summary>
        /// Error output, to be called after the process exits
        /// </summary>
        public StringBuilder ErrorOutput {
            get {
                if (_errorOutput == null || _process != null && !_process.HasExited) {
                    _errorOutput = new StringBuilder();
                    foreach (var s in ErrorOutputArray) {
                        _errorOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                    }
                    _errorOutput.TrimEnd();
                }
                return _errorOutput;
            }
        }

        /// <summary>
        /// Returns all the messages sent to the standard or error output, should be used once the process has exited
        /// </summary>
        public StringBuilder BatchOutput {
            get {
                if (_batchModeOutput == null || _process != null && !_process.HasExited) {
                    _batchModeOutput = new StringBuilder();
                    foreach (var s in ErrorOutputArray) {
                        _batchModeOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                    }
                    _batchModeOutput.TrimEnd();

                    foreach (var s in StandardOutputArray) {
                        _batchModeOutput.AppendLine(TrimOutputLine ? s.Trim() : s);
                    }
                    _batchModeOutput.TrimEnd();
                }
                return _batchModeOutput;
            }
        }

        /// <summary>
        /// Returns all the messages sent to the standard or error output, should be used once the process has exited
        /// </summary>
        public string BatchOutputString => _batchOutputString ?? (_batchOutputString = BatchOutput.ToString());

        /// <summary>
        /// Standard output, to be called after the process exits
        /// </summary>
        public List<string> StandardOutputArray { get; private set; } = new List<string>();

        /// <summary>
        /// Error output, to be called after the process exits
        /// </summary>
        public List<string> ErrorOutputArray { get; private set; } = new List<string>();

        /// <summary>
        /// Returns the command line used for the execution.
        /// </summary>
        public string ExecutedCommandLine => $"{ProcessArgs.ToCliArg(ExecutablePath)} {UsedArguments}";

        /// <summary>
        /// The complete arguments used to start the process.
        /// </summary>
        public string UsedArguments => _startInfo?.Arguments;

        private int? _exitCode;

        /// <summary>
        /// Exit code of the process
        /// </summary>
        public int ExitCode {
            get {
                if (!_exitCode.HasValue && _process != null) {
                    _process.WaitForExit();
                    _exitCode = _process.ExitCode;
                }
                return _exitCode ?? 0;
            }
            set { _exitCode = value; }
        }


        /// <summary>
        /// Whether or not this process has been killed
        /// </summary>
        public bool Killed { get; private set; }

        /// <inheritdoc cref="ProcessStartInfo"/>
        protected ProcessStartInfo _startInfo;

        /// <inheritdoc cref="Process"/>
        protected System.Diagnostics.Process _process;

        private string _batchOutputString;
        private StringBuilder _standardOutput;
        private StringBuilder _batchModeOutput;
        private StringBuilder _errorOutput;

        private bool _exitedEventPublished;
        private CancellationTokenRegistration? _cancelRegistration;
        private CancellationToken? _cancelToken;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessIo(string executablePath) {
            ExecutablePath = executablePath;
        }

        /// <summary>
        /// Start the process synchronously, catch the exceptions
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="silent"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public virtual bool TryExecute(ProcessArgs arguments = null, bool silent = true, int timeoutMs = 0) {
            try {
                return Execute(arguments, silent, timeoutMs) && ErrorOutputArray.Count == 0;
            } catch (Exception e) {
                ErrorOutputArray.Add(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Start the process synchronously
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="silent"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public virtual bool Execute(ProcessArgs arguments = null, bool silent = true, int timeoutMs = 0) {
            ExecuteNoWaitInternal(arguments, silent);

            if (!WaitForExitInternal(timeoutMs)) {
                return false;
            }

            Log?.Debug($"Program exit code: {ExitCode}.");

            return ExitCode == 0;
        }

        /// <summary>
        /// Start the process but does not wait for its ending.
        /// Wait for the end with <see cref="WaitForExitInternal"/> or use the <see cref="OnProcessExit"/> event to know when the process is done.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="silent"></param>
        protected virtual void ExecuteNoWaitInternal(ProcessArgs arguments = null, bool silent = true) {
            PrepareStart(arguments, silent);

            Log?.Debug($"Executing program:\n{ExecutedCommandLine}");

            _process.Start();

            if (RedirectOutput) {
                // Asynchronously read the standard output of the spawned process
                // This raises OutputDataReceived events for each line of output
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
        }

        private void OnCancellation() {
            if (_process != null) {
                Kill();
            }
        }

        /// <summary>
        /// Kill the process
        /// </summary>
        public void Kill() {
            Killed = true;
            if (!_process?.HasExited ?? false) {
                _process?.Kill();
            }
        }

        /// <summary>
        /// Wait for a process to end
        /// Returns true if the process has exited (can be false if timeout was reached)
        /// </summary>
        /// <param name="timeoutMs"></param>
        protected virtual bool WaitForExitInternal(int timeoutMs) {
            if (_process == null) {
                return true;
            }

            if (timeoutMs > 0) {
                var exited = _process.WaitForExit(timeoutMs);
                if (!exited) {
                    return false;
                }
            } else {
                _process.WaitForExit();
            }

            ExitCode = _process.ExitCode;

            _process?.Close();
            _process?.Dispose();
            _process = null;

            return true;
        }

        /// <summary>
        /// Prepare the execution of the process.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="silent"></param>
        protected virtual void PrepareStart(ProcessArgs arguments, bool silent) {
            _exitedEventPublished = false;
            StandardOutputArray.Clear();
            _standardOutput = null;
            ErrorOutputArray.Clear();
            _errorOutput = null;
            _batchModeOutput = null;
            _batchOutputString = null;
            Killed = false;
            ExitCode = 0;

            _startInfo = new ProcessStartInfo {
                FileName = ExecutablePath,
                UseShellExecute = false
            };

            if (EnvironmentVariables != null) {
                foreach (var variable in EnvironmentVariables) {
                    if (_startInfo.EnvironmentVariables.ContainsKey(variable.Key)) {
                        _startInfo.EnvironmentVariables[variable.Key] = variable.Value;
                    } else {
                        _startInfo.EnvironmentVariables.Add(variable.Key, variable.Value);
                    }
                }
            }

            if (ProcessOwner != null) {
                _startInfo.Domain = ProcessOwner.DomainName;
                _startInfo.UserName = ProcessOwner.UserName;
                _startInfo.Password = ProcessOwner.Password;
            }

            if (arguments != null) {
                _startInfo.Arguments = arguments.ToCliArgs();
            }

            if (!string.IsNullOrEmpty(WorkingDirectory)) {
                _startInfo.WorkingDirectory = WorkingDirectory;
            }

            if (silent) {
                _startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _startInfo.CreateNoWindow = true;
            }

            if (RedirectOutput) {
                _startInfo.RedirectStandardError = true;
                _startInfo.RedirectStandardOutput = true;
                if (RedirectedOutputEncoding != null) {
                    _startInfo.StandardErrorEncoding = RedirectedOutputEncoding;
                    _startInfo.StandardOutputEncoding = RedirectedOutputEncoding;
                }
            }

            _process = new System.Diagnostics.Process {
                StartInfo = _startInfo
            };

            if (RedirectOutput) {
                _process.OutputDataReceived += OnProcessOnOutputDataReceived;
                _process.ErrorDataReceived += OnProcessOnErrorDataReceived;
            }

            if (OnProcessExit != null) {
                _process.EnableRaisingEvents = true;
                _process.Exited += ProcessOnExited;
            }
        }

        /// <summary>
        /// Called when the process writes in the error stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnProcessOnErrorDataReceived(object sender, DataReceivedEventArgs args) {
            if (!string.IsNullOrEmpty(args.Data)) {
                OnOutputReceived?.Invoke(this, new ProcessOutputEventArgs(args.Data, true));
                ErrorOutputArray.Add(args.Data);
                if (Log != null && LogErrorOutput) {
                    var line = TrimOutputLine ? args.Data?.Trim() : args.Data;
                    if (!string.IsNullOrEmpty(line)) {
                        Log.Debug(line);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the process writes in the standard output stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args) {
            if (!string.IsNullOrEmpty(args.Data)) {
                OnOutputReceived?.Invoke(this, new ProcessOutputEventArgs(args.Data, false));
                StandardOutputArray.Add(args.Data);
                if (Log != null && LogStandardOutput) {
                    var line = TrimOutputLine ? args.Data?.Trim() : args.Data;
                    if (!string.IsNullOrEmpty(line)) {
                        Log.Debug(line);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the process exits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ProcessOnExited(object sender, EventArgs e) {
            if (!_exitedEventPublished) {
                // this boolean does not seem useful but i have seen weird behaviors where the
                // exited event is called twice when we WaitForExit(), better safe than sorry
                _exitedEventPublished = true;
                _cancelRegistration?.Dispose();
                _cancelRegistration = null;
                OnProcessExit?.Invoke(this, e);
            }
        }
    }
}
