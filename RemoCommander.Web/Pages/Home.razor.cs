using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace RemoCommander.Web.Pages;

public partial class Home
{
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string? token;
    private string tokenInput = "";

    protected override async Task OnInitializedAsync()
    {
        token = await JS.InvokeAsync<string>("localStorage.getItem", StorageKeys.NatureRemoToken);
    }

    private async Task SaveToken()
    {
        if (!string.IsNullOrEmpty(tokenInput)) {
            token = tokenInput;
            await JS.InvokeVoidAsync("localStorage.setItem", StorageKeys.NatureRemoToken, token);
            Navigation.NavigateTo("appliances");
        }
    }

    private async Task ClearToken()
    {
        token = null;
        await JS.InvokeVoidAsync("localStorage.removeItem", StorageKeys.NatureRemoToken);
    }
}
