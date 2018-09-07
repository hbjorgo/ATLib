namespace HeboTech.ATLib
{
    public interface IGsmStream
    {
        /// <summary>
        /// Send a command and wait for an expected reply.
        /// </summary>
        /// <param name="send">The command to send</param>
        /// <param name="expectedReply">The expected reply to wait for</param>
        /// <param name="timeout">Time timeout in milliseconds</param>
        /// <returns></returns>
        bool SendCheckReply(string send, string expectedReply, int timeout);

        /// <summary>
        /// Send a command, wait for an expected termination reply and return the reply.
        /// </summary>
        /// <param name="send">The command to send</param>
        /// <param name="terminationReply">The expected termination reply to wait for</param>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns></returns>
        string SendGetReply(string send, string terminationReply, int timeout);
    }
}
