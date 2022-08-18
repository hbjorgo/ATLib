namespace HeboTech.ATLib.Events
{
    public class MissedCallEventArgs
    {
        public MissedCallEventArgs(string time, string phoneNumber)
        {
            Time = time;
            PhoneNumber = phoneNumber;
        }

        public string Time { get; }
        public string PhoneNumber { get; }

        public static MissedCallEventArgs CreateFromResponse(string response)
        {
            string[] split = response.Split(new []{ ' ' }, 3);
            return new MissedCallEventArgs(split[1], split[2]);
        }
    }
}
