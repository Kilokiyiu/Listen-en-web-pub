using Microsoft.AspNetCore.Mvc;
using WordService.WebAPI.Services;

namespace WordService.WebAPI.Controllers.Word;

[ApiController]
[Route("dictionary")]
public class DictionaryController : ControllerBase
{
    private readonly IWordLookupService wordLookupService;

    public DictionaryController(IWordLookupService wordLookupService)
    {
        this.wordLookupService = wordLookupService;
    }

    /// <summary>
    /// 查询英语单词、短语或句子（xxapi 词典 + 翻译回退）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Lookup([FromQuery] string word, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return BadRequest(new { code = 400, message = "请输入要查询的内容" });
        }

        if (!EnglishQueryValidator.IsValid(word))
        {
            return BadRequest(new { code = 400, message = "请输入有效的英语单词、短语或句子" });
        }

        try
        {
            var detail = await wordLookupService.LookupAsync(word, cancellationToken);
            if (detail == null)
            {
                return Ok(new { code = 404, message = "未找到相关释义" });
            }

            return Ok(new
            {
                code = 200,
                data = new
                {
                    word = detail.Word,
                    ukphone = detail.Ukphone,
                    usphone = detail.Usphone,
                    ukspeech = detail.Ukspeech,
                    usspeech = detail.Usspeech,
                    translations = detail.Translations.Select(t => new { pos = t.Pos, tran_cn = t.TranCn }),
                    sentences = detail.Sentences.Select(s => new { s_content = s.SContent, s_cn = s.SCn }),
                    phrases = detail.Phrases.Select(p => new { p_content = p.PContent }),
                    relWords = detail.RelWords.Select(g => new
                    {
                        Pos = g.Pos,
                        Hwds = g.Hwds.Select(w => new { hwd = w.Hwd, tran = w.Tran }),
                    }),
                    synonyms = detail.Synonyms.Select(g => new
                    {
                        pos = g.Pos,
                        Hwds = g.Hwds.Select(w => new { word = w.Word }),
                    }),
                },
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { code = 503, message = ex.Message });
        }
    }
}
