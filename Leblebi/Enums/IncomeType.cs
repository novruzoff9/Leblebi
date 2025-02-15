using System.ComponentModel.DataAnnotations;

namespace Leblebi.Enums;

public enum IncomeType
{
    [Display(Name = "Nağd Pul")]
    Cash,

    [Display(Name = "POS Terminal")]
    PosTerminal
}
