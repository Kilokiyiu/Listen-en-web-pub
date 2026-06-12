namespace WordService.WebAPI.Models;

public class EnglishWordDetailDto
{
    public string Word { get; set; } = string.Empty;
    public string? Ukphone { get; set; }
    public string? Usphone { get; set; }
    public string? Ukspeech { get; set; }
    public string? Usspeech { get; set; }
    public List<WordTranslationDto> Translations { get; set; } = [];
    public List<WordSentenceDto> Sentences { get; set; } = [];
    public List<WordPhraseDto> Phrases { get; set; } = [];
    public List<WordRelGroupDto> RelWords { get; set; } = [];
    public List<WordSynonymGroupDto> Synonyms { get; set; } = [];
}

public class WordTranslationDto
{
    public string Pos { get; set; } = string.Empty;
    public string TranCn { get; set; } = string.Empty;
}

public class WordSentenceDto
{
    public string SContent { get; set; } = string.Empty;
    public string SCn { get; set; } = string.Empty;
}

public class WordPhraseDto
{
    public string PContent { get; set; } = string.Empty;
}

public class WordRelGroupDto
{
    public string Pos { get; set; } = string.Empty;
    public List<WordRelItemDto> Hwds { get; set; } = [];
}

public class WordRelItemDto
{
    public string Hwd { get; set; } = string.Empty;
    public string Tran { get; set; } = string.Empty;
}

public class WordSynonymGroupDto
{
    public string Pos { get; set; } = string.Empty;
    public List<WordSynonymItemDto> Hwds { get; set; } = [];
}

public class WordSynonymItemDto
{
    public string Word { get; set; } = string.Empty;
}
