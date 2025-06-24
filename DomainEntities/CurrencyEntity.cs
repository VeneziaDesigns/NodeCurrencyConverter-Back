namespace NodeCurrencyConverter.Entities
{
    public sealed record CurrencyCode
    {
        public string Code { get; }

        public CurrencyCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Currency code cannot be empty", nameof(code));

            Code = code.Trim().ToUpper(); 
        }

        public override int GetHashCode() => Code.GetHashCode();
        public bool Equals(CurrencyCode? other) => other?.Code == Code;
    }
}
