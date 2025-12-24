using System.ComponentModel.DataAnnotations;

namespace Business_School.Helpers
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) => value is bool b && b;
    }
}
