using TmsApi.Domain.Entities;

namespace TmsApi.Application.DTOs;
public record UpdateEnrollmentStatusRequest(
    EnrollmentStatus Status
);