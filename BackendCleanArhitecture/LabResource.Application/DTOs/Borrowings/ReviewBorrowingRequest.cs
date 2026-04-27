namespace LabResource.Application.DTOs.Borrowings;

public class ReviewBorrowingRequest
{
    public bool IsApproved { get; set; }
    public string? TeacherNotes { get; set; } 
}