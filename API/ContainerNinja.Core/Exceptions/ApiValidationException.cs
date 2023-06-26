using FluentValidation.Results;

namespace ContainerNinja.Core.Exceptions
{
    public class ApiValidationException : Exception
    {
        public string ForceFunctionCall { get; private set; }

        public ApiValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ApiValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

            var functionCallValidator = failures.FirstOrDefault(f => f.ErrorMessage.Contains("ForceFunctionCall="));
            if (functionCallValidator != null)
            {
                ForceFunctionCall = functionCallValidator.ErrorMessage.Replace("ForceFunctionCall=", "");
            }
            else
            {
                ForceFunctionCall = "auto";
            }
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}