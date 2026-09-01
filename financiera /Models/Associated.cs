namespace financiera.Models;

public class Associated
{
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone {get;set;} = string.Empty;
    public string Address {get; set; } = string.Empty;
    public bool IsActive {get; set;} = true;
}