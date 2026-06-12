using DomainCommons;

namespace ListenService.Domain.Entity;

/// <summary>
/// 这是每张试卷的聚合根，程序获取到了Category的对应Id后，在对应的Category获取对应的试卷
/// </summary>
public class Album : IEntity, ICreationTime
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// 试卷名称
    /// </summary>
    public MultilingualString Name { get; private set; }
    
    /// <summary>
    /// 所属的Category类别，比如这张试卷属于CET4
    /// </summary>
    public Guid CategoryId { get; private set; }
    
    /// <summary>
    /// 排序
    /// </summary>
    public int SequenceNumber { get; private set; }
    
    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// 试卷 PDF 相对路径，如 /papers/CET6/{albumId}.paper.pdf
    /// </summary>
    public string? PaperFileUrl { get; private set; }

    /// <summary>
    /// 答案 PDF 相对路径，如 /papers/CET6/{albumId}.answer.pdf
    /// </summary>
    public string? AnswerFileUrl { get; private set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; init; }

    private Album()
    {
        
    }

    public Album(MultilingualString name, Guid categoryId, int sequenceNumber)
    {
        Id =  Guid.NewGuid();
        Name = name;
        CategoryId = categoryId;
        SequenceNumber = sequenceNumber;
        IsVisible = true;
        CreationTime = DateTime.Now;
    }
    
    public Album ChangeName(MultilingualString name)
    {
        Name = name;
        return this;
    }

    public Album ChangeSequenceNumber(int seq)
    {
        SequenceNumber = seq;
        return this;
    }

    public Album Show()
    {
        IsVisible = true;
        return this;
    }

    public Album Hide()
    {
        IsVisible = false;
        return this;
    }

    public Album SetPaperFileUrl(string? url)
    {
        PaperFileUrl = url;
        return this;
    }

    public Album SetAnswerFileUrl(string? url)
    {
        AnswerFileUrl = url;
        return this;
    }

}