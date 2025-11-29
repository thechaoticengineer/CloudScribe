namespace CloudScribe.SharedKernel;

public record Error
{
    public string Code { get;}
    public string Message { get;}
    public ErrorType Type { get;}

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }
    
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "Value cannot be null", ErrorType.Validation);
    
    public static Error Failure(string code, string description) => 
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) => 
        new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description) => 
        new(code, description, ErrorType.Validation);

    public static Error Conflict(string code, string description) => 
        new(code, description, ErrorType.Conflict);
        
    public static Error Unauthorized(string code, string description) => 
        new(code, description, ErrorType.Unauthorized);
}