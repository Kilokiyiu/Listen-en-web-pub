using KaoyanService.Domain.Entity;

namespace KaoyanService.Domain;

public interface IKaoyanRepo
{
    Task<ExamPaper[]> GetVisiblePapersAsync(string? series = null);
    Task<ExamPaper?> GetPaperByIdAsync(Guid paperId);
    Task<ExamSection[]> GetSectionsByPaperIdAsync(Guid paperId);
    Task<ExamQuestion[]> GetQuestionsBySectionIdsAsync(IEnumerable<Guid> sectionIds);
    Task<ExamPaper[]> GetAllPapersAsync();
    Task<ExamQuestion?> GetQuestionByIdAsync(Guid questionId);
}
