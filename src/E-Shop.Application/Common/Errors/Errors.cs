
using FluentResults;

namespace E_Shop.Application.Common.Errors
{
    //404
    public class NotFoundError : Error
    {
        public NotFoundError(string message) : base(message)
        {
            Metadata.Add("ErrorCode", "NOT_FOUND");
        }
    }
    //409
    public class ConflictError : Error
    {
        public ConflictError(string message) : base(message)
        {
            Metadata.Add("ErrorCode", "CONFLICT");
        }
    }
}
