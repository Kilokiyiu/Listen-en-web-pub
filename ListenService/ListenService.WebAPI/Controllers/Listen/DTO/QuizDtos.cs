namespace ListenService.WebAPI.Controllers.Listen.DTO;

public class QuizQuestionPublicDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Stem { get; set; } = string.Empty;
    public string[] Options { get; set; } = Array.Empty<string>();
    public int SequenceNumber { get; set; }
}

public class QuizSectionPublicDto
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = "Section A";
    public string Title { get; set; } = string.Empty;
    public string Transcript { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int SequenceNumber { get; set; }
    public int QuestionCount { get; set; }
    public QuizQuestionPublicDto[] Questions { get; set; } = Array.Empty<QuizQuestionPublicDto>();
}

public class QuizPaperDto
{
    public Guid AlbumId { get; set; }
    public int SectionCount { get; set; }
    public int QuestionCount { get; set; }
    public QuizSectionPublicDto[] Sections { get; set; } = Array.Empty<QuizSectionPublicDto>();
}

public class QuizAnswerItem
{
    public Guid QuestionId { get; set; }
    public int SelectedIndex { get; set; }
}

public class SubmitQuizRequest
{
    public Guid AlbumId { get; set; }
    public Guid? SectionId { get; set; }
    public QuizAnswerItem[] Answers { get; set; } = Array.Empty<QuizAnswerItem>();
}

public class QuizQuestionResultDto
{
    public Guid QuestionId { get; set; }
    public Guid SectionId { get; set; }
    public int Number { get; set; }
    public string Stem { get; set; } = string.Empty;
    public string[] Options { get; set; } = Array.Empty<string>();
    public int? SelectedIndex { get; set; }
    public int CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}

public class SubmitQuizResponse
{
    public Guid AlbumId { get; set; }
    public Guid? SectionId { get; set; }
    public int Total { get; set; }
    public int CorrectCount { get; set; }
    public double ScorePercent { get; set; }
    public QuizQuestionResultDto[] Results { get; set; } = Array.Empty<QuizQuestionResultDto>();
}
