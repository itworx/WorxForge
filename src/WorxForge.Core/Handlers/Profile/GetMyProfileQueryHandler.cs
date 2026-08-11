using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.Core.Handlers.Profile;

public sealed class GetMyProfileQueryHandler : IQueryHandler<GetMyProfileQuery, ProfileDto>
{
    private readonly IRequestValidator<GetMyProfileQuery> _validator;
    private readonly IProfileService _profileService;

    public GetMyProfileQueryHandler(
        IRequestValidator<GetMyProfileQuery> validator,
        IProfileService profileService)
    {
        _validator = validator;
        _profileService = profileService;
    }

    public async Task<ProfileDto> HandleAsync(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);
        return await _profileService.GetMyProfileAsync(query.UserId, cancellationToken);
    }
}