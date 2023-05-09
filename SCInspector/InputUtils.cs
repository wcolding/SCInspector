namespace SCInspector
{
    public static class InputUtils
    {
        public static Action Debounce(int ms, Action func)
        {
            var counter = 0;
            return () =>
            {
                var current = Interlocked.Increment(ref counter);
                Task.Delay(ms).ContinueWith(task =>
                {
                    if (current == counter)
                    {
                        func();
                    }

                    task.Dispose();
                });
            };
        }
    }
}
