namespace CosmosWebApi.Models;

public class MenuItem
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string category { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int price { get; set; }
    public string pickey { get; set; }
    
}
