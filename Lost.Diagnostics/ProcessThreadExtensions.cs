using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Lost.Diagnostics.Win32;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace Lost.Diagnostics
{
	/// <summary>
	/// This class provides extensions for System.Diagnostics.ProcessThread class
	/// </summary>
	public static class ProcessThreadExtensions
	{
		#region Win32
		[DllImport("kernel32.dll")]
		static extern int SuspendThread(IntPtr threadHandle);

		[DllImport("kernel32.dll")]
		static extern int ResumeThread(IntPtr threadHandle);

		[DllImport("kernel32.dll")]
		static extern IntPtr OpenThread(ThreadAccessRights access, bool inheritHandle, int threadID);
		#endregion Win32

		static SafeFileHandle Open(this ProcessThread thread, ThreadAccessRights access)
		{
			if (thread == null) throw new ArgumentNullException("thread");

			IntPtr handle = OpenThread(access, false, thread.Id);

			if (handle == IntPtr.Zero)
				throw new Win32Exception();

			return new SafeFileHandle(handle, true);
		}

		/// <summary>
		/// Suspends thread execution
		/// </summary>
		/// <param name="thread">A thread to suspend</param>
		/// <returns>Pre-call suspend counter</returns>
		public static int Suspend(this ProcessThread thread)
		{
			if (thread == null) throw new ArgumentNullException("thread");

			using(SafeFileHandle handle = thread.Open(ThreadAccessRights.SuspendResume)){
				int suspendCount = SuspendThread(handle.DangerousGetHandle());
				if (suspendCount == -1)
					throw new Win32Exception();
				return suspendCount;
			}
		}


		/// <summary>
		/// Resumes thread execution
		/// </summary>
		/// <param name="thread">A thread to resume</param>
		/// <returns>Pre-call suspend counter</returns>
		public static int Resume(this ProcessThread thread)
		{
			if (thread == null) throw new ArgumentNullException("thread");

			using (SafeFileHandle handle = thread.Open(ThreadAccessRights.SuspendResume)) {
				int suspendCount = ResumeThread(handle.DangerousGetHandle());
				if (suspendCount == -1)
					throw new Win32Exception();
				return suspendCount;
			}
		}
	}
}
