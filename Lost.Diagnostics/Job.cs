using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Lost.Diagnostics.Win32;
using Microsoft.Win32.SafeHandles;

namespace Lost.Diagnostics
{
	/// <summary>
	/// Provides access to job objects, 
	/// enables you to create, track, control and destroy them
	/// </summary>
	[PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[HostProtectionAttribute(SecurityAction.LinkDemand, SharedState = true, Synchronization = true,
		ExternalProcessMgmt = true, SelfAffectingProcessMgmt = true)]
	[PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
	public class Job: Component, IEnumerable<Process>
	{
		public Job(string name = null)
		{
			IntPtr handle = CreateJobObject(IntPtr.Zero, name);
			if (handle == IntPtr.Zero) throw new Win32Exception();
			safeHandle = new SafeFileHandle(handle, true);
		}

		readonly SafeFileHandle safeHandle;

		public IntPtr Handle
		{
			get { return safeHandle.DangerousGetHandle(); }
		}

		#region WinAPI
		[DllImport("kernel32.dll")]
		static extern bool IsProcessInJob(IntPtr processHandle, IntPtr jobHandle, out bool result);

		[DllImport("kernel32.dll")]
		static extern IntPtr CreateJobObject(IntPtr securityAttributes, string name);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

		[DllImport("kernel32.dll")]
		static extern bool TerminateJobObject(IntPtr job, int exitCode);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CreateProcess(string appName, string commandLine, IntPtr processSecurity,
			IntPtr threadSecurity, bool inheritHandles, ProcessCreationFlags flags, IntPtr env, string currentDirectory,
			[In] ref ProcessStartupInfo startInfo, out ProcessInformation processInformation);

		[DllImport("kernel32.dll")]
		static extern uint ResumeThread(IntPtr thread);

		#region Job information
		[DllImport("kernel32.dll", EntryPoint = "QueryInformationJobObject")]
		static extern bool QueryBasicAccounting(IntPtr job, JobInfoClass infoClass, out JobBasicAccounting info,
			int infoBytes, out int resultBytes);

		[DllImport("kernel32.dll", EntryPoint = "QueryInformationJobObject")]
		static extern bool QueryProcessIDs(IntPtr job, JobInfoClass infoClass, ref JobBasicProcessIDs info,
			int infoBytes, out int resultBytes);
		#endregion
		#endregion

		#region Job Info
		#region Processor usage
		/// <summary>
		/// Gets the user processor time for this job
		/// </summary>
		public TimeSpan UserProcessorTime
		{
			get {
				CheckDisposed();

				JobBasicAccounting accounting;
				int resultBytes;
				if (!QueryBasicAccounting(Handle, JobInfoClass.BasicAccounting, 
					out accounting, JobBasicAccounting.Size, out resultBytes))
					throw new Win32Exception();

				if (resultBytes < 8) throw new NotSupportedException("This OS is not supported");
				
				return new TimeSpan(accounting.TotalUserTime);
			}
		}

		/// <summary>
		/// Gets the privileged processor time for this job
		/// </summary>
		public TimeSpan PrivilegedProcessorTime
		{
			get
			{
				CheckDisposed();

				JobBasicAccounting accounting;
				int resultBytes;
				if (!QueryBasicAccounting(Handle, JobInfoClass.BasicAccounting,
					out accounting, JobBasicAccounting.Size, out resultBytes))
					throw new Win32Exception();

				if (resultBytes < 16) throw new NotSupportedException("This OS is not supported");
				
				return new TimeSpan(accounting.TotalKernelTime);
			}
		}

		/// <summary>
		/// Gets the total processor time for this job
		/// </summary>
		public TimeSpan TotalProcessorTime
		{
			get
			{
				CheckDisposed();

				JobBasicAccounting accounting;
				int resultBytes;
				if (!QueryBasicAccounting(Handle, JobInfoClass.BasicAccounting,
					out accounting, JobBasicAccounting.Size, out resultBytes))
					throw new Win32Exception();

				if (resultBytes < 16) throw new NotSupportedException("This OS is not supported");

				return new TimeSpan(accounting.TotalUserTime + accounting.TotalKernelTime);
			}
		}
		#endregion Processor usage

		/// <summary>
		/// Determines whether the job contains the specified process
		/// </summary>
		/// <param name="process">The process to locate in the job</param>
		/// <returns>true if job contains the specified process; otherwise, false</returns>
		public bool Contains(Process process)
		{
			CheckDisposed();

			bool result;

			if (!IsProcessInJob(process.Handle, Handle, out result))
				throw new Win32Exception();

			return result;
		}

		/// <summary>
		/// Determines whether the process belongs to any job
		/// </summary>
		/// <param name="process">The process to determine state,
		/// or null to check current process</param>
		/// <returns>true if process belongs to some group; otherwise, false</returns>
		public static bool IsInJob(Process process = null)
		{
			bool result;

			process = process ?? Process.GetCurrentProcess();

			if (!IsProcessInJob(process.Handle, IntPtr.Zero, out result))
				throw new Win32Exception();

			return result;
		}

		#region Enumerating processes
		/// <summary>
		/// Enumerates all processes that belong to that job
		/// </summary>
		/// <returns>A process enumerator for the job</returns>
		public IEnumerator<Process> GetEnumerator()
		{
			CheckDisposed();

			bool done = false;
			var ids = new JobBasicProcessIDs {
				ListSize = 8,
			};
			do {
				int resultBytes;


				ids.Identifiers = new int[ids.ListSize];
				if (!QueryProcessIDs(Handle, JobInfoClass.ProcessIDs, ref ids,
					JobBasicProcessIDs.Size, out resultBytes))
					throw new Win32Exception();

				done = ids.ListSize <= ids.RealCount;
				if (!done) ids.ListSize *= 2;
			} while (!done);

			return ids.Identifiers.Take(ids.RealCount)
				.Select(Process.GetProcessById).GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#endregion Job Info

		#region Operations
		/// <summary>
		/// Terminates job's execution
		/// </summary>
		/// <param name="exitCode">Optional job's exit code</param>
		public void Terminate(int exitCode = 0)
		{
			CheckDisposed();

			if (!TerminateJobObject(Handle, exitCode))
				throw new Win32Exception();
		}

		/// <summary>
		/// Assigns a process to this job.
		/// </summary>
		/// <param name="process">Process to assign to job.</param>
		/// <returns>false if process was already assigned to this job, 
		/// true on successful assignment</returns>
		public bool Assign(Process process)
		{
			CheckDisposed();

			if (Contains(process)) return false;

			Assign(process.Handle);

			return true;
		}

		/// <summary>
		/// Tries to suspend all the job's processes
		/// </summary>
		/// <param name="maxTries">Maximum number of tries must be taken before suspending fails</param>
		/// <returns>True in case the job was successfully suspended,
		/// false in case new processes or new threads were appearing too fast during tries</returns>
		public bool TrySuspend(int maxTries = int.MaxValue)
		{
			if (maxTries <= 0) throw new ArgumentOutOfRangeException("maxTries");

			var suspended = new HashSet<int>();
			bool newProcesses = false;
			int triesLeft = maxTries;
			do {
				triesLeft--;
				foreach (Process process in this) {
					if (suspended.Contains(process.Id)) continue;

					newProcesses = true;
					if (process.TrySuspend(maxTries)) suspended.Add(process.Id);
				}
			} while (newProcesses && triesLeft >= 0);

			if (newProcesses) {
				foreach (Process process in this) {
					if (suspended.Contains(process.Id))
						process.Resume();
				}

				return false;
			}

			return true;
		}

		/// <summary>
		/// Resumes execution of all job's processes
		/// </summary>
		public void Resume()
		{
			foreach (Process process in this) {
				process.Resume();
			}

			// TODO: rollback states on exception
		}
		#endregion

		#region Starting
		/// <summary>
		/// Starts new process inside the job
		/// </summary>
		/// <param name="startInfo">Process start parameters</param>
		/// <param name="beforeStart">An action that would be executed
		/// between process creation and starting its main thread</param>
		/// <returns></returns>
		public Process Start(ProcessStartInfo startInfo, Action<Process> beforeStart = null)
		{
			CheckStartInfo(startInfo);

			ProcessStartupInfo pstartInfo = PrepareStartupInfo(startInfo);

			ProcessInformation pinfo;
			if (!CreateProcess(null, startInfo.FileName + " " + startInfo.Arguments,
				IntPtr.Zero, IntPtr.Zero, false,
				ProcessCreationFlags.Suspended | ProcessCreationFlags.OutsideJob,
				IntPtr.Zero,
				string.IsNullOrEmpty(startInfo.WorkingDirectory)
					? null: startInfo.WorkingDirectory,
				ref pstartInfo, out pinfo))
				throw new Win32Exception();

			Assign(pinfo.ProcessHandle);

			var process = Process.GetProcessById((int)pinfo.ProcessID);
			if (beforeStart != null) beforeStart(process);

			if (ResumeThread(pinfo.MainThreadHandle) == uint.MaxValue)
				throw new Win32Exception();

			return process;
		}

		private ProcessStartupInfo PrepareStartupInfo(ProcessStartInfo startInfo)
		{
			var pstartInfo = new ProcessStartupInfo {
				Flags = StartupInfoFlags.None,
			};
			pstartInfo.AdjustSize();
			return pstartInfo;
		}

		private void CheckStartInfo(ProcessStartInfo startInfo)
		{
			if (startInfo.UseShellExecute)
				throw new ArgumentException("Can't start a process within a job using ShellExecute");
			if (startInfo.CreateNoWindow) throw new NotImplementedException("CreateNoWindow");
			if (!string.IsNullOrEmpty(startInfo.UserName))
				throw new NotImplementedException("Can't start process within a job under different credentials");
			if (startInfo.RedirectStandardError || startInfo.RedirectStandardInput
				|| startInfo.RedirectStandardOutput)
				throw new NotImplementedException("Redirect STD IO");
			if (startInfo.WindowStyle != ProcessWindowStyle.Normal)
				throw new NotImplementedException("WindowStyle");
		}

		private void Assign(IntPtr processHandle)
		{
			if (!AssignProcessToJobObject(Handle, processHandle))
				throw new Win32Exception();
		}
		#endregion Starting

		#region Disposing
		private void CheckDisposed()
		{
			if (safeHandle.IsClosed)
				throw new ObjectDisposedException("Job");
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!safeHandle.IsClosed)
				safeHandle.Close();
		}
		#endregion
	}
}
