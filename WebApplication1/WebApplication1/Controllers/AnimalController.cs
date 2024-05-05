using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalController : ControllerBase
{
    private readonly AnimalRepository _animalRepository;

    public AnimalController(AnimalRepository animalRepository)
    {
        _animalRepository = animalRepository;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetAnimal(int id)
    {
        if (!await _animalRepository.doesAnimalExists(id))
        {
            return NotFound("Animal with given id does not exist");
        }

        var animal = await _animalRepository.GetAnimal(id);

        return Ok(animal);
    }

    [HttpPost]
    public async Task<IActionResult> AddAnimal(NewAnimalWithProcedures newAnimalWithProcedures)
    {
        if (!await _animalRepository.doesOwnerExist(newAnimalWithProcedures.Id))
        {
            return NotFound();
        }

        foreach (var procedure in newAnimalWithProcedures.newProcedures)
        {
            if (!await _animalRepository.doesProcedureExist(procedure.ProcedureId))
                return NotFound();
        }

        await _animalRepository.AddNewAnimalWithProcedures(newAnimalWithProcedures);

        return Created();
    }
}