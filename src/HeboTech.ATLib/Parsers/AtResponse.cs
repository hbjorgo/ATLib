using System.Collections.Generic;

namespace HeboTech.ATLib.Parsers
{
    public class AtResponse
    {
        public AtResponse(bool success, string finalResponse)
        {
            Success = success;
            FinalResponse = finalResponse;
        }

        public bool Success { get; }
        public string FinalResponse { get; }

        public override string ToString()
        {
            return $"Success: {Success}, FinalResponse: {FinalResponse}";
        }
    }

    public class AtSingleLineResponse : AtResponse
    {
        public AtSingleLineResponse(bool success, string finalResponse, string intermediate)
            : base(success, finalResponse)
        {
            Intermediate = intermediate;
        }

        public string Intermediate { get; }

        public override string ToString()
        {
            return $"Success: {Success}, FinalResponse: {FinalResponse}, Intermediate: {Intermediate}";
        }
    }

    public class AtMultiLineResponse : AtResponse
    {
        private readonly List<string> intermediates;

        public AtMultiLineResponse(bool success, string finalResponse, List<string> intermediates)
            : base(success, finalResponse)
        {
            this.intermediates = intermediates;
        }

        public IReadOnlyCollection<string> Intermediates => intermediates.AsReadOnly();

        public override string ToString()
        {
            return $"Success: {Success}, FinalResponse: {FinalResponse}, Intermediates: {Intermediates.Count}";
        }
    }
}
