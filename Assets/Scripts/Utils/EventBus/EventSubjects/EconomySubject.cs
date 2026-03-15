namespace LittleFarm.EconomyEventSubject
{
    public readonly struct CurrencyValueChanged
    {
        public CurrencyValueChanged(CurrencyType currencyType, long previousBalance, long newBalance, long delta, string reason)
        {
            CurrencyType = currencyType;
            PreviousBalance = previousBalance;
            NewBalance = newBalance;
            Delta = delta;
            Reason = reason;
        }

        public CurrencyType CurrencyType { get; }
        public long PreviousBalance { get; }
        public long NewBalance { get; }
        public long Delta { get; }
        public string Reason { get; }
    }
}
