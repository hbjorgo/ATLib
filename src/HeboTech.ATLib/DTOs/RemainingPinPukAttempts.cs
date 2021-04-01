namespace HeboTech.ATLib.DTOs
{
    public class RemainingPinPukAttempts
    {
        public RemainingPinPukAttempts(int pin1, int pin2, int puk1, int puk2)
        {
            Pin1 = pin1;
            Pin2 = pin2;
            Puk1 = puk1;
            Puk2 = puk2;
        }

        public int Pin1 { get; }
        public int Pin2 { get; }
        public int Puk1 { get; }
        public int Puk2 { get; }

        public override string ToString()
        {
            return $"PIN1: {Pin1}, PIN2: {Pin2}, PUK1: {Puk1}, PUK2: {Puk2}";
        }
    }
}
