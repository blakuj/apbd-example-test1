using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class AnimalRepository
{
    private readonly IConfiguration _configuration;

    public AnimalRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task<bool> doesAnimalExists(int id)
    {
        var query = "Select 1 from animal where id = @id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    public async Task<Animal> GetAnimal(int id)
    {
        var query =
            "SELECT A.id as AnimalID,A.Name as Animal_Name, A.Type as Type, A.AdmissionDate as Admission_Date," +
            " O.Id as OwnerID, O.FirstName as First_Name , o.LastName as Last_Name ," +
            " PA.Date as Date," +
            " P.Name as Procedure_Name , P.Description as Description" +
            " FROM Animal A JOIN Owner O on A.Owner_ID = O.ID join Procedure_Animal PA on A.ID = PA.Animal_ID join [Procedure] P on PA.Procedure_ID = P.ID" +
            " where A.id = @id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
   
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var animalIdOrdinal = reader.GetOrdinal("AnimalID");
        var animalNameOrdinal = reader.GetOrdinal("Animal_Name");
        var animalTypeOrdinal = reader.GetOrdinal("Type");
        var admissionDateOrdinal = reader.GetOrdinal("Admission_Date");
        var ownerIdOrdinal = reader.GetOrdinal("OwnerID");
        var firstNameOrdinal = reader.GetOrdinal("First_Name");
        var lastNameOrdinal = reader.GetOrdinal("Last_Name");
        var dateOrdinal = reader.GetOrdinal("Date");
        var procedureNameOrdinal = reader.GetOrdinal("Procedure_Name");
        var procedureDescriptionOrdinal = reader.GetOrdinal("Description");

        Animal animal = null;

        while (await reader.ReadAsync())
        {
            if (animal is not null)
            {
                animal.Procedures.Add(new Procedure()
                {
                    Date = reader.GetDateTime(dateOrdinal),
                    Name = reader.GetString(procedureNameOrdinal),
                    Description = reader.GetString(procedureDescriptionOrdinal)
                });
            }
            else
            {
                animal = new Animal()
                {
                    Id = reader.GetInt32(animalIdOrdinal),
                    Name = reader.GetString(animalNameOrdinal),
                    Type = reader.GetString(animalTypeOrdinal),
                    AdmissionDate = reader.GetDateTime(admissionDateOrdinal),
                    Owner = new Owner()
                    {
                        Id = reader.GetInt32(ownerIdOrdinal),
                        FirstName = reader.GetString(firstNameOrdinal),
                        LastName = reader.GetString(lastNameOrdinal),
                    },
                    Procedures = new List<Procedure>()
                    {
                        new Procedure()
                        {
                            Date = reader.GetDateTime(dateOrdinal),
                            Name = reader.GetString(procedureNameOrdinal),
                            Description = reader.GetString(procedureDescriptionOrdinal)
                        }
                    }
                };
            }
        }

        if (animal is null) throw new Exception();

        return animal;
    }


    public async Task<bool> doesOwnerExist(int id)
    {
        var query = "Select 1 from Owner where id = @id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    public async Task<bool> doesProcedureExist(int id)
    {
        var query = "Select 1 from [Procedure] where id = @id";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }
    public async Task AddNewAnimalWithProcedures(NewAnimalWithProcedures newAnimalWithProcedures)
    {
        var insert = @"INSERT INTO Animal VALUES(@Name, @Type, @AdmissionDate, @OwnerId);
					   SELECT @@IDENTITY AS ID;";
	    
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
	    
        command.Connection = connection;
        command.CommandText = insert;
	    
        command.Parameters.AddWithValue("@Name", newAnimalWithProcedures.Name);
        command.Parameters.AddWithValue("@Type", newAnimalWithProcedures.Type);
        command.Parameters.AddWithValue("@AdmissionDate", newAnimalWithProcedures.AdmissionDate);
        command.Parameters.AddWithValue("@OwnerId", newAnimalWithProcedures.OwnerID);
	    
        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
	    
        try
        {
            var id = await command.ExecuteScalarAsync();
    
            foreach (var procedure in newAnimalWithProcedures.newProcedures)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Procedure_Animal VALUES(@ProcedureId, @AnimalId, @Date)";
                command.Parameters.AddWithValue("@ProcedureId", procedure.ProcedureId);
                command.Parameters.AddWithValue("@AnimalId", id);
                command.Parameters.AddWithValue("@Date", procedure.Date);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

}