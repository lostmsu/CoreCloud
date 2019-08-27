using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lost.Diagnostics
{
	/// <summary>
	/// This class provides extensions for System.Diagnostics.Process class
	/// </summary>
	public static class ProcessExtensions
	{
		/// <summary>
		/// Tries to suspend all the process's threads
		/// </summary>
		/// <param name="process">A process to suspend</param>
		/// <param name="maxTries">Maximum number of tries must be taken before suspending fails</param>
		/// <returns>True if process was successfully suspended,
		/// false in case new threads were appearing to fast during tries</returns>
		public static bool TrySuspend(this Process process, int maxTries = int.MaxValue)
		{
			if (process == null) throw new ArgumentNullException("process");
			if (maxTries <= 0) throw new ArgumentOutOfRangeException("maxTries");

			var suspended = new HashSet<int>();
			bool newThreads = false;
			int triesLeft = maxTries;
			do {
				newThreads = false;
				triesLeft--;
				foreach (ProcessThread thread in process.Threads) {
					if (suspended.Contains(thread.Id)) continue;

					newThreads = true;
					ProcessThreadExtensions.Suspend(thread);
					suspended.Add(thread.Id);
				}
			} while (newThreads && triesLeft >= 0);
			// TODO: rollback thread states in case of exception

			if (newThreads) {
				foreach (ProcessThread thread in process.Threads) {
					if (suspended.Contains(thread.Id))
						ProcessThreadExtensions.Resume(thread);
					else
						Console.WriteLine("failed to suspend #{0}", thread.Id);
				}

				return false; 
			}

			return true;
		}

		/// <summary>
		/// Resumes process execution
		/// </summary>
		/// <param name="process">A process to resume</param>
		public static void Resume(this Process process)
		{
			if (process == null) throw new ArgumentNullException("process");

			foreach (ProcessThread thread in process.Threads) {
				thread.Resume();
			}
			// TODO: rollback thread states in case of exception
		}
	}
}
