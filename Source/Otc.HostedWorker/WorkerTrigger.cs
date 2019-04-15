namespace Otc.HostedWorker
{
    internal static class InternalHostedWorkerTrigger
    {
        private static volatile bool fired = false;
        private static object lockPad = new object();

        public static bool Pulled()
        {
            if(fired)
            {
                lock (lockPad)
                {
                    fired = false;
                }

                return true;
            }

            return false;
        }

        public static void Pull()
        {
            lock (lockPad)
            {
                fired = true;
            }
        }
    }
}
