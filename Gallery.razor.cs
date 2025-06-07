using Microsoft.AspNetCore.Components;

namespace Blazor.Quill;

public partial class Gallery
{
    [Parameter]
    public required List<ImageFile> Images { get; set; }

    [Parameter] public EventCallback<ImageFile> OnImageClicked { get; set; }

    [Parameter] public bool IsVisible { get; set; } = false;

    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<ChangeEventArgs> OnSearchTexChanged { get; set; }

    private string? _linkUrl;

    private void OnImageClick(ImageFile image)
    {
        OnImageClicked.InvokeAsync(image);
    }

    private void OnLinkClick()
    {
        if (!string.IsNullOrWhiteSpace(_linkUrl))
        {
            OnImageClicked.InvokeAsync(new ImageFile
            {
                Src = _linkUrl,
                Alt = string.Empty
            });
        }
    }

    private async Task OnSearchInput(ChangeEventArgs e)
    {
        await OnSearchTexChanged.InvokeAsync(e);
    }

    private void CloseModal()
    {
        IsVisible = false;
        OnClose.InvokeAsync();
    }
}
