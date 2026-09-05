namespace AutoWeeklyCap.Exceptions;

public class InvalidCaptureStateException(string message) : Exception(message)
{
    public static InvalidCaptureStateException CreateCaptureIsAlreadyEnabled()
    {
        return new InvalidCaptureStateException("Cannot start capturing queued actions while already capturing actions");
    }
}
