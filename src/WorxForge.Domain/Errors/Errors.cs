namespace WorxForge.Domain.Errors;

/// <summary>
/// The single lookup of every non-validation <see cref="Error"/> in the system. Each entry pairs a stable
/// <see cref="ErrorCodes"/> constant with its one canonical message and <see cref="ErrorType"/>, so throw sites
/// never re-type wording and a given code can never mean two different things. Field-level validation messages
/// are intentionally not catalogued here - see <see cref="Exceptions.ValidationException"/> /
/// <see cref="Exceptions.RequestValidationException"/>, which are inherently per-rule and not duplicated.
/// </summary>
public static class Errors
{
    public static readonly Error GeneralUnexpected = new(ErrorCodes.General.Unexpected, "An unexpected error occurred. Please try again later.", ErrorType.Unexpected);

    public static readonly Error ValidationFailed = new(ErrorCodes.Validation.Failed, "One or more validation errors occurred.", ErrorType.Validation);

    public static readonly Error ResourceNotFound = new(ErrorCodes.Resource.NotFound, "The requested resource was not found.", ErrorType.NotFound);
    public static readonly Error ResourceConflict = new(ErrorCodes.Resource.Conflict, "The request could not be completed due to a conflict.", ErrorType.Conflict);

    public static readonly Error AuthenticationInvalidCredentials = new(ErrorCodes.Authentication.InvalidCredentials, "The provided credentials are invalid.", ErrorType.Unauthorized);
    public static readonly Error AuthenticationEmailNotVerified = new(ErrorCodes.Authentication.EmailNotVerified, "Your Google account email must be verified before you can sign in.", ErrorType.Unauthorized);
    public static readonly Error AuthenticationEmailAlreadyLinked = new(ErrorCodes.Authentication.EmailAlreadyLinked, "This email address is already linked to a different account.", ErrorType.Conflict);
    public static readonly Error AuthenticationSessionNotFound = new(ErrorCodes.Authentication.SessionNotFound, "Session was not found.", ErrorType.NotFound);

    public static readonly Error AuthorizationForbidden = new(ErrorCodes.Authorization.Forbidden, "You are not allowed to perform this action.", ErrorType.Forbidden);

    public static readonly Error UserNotFound = new(ErrorCodes.User.NotFound, "User was not found.", ErrorType.NotFound);
    public static readonly Error UserUsernameAlreadyInUse = new(ErrorCodes.User.UsernameAlreadyInUse, "Username is already in use.", ErrorType.Conflict);

    public static readonly Error AssetNotFound = new(ErrorCodes.Asset.NotFound, "Asset was not found.", ErrorType.NotFound);
    public static readonly Error AssetForbidden = new(ErrorCodes.Asset.Forbidden, "You are not allowed to modify this asset.", ErrorType.Forbidden);

    public static readonly Error AssetTypeNotFound = new(ErrorCodes.AssetType.NotFound, "Asset type was not found.", ErrorType.NotFound);
    public static readonly Error DomainNotFound = new(ErrorCodes.Domain.NotFound, "Domain was not found.", ErrorType.NotFound);
    public static readonly Error UseCaseNotFound = new(ErrorCodes.UseCase.NotFound, "Use case was not found.", ErrorType.NotFound);
    public static readonly Error ToolNotFound = new(ErrorCodes.Tool.NotFound, "Tool was not found.", ErrorType.NotFound);

    public static readonly Error CommentNotFound = new(ErrorCodes.Comment.NotFound, "Comment was not found.", ErrorType.NotFound);
    public static readonly Error CommentForbidden = new(ErrorCodes.Comment.Forbidden, "You are not allowed to modify this comment.", ErrorType.Forbidden);

    public static readonly Error NotificationNotFound = new(ErrorCodes.Notification.NotFound, "Notification was not found.", ErrorType.NotFound);
    public static readonly Error NotificationForbidden = new(ErrorCodes.Notification.Forbidden, "You are not allowed to access this notification.", ErrorType.Forbidden);
    public static readonly Error NotificationAlreadyRead = new(ErrorCodes.Notification.AlreadyRead, "Notification has already been read.", ErrorType.Conflict);
}
