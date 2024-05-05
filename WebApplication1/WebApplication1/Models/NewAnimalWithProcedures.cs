namespace WebApplication1.Models;

public class NewAnimalWithProcedures
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public int OwnerID { get; set; } 
    public List<newProcedure> newProcedures { get; set; } = null!;
}
public class newProcedure
{
    public int ProcedureId { get; set; }
    public DateTime Date { get; set; }
}