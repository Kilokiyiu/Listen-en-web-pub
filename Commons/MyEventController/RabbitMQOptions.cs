namespace MyEventController;

public class RabbitMQOptions
{
    public bool Enabled { get; set; } = true;
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string ExchangeName { get; set; } = "listen_en_web_events";
}
