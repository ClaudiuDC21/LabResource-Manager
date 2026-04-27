using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class GetAllActiveLabAssets
{
    public record Query() : IRequest<IEnumerable<Result>>;

    public record Result(
        Guid Id,
        string Name,
        string? SerialNumber,
        string? Location,
        Guid? AssignedTeacherId,
        string? AssignedTeacherName,
        AssetStatus Status,
        bool IsActive,
        string? CurrentBorrowerName,
        DateTime? CurrentBorrowDate);

    public class Handler : IRequestHandler<Query, IEnumerable<Result>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.LabAssets
                .Include(a => a.AssignedTeacher)
                .Where(a => a.IsActive)
                .Select(a => new Result(
                    a.Id,
                    a.Name,
                    a.SerialNumber,
                    a.Location,
                    a.AssignedTeacherId,
                    a.AssignedTeacher != null ? a.AssignedTeacher.FullName : null,
                    a.Status,
                    a.IsActive,
                    a.BorrowingRecords
                        .Where(b => b.ActualReturnedAt == null && b.Status == BorrowingStatus.Active)
                        .Select(b => b.User.FullName)
                        .FirstOrDefault(),
                    a.BorrowingRecords
                        .Where(b => b.ActualReturnedAt == null && b.Status == BorrowingStatus.Active)
                        .Select(b => (DateTime?)b.ActualBorrowedAt)
                        .FirstOrDefault()
                ))
                .ToListAsync(cancellationToken);
        }
    }
}