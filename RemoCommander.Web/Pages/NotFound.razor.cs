using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace RemoCommander.Web.Pages;

public partial class NotFound
{
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
}
