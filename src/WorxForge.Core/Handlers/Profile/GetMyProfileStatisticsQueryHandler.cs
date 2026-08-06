using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Dtos.Profile;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.Core.Handlers.Profile;

public sealed class GetMyProfileStatisticsQueryHandler : IQueryHandler<GetMyProfileStatisticsQuery, ProfileStatisticsDto>
{
    private readonly IRequestValidator<GetMyProfileStatisticsQuery> _validator;
    private readonly IProfileService _profileService;

    public GetMyProfileStatisticsQueryHandler(
        IRequestValidator<GetMyProfileStatisticsQuery> validator,
        IProfileService profileService)
    {
        _validator = validator;
        _profileService = profileService;
    }

    public async Task<ProfileStatisticsDto> HandleAsync(GetMyProfileStatisticsQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(query, cancellationToken);
        return await _profileService.GetStatisticsAsync(query.UserId, cancellationToken);
    }
}
