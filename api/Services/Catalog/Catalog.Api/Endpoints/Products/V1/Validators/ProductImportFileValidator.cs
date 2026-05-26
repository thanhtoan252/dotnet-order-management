using FluentValidation;
using FluentValidation.Results;

namespace Catalog.Api.Endpoints.Products.V1.Validators;

public sealed class ProductImportFileValidator : AbstractValidator<IFormFile>
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    public const string PayloadTooLargeErrorCode = "PayloadTooLarge";

    private static readonly string[] AllowedContentTypes =
    [
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/octet-stream"
    ];

    public ProductImportFileValidator()
    {
        RuleFor(file => file).Custom(ValidateFile);
    }

    private static void ValidateFile(IFormFile? file, ValidationContext<IFormFile> context)
    {
        if (file is null || file.Length == 0)
        {
            context.AddFailure("file", "No file was uploaded.");
            return;
        }

        if (file.Length > MaxFileSizeBytes)
        {
            context.AddFailure(new ValidationFailure("file", "The uploaded file is too large.")
            {
                ErrorCode = PayloadTooLargeErrorCode
            });
            return;
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            context.AddFailure("file", "Only .xlsx files are accepted.");
            return;
        }

        if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            context.AddFailure("file", "Only .xlsx files are accepted.");
            return;
        }

        using var peek = file.OpenReadStream();
        Span<byte> magic = stackalloc byte[4];
        var read = peek.Read(magic);
        if (read < 4 || magic[0] != 0x50 || magic[1] != 0x4B || magic[2] != 0x03 || magic[3] != 0x04)
        {
            context.AddFailure("file", "Only .xlsx files are accepted.");
        }
    }
}
