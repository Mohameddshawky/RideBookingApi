using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Features.Drivers.AcceptRide;
using RideBookingApi.Application.Features.Drivers.GetEarnings;
using RideBookingApi.Application.Features.Drivers.GetPendingRideRequests;
using RideBookingApi.Application.Features.Drivers.ToggleAvailability;
using RideBookingApi.Application.Features.Drivers.UpdateLocation;
using RideBookingApi.Application.Features.Drivers.UploadDocument;
using RideBookingApi.Domain.Enums;
using RideBookingApi.Models;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png"
    };

    private readonly GetPendingRideRequestsHandler _getPendingRideRequestsHandler;
    private readonly UpdateDriverLocationHandler _updateDriverLocationHandler;
    private readonly AcceptRideHandler _acceptRideHandler;
    private readonly ToggleAvailabilityHandler _toggleAvailabilityHandler;
    private readonly UploadDocumentHandler _uploadDocumentHandler;
    private readonly GetEarningsHandler _getEarningsHandler;
    private readonly IWebHostEnvironment _environment;

    public DriversController(
        GetPendingRideRequestsHandler getPendingRideRequestsHandler,
        UpdateDriverLocationHandler updateDriverLocationHandler,
        AcceptRideHandler acceptRideHandler,
        ToggleAvailabilityHandler toggleAvailabilityHandler,
        UploadDocumentHandler uploadDocumentHandler,
        GetEarningsHandler getEarningsHandler,
        IWebHostEnvironment environment)
    {
        _getPendingRideRequestsHandler = getPendingRideRequestsHandler;
        _updateDriverLocationHandler = updateDriverLocationHandler;
        _acceptRideHandler = acceptRideHandler;
        _toggleAvailabilityHandler = toggleAvailabilityHandler;
        _uploadDocumentHandler = uploadDocumentHandler;
        _getEarningsHandler = getEarningsHandler;
        _environment = environment;
    }

    [HttpGet("rides/pending")]
    public async Task<ActionResult<List<PendingRideRequestDto>>> GetPendingRideRequests(CancellationToken cancellationToken)
    {
        var rides = await _getPendingRideRequestsHandler.HandleAsync(cancellationToken);
        return Ok(rides);
    }

    [HttpPost("rides/accept")]
    public async Task<ActionResult<AcceptRideResponseDto>> AcceptRide([FromBody] AcceptRideCommand command, CancellationToken cancellationToken)
    {
        var result = await _acceptRideHandler.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("availability")]
    public async Task<ActionResult<ToggleAvailabilityResponseDto>> ToggleAvailability([FromBody] ToggleAvailabilityCommand command, CancellationToken cancellationToken)
    {
        var result = await _toggleAvailabilityHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateDriverLocationCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var updated = await _updateDriverLocationHandler.HandleAsync(userId, command, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("documents/upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxDocumentSizeBytes)]
    public async Task<ActionResult<UploadDocumentResponseDto>> UploadDocument(
        [FromForm] UploadDriverDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Select a document to upload." });
        }

        if (file.Length > MaxDocumentSizeBytes)
        {
            return BadRequest(new { message = "Document size must not exceed 10 MB." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedDocumentExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only PDF, JPG, JPEG, and PNG documents are allowed." });
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadDirectory = Path.Combine(webRoot, "uploads", "driver-documents");
        Directory.CreateDirectory(uploadDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var storedFilePath = Path.Combine(uploadDirectory, storedFileName);

        await using (var stream = System.IO.File.Create(storedFilePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var documentUrl = $"/uploads/driver-documents/{storedFileName}";
        var result = await _uploadDocumentHandler.HandleAsync(
            new UploadDocumentCommand(request.DriverId, request.DocumentType, documentUrl),
            cancellationToken);

        if (result.DocumentId == Guid.Empty)
        {
            System.IO.File.Delete(storedFilePath);
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{driverId:guid}/earnings")]
    public async Task<ActionResult<DriverEarningsDto>> GetEarnings(Guid driverId, CancellationToken cancellationToken)
    {
        var result = await _getEarningsHandler.HandleAsync(driverId, cancellationToken);
        if (result.DriverId == Guid.Empty)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);
    }
}
