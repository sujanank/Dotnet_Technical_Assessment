using System.ComponentModel.DataAnnotations;

namespace OpenSenseMapAPI.Models;

public class NewSenseBoxRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Exposure is required")]
    public string Exposure { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required")]
    public LocationDto Location { get; set; } = new();
}

public class LocationDto
{
    [Required(ErrorMessage = "Latitude is required")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Lat { get; set; }

    [Required(ErrorMessage = "Longitude is required")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Lng { get; set; }

    [Required(ErrorMessage = "Height is required")]
    public double Height { get; set; }
}
