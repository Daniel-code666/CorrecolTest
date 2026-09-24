using System.ComponentModel.DataAnnotations;

namespace CorrecolTest.Application.Common;

public class Pagination
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public void Validate()
    {
        if (PageNumber < 1 || PageSize is < 1 or > 100 || ((long)PageNumber - 1) * PageSize > int.MaxValue)
            throw new ValidationException("La página debe ser positiva y su tamaño debe estar entre 1 y 100.");
    }
}

