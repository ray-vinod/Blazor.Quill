using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;

namespace Blazor.Quill;

public partial class Editor
{
    #region Parameters
    [Inject] private IJSRuntime? JSRuntime { get; set; }

    [Parameter] public bool ReadOnly { get; set; } = false;

    [Parameter] public string Placeholder { get; set; } = "Insert text here...";

    [Parameter] public string Theme { get; set; } = "snow";

    [Parameter] public string GalleryUrl { get; set; } = "/api/images/gallery";

    private bool _contentAvailable = false;
    private StringBuilder _chunks = new();

    private List<ImageFile> _filteredImages = [];
    private List<ImageFile>? _images;
    private bool _isImageModalVisible = false;
    private ElementReference _quillElement;
    IJSObjectReference? _module;
    #endregion

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JSRuntime!.InvokeAsync<IJSObjectReference>("import", "./_content/Blazor.Editor/Editor.razor.js");
            await _module!.InvokeVoidAsync("registerEditorInstance", DotNetObjectReference.Create(this));
            await CreateQuill();
        }
    }

    private async ValueTask<object> CreateQuill()
    {
        return await _module!.InvokeAsync<object>("initQuill", _quillElement, GalleryUrl, ReadOnly, Placeholder, Theme);
    }

    public async Task SetHTMLAsync(string html)
    {
        await _module!.InvokeVoidAsync("setQuillHTML", html);
    }

    public async Task<string> GetHTMLAsync()
    {
        await _module!.InvokeAsync<string>("getQuillHTML");

        if (_contentAvailable)
        {
            _contentAvailable = false;
            return _chunks.ToString();
        }
        return string.Empty;
    }

    public async Task SetContentAsync(Delta delta)
    {
        var options = Options();
        var deltaJson = JsonSerializer.Serialize(delta, options);
        await _module!.InvokeVoidAsync("setQuillContents", deltaJson);
    }

    public async Task<string> GetContentAsync()
    {
        await _module!.InvokeAsync<string>("getQuillContents");

        if (_contentAvailable)
        {
            _contentAvailable = false;
            return _chunks.ToString();
        }
        return string.Empty;
    }

    [JSInvokable]
    public void DataReceived(string chunk, int index, int totalChunks)
    {
        _chunks.Append(chunk);
        if (index == totalChunks - 1)
        {
            _contentAvailable = true;
        }
    }

    [JSInvokable]
    public async Task ShowImageModal()
    {
        _isImageModalVisible = true;
        var imageJson = await _module!.InvokeAsync<string>("fetchImagesAsync");
        var options = Options();

        _images = JsonSerializer.Deserialize<List<ImageFile>>(imageJson, options);
        _filteredImages = _images!;

        StateHasChanged();
    }

    private async Task ImageClicked(ImageFile image)
    {
        _isImageModalVisible = false;
        bool isUrl = IsValidUrl(image.Src);
        await _module!.InvokeVoidAsync("insertImage", isUrl, image.Src, image.Alt);
        StateHasChanged();
    }

    public static bool IsValidUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult))
        {
            if (uriResult != null)
            {
                bool result = uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
                return result;
            }
        }
        return false;
    }
    private JsonSerializerOptions Options()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        return options;
    }

    private void OnSearchTexChanged(ChangeEventArgs e)
    {
        string searchText = e.Value?.ToString()?.ToUpper() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            _filteredImages = _images!.Where(x => x.Src!.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    }
}