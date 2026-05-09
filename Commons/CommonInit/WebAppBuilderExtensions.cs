// using Microsoft.Data.SqlClient;
//
// namespace CommonInit;
//
// public static class WebAppBuilderExtensions
// {
//     public static void ConfigureDbConfiguration(this WebApplicationBuilder builder)
//     {
//         builder.Host.ConfigureAppConfiguration((hostCtx, configBuilder) =>
//         {
//             string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
//             // 从数据库读取配置，支持热更新（每5秒轮询）
//             configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), 
//                 reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
//         });
//     }
// }