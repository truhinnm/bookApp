namespace BookApp.Services
{
    public class BookValidationException : Exception
    {
        public BookValidationException(IReadOnlyList<string> errors)
            : base(string.Join(" ", errors))
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
    }
}
