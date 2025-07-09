namespace NodeCurrencyConverter.Entities
{
    public sealed record CurrencyEntity
    {
        public string Code { get; }

        public CurrencyEntity(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Currency code cannot be empty", nameof(code));

            Code = code.Trim().ToUpper(); 
        }

        public override int GetHashCode() => Code.GetHashCode();
        public bool Equals(CurrencyEntity? other) => other?.Code == Code;
    }
}
