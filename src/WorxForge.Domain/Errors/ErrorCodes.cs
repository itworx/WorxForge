namespace WorxForge.Domain.Errors;

/// <summary>Stable, machine-readable error codes shared by every layer; grouped by the concept they describe.</summary>
public static class ErrorCodes
{
    public static class General
    {
        public const string Unexpected = "General.Unexpected";
    }

    public static class Validation
    {
        public const string Failed = "Validation.Failed";
    }

    public static class Resource
    {
        public const string NotFound = "Resource.NotFound";
        public const string Conflict = "Resource.Conflict";
    }

    public static class Authentication
    {
        public const string InvalidCredentials = "Authentication.InvalidCredentials";
        public const string EmailNotVerified = "Authentication.EmailNotVerified";
        public const string EmailAlreadyLinked = "Authentication.EmailAlreadyLinked";
        public const string SessionNotFound = "Authentication.SessionNotFound";
    }

    public static class Authorization
    {
        public const string Forbidden = "Authorization.Forbidden";
    }

    public static class User
    {
        public const string NotFound = "User.NotFound";
        public const string UsernameAlreadyInUse = "User.UsernameAlreadyInUse";
    }

    public static class Asset
    {
        public const string NotFound = "Asset.NotFound";
        public const string Forbidden = "Asset.Forbidden";
    }

    public static class AssetType
    {
        public const string NotFound = "AssetType.NotFound";
    }

    public static class Domain
    {
        public const string NotFound = "Domain.NotFound";
    }

    public static class UseCase
    {
        public const string NotFound = "UseCase.NotFound";
    }

    public static class Tool
    {
        public const string NotFound = "Tool.NotFound";
    }

    public static class Comment
    {
        public const string NotFound = "Comment.NotFound";
        public const string Forbidden = "Comment.Forbidden";
    }

    public static class Notification
    {
        public const string NotFound = "Notification.NotFound";
        public const string AlreadyRead = "Notification.AlreadyRead";
        public const string Forbidden = "Notification.Forbidden";
    }
}
