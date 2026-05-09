using DomainCommons;

namespace ListenService.Domain.Entity;

/// <summary>
/// 这是试卷题目的聚合根，在获取到了Album对应的试卷后，获取当前试卷上的所有题目数据
/// </summary>
public class Episode : IEntity, ICreationTime
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// 题目名称，如"Section A - News Report 1"
    /// </summary>
    public MultilingualString Name { get; private set; }
    
    /// <summary>
    /// 所属试卷ID
    /// </summary>
    public Guid AlbumId { get; private set; }
    
    /// <summary>
    /// 音频文件相对路径，如"/audios/cet4-2024-06-a.mp3"
    /// </summary>
    public string AudioUrl { get; private set; }
    
    /// <summary>
    /// 音频时长（秒）
    /// 部分浏览器计算 duration 不准确，需要服务器端存储真实值
    /// </summary>
    public double DurationInSecond { get; private set; }
    
    /// <summary>
    /// 字幕内容（JSON 格式）
    /// 格式：[{"start":0,"end":3.5,"text":"...","translation":"..."}]
    /// </summary>
    public string Subtitle { get; private set; }
    
    /// <summary>
    /// 字幕格式，MVP 阶段固定为 "json"
    /// </summary>
    public string SubtitleType { get; private set; }
    
    /// <summary>
    /// 是否可见（发现问题时可先隐藏）
    /// </summary>
    public bool IsVisible { get; private set; }
    
    /// <summary>
    /// 每个类别的排序
    /// </summary>
    public int SequenceNumber { get; private set; }

    public DateTime CreationTime { get; init; }

    private Episode() { } // EF Core

    public Episode(MultilingualString name, Guid albumId, string audioUrl, 
        double durationInSecond, string subtitle, int sequenceNumber, string subtitleType = "json" )
    {
        Id = Guid.NewGuid();
        Name = name;
        AlbumId = albumId;
        AudioUrl = audioUrl;
        DurationInSecond = durationInSecond;
        SequenceNumber = sequenceNumber;
        Subtitle = subtitle;
        SubtitleType = subtitleType;
        IsVisible = true; // 默认可见
        CreationTime = DateTime.Now;
    }

    public Episode ChangeName(MultilingualString name)
    {
        Name = name;
        return this;
    }

    public Episode ChangeSubtitle(string subtitle, string subtitleType)
    {
        Subtitle = subtitle;
        SubtitleType = subtitleType;
        return this;
    }

    public Episode Show()
    {
        IsVisible = true;
        return this;
    }

    public Episode Hide()
    {
        IsVisible = false;
        return this;
    }
}