using System.ComponentModel.DataAnnotations;

namespace BIZFLOW.Web.Models
{
    /// <summary>
    /// Одиниці виміру для товарів
    /// </summary>
    public enum UnitOfMeasure
    {
        [Display(Name = "Штуки")]
        Pieces = 0,

        [Display(Name = "Кілограми")]
        Kilograms = 1,

        [Display(Name = "Літри")]
        Liters = 2
    }
}
