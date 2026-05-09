using Microsoft.Extensions.DependencyInjection;

namespace MyCommonTools;

/// <summary>
/// 这是一个各自注册各自服务的接口，请在不同的业务服务中调用此接口来注册需要的服务器
/// </summary>
public interface IServiceInit
{
    public void ServiceInit(IServiceCollection services);
}