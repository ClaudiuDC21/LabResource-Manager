using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class GetLabAssetById
{
    public record Query(Guid Id) : IRequest<Result>;

    public record Result(Guid Id, string Name, string? SerialNumber, string? Location, Guid? AssignedTeacherId, string? AssignedTeacherName, AssetStatus Status, bool IsActive, string? CurrentBorrowerName, DateTime? CurrentBorrowDate);

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var asset = await _context.LabAssets
                .Include(a => a.AssignedTeacher)
                .Include(a => a.BorrowingRecords).ThenInclude(b => b.User)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null) throw new NotFoundException("LabAsset", request.Id);

            var active = asset.BorrowingRecords.FirstOrDefault(b => b.ActualReturnedAt == null && b.Status == BorrowingStatus.Active);

            return new Result(asset.Id, asset.Name, asset.SerialNumber, asset.Location, asset.AssignedTeacherId, asset.AssignedTeacher?.FullName, asset.Status, asset.IsActive, active?.User?.FullName, active?.ActualBorrowedAt);
        }
    }
}