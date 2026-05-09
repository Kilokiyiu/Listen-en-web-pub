using DomainCommons;

namespace ListenService.Domain.Entity;

/// <summary>
/// 这是分类的聚合根，用于将所有的材料按照四六级、雅思、托福等进行分类，程序获取数据会首先获取Category的分类数据，再在后续的聚合中细分
/// </summary>
public class Category : IEntity, ICreationTime
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// 分类名称
    /// </summary>
    public MultilingualString Name { get; private set; }
    
    /// <summary>
    /// 分类编码
    /// </summary>
    public string Code { get; private set; }
    
    /// <summary>
    /// 每个类别的排序
    /// </summary>
    public int SequenceNumber { get; private set; }
    
    /// <summary>
    /// 图片连接
    /// </summary>
    public string? CoverUrl { get; private set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; init; }

    private Category()
    {
        
    }

    public Category(MultilingualString name, string code, int sequenceNumber, string? coverUrl = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        SequenceNumber = sequenceNumber;
        CoverUrl = coverUrl;
        CreationTime = DateTime.UtcNow;
    }

    public Category ChangeName(MultilingualString name)
    {
        Name = name;
        return this;
    }

    public Category ChangeSequenceNumber(int seq)
    {
        SequenceNumber = seq;
        return this;
    }
}