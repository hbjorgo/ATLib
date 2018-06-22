using System;
using System.Threading.Tasks;

namespace HeboTech.ATLib
{
    internal static class TaskExtensions
    {
        public static async Task<T> TimeoutAfter<T>(this Task<T> task, int delay)
        {
            await Task.WhenAny(task, Task.Delay(delay));
            if (!task.IsCompleted)
            {
                throw new TimeoutException();
            }
            return await task;
        }
    }
}
