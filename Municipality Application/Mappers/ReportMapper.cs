using Municipality_Application.Models;
using Municipality_Application.ViewModels;

namespace Municipality_Application.Mappers
{
    public static class ReportMapper
    {
        public static ServiceRequestStatusViewModel ToViewModel(Report report)
        {
            return new ServiceRequestStatusViewModel
            {
                Id = report.Id,
                Description = report.Description,
                Address = report.Address?.FormattedAddress
                    ?? $"{report.Address?.Street}, " +
                    $"{report.Address?.City}, " +
                    $"{report.Address?.Province}, " +
                    $"{report.Address?.PostalCode}, " +
                    $"{report.Address?.Country}",
                ReportedAt = report.ReportedAt,
                Status = report.Status.ToString(),
                Category = report.Category?.Name
            };
        }

        public static Report ToDomainModel(ReportCreateViewModel model)
        {
            return new Report
            {
                Description = model.Description,
                CategoryId = model.CategoryId,
                Address = new Address
                {
                    Street = model.Address.Street,
                    City = model.Address.City,
                    Province = model.Address.Province,
                    PostalCode = model.Address.PostalCode,
                    Country = model.Address.Country,
                    Latitude = model.Address.Latitude,
                    Longitude = model.Address.Longitude,
                    FormattedAddress = model.Address.FormattedAddress
                },
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                ReportedAt = DateTime.UtcNow,
                Status = IssueStatus.Reported
            };
        }

        public static AddressViewModel ToAddressViewModel(Address? address)
        {
            if (address == null) return new AddressViewModel();
            return new AddressViewModel
            {
                Street = address.Street,
                Suburb = address.Suburb,
                City = address.City,
                Province = address.Province,
                PostalCode = address.PostalCode,
                Country = address.Country,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                FormattedAddress = address.FormattedAddress
            };
        }

        public static ReportConfirmationViewModel ToConfirmationViewModel(Report report)
        {
            return new ReportConfirmationViewModel
            {
                Id = report.Id,
                CategoryName = report.Category?.Name ?? string.Empty,
                Description = report.Description,
                ReportedAt = report.ReportedAt,
                Status = report.Status.ToString(),
                Address = ToAddressViewModel(report.Address),
                PhoneNumber = report.PhoneNumber,
                Email = report.Email,
                Attachments = report.Attachments?
                    .Select(a => new AttachmentViewModel
                    {
                        FileName = a.FileName,
                        FilePath = a.FilePath
                    })
                    .ToList() ?? new List<AttachmentViewModel>()
            };
        }
    }
}