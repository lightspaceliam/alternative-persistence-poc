namespace Stub.Models;

public class Patient
{
	public string Id { get; set; }
	public string Given { get; set; }
	public string Family { get; set; }
	public DateOnly BirthDate { get; set; }
}