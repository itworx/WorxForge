using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.Core.Handlers.Profile;

public sealed class GetPublicProfileQueryHandler : IQueryHandler<GetPublicProfileQuery, ProfileDto>
{
    private readonly IRequestValidator<GetPublicProfileQuery> _validator;
    private readonly IProfileService _profileService;

    public GetPublicProfileQueryHandler(
        IRequestValidator<GetPublicProfileQuery> validator,
        IProfileService profileService)
    {
        _validator = validator;
        _profileService = profileService;
    }

    public async Task<ProfileDto> HandleAsync(GetPublicProfileQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);
        return await _profileService.GetPublicProfileAsync(query.TargetUserId, cancellationToken);
    }
}
