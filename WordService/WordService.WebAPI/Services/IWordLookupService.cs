using WordService.WebAPI.Models;

namespace WordService.WebAPI.Services;

public interface IWordLookupService
{
    Task<EnglishWordDetailDto?> LookupAsync(string word, CancellationToken cancellationToken = default);
}
