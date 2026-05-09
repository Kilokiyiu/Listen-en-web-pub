namespace ListenService.WebAPI.Controllers.Admin.DTO;

public class UploadAudioRequest
{
    public string Category { get; set; } //分类
    public int Year { get; set; } //年份
    public int Month { get; set; } //月份
    public int SetNumber { get; set; } //第几套
    public IFormFile File { get; set; } //对应的文件
    
}