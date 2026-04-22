using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Domain.Entities;

public class BaseEntity
{
    [Key] public int Id { get; set; }
}
