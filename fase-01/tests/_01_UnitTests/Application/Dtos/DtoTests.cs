using System.ComponentModel.DataAnnotations;

namespace fase_01.tests.UnitTests.Application.Dtos
{
    public static class DtoTests
    {
        /// <summary>
        /// Para testar as validações via DataAnnotations 
        /// ([Required], [EmailAddress], [RegularExpression], [Compare], [Url]) e a 
        /// interface IValidatableObject sem precisar subir controllers, utilizamos 
        /// a classe auxiliar System.ComponentModel.DataAnnotations.Validator.
        /// </summary>
        /// <param name="model">dto que terá as DataAnnotations validadas</param>
        /// <returns>Listagem com as falhas</returns>
        public static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
            return validationResults;
        }

    }
}