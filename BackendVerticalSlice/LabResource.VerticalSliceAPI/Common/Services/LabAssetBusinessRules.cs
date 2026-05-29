using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Common.Services;

public static class LabAssetBusinessRules
{
    public static async Task ValidateTeacherAsync(ApplicationDbContext context, Guid? teacherId, CancellationToken ct)
    {
        if (!teacherId.HasValue) return;

        var teacher = await context.Users.FindAsync(new object[] { teacherId.Value }, ct);
        if (teacher == null) throw new NotFoundException("User", teacherId.Value);
        if (teacher.Role != UserRole.Teacher) throw new BadRequestException("Assigned user must be a Teacher.");
    }

    public static async Task ValidateSerialNumberUniquenessAsync(ApplicationDbContext context, string? serialNumber, Guid? excludeAssetId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(serialNumber)) return;

        var query = context.LabAssets.Where(a => a.SerialNumber == serialNumber);

        if (excludeAssetId.HasValue)
            query = query.Where(a => a.Id != excludeAssetId.Value);

        if (await query.AnyAsync(ct))
            throw new AlreadyExistsException("LabAsset", serialNumber);
    }
}