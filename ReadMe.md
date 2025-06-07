# BlazorEditor

It is a quilljs based richtext editor.

It's best suitable for blogging editor for dotnet framework specially for blazor.

it's accept ReadOnly, Placeholder, Theme, Height and endpoint url to insert image etc.

Very simple to use add nuget package and use it.

In this version 2.0.9 has update ui and core functions improvements like image search, resize, align etc and also package size optimized.

## New Features in this version:

- It updated for .Net 9.0 and fixed UI
- Image resize can be done from top and bottom
- Compact resize toolbar
- Image drop enable
- Dropped image inserted into editor as base64 string
- Getter and Setter by quilljs Delta object

## How to add package into project

```
dotnet add package Blazor.Quill --version 2.0.9
```
## How to use it

```
@page "/article"
@rendermode InteractiveServer
@using Blazor.Quill


<div>
	@*GalleryUrl is endpoint of your server's image gallery to show image list, if api endpoint is not provided then url image link is still working*@
	<Editor @ref="@_quillNative" GalleryUrl="/api/images/gallery" />
</div>

<div>
	<button class="btn btn-primary" @onclick="SetHTMLAsync">Set HTML</button>
    <button class="btn btn-primary" @onclick="GetHTMLAsync">Get HTML</button>
	<button class="btn btn-primary" @onclick="SetContentAsync">Set Contents</button>
    <button class="btn btn-primary" @onclick="GetContentAsync">Get Contents</button>
</div>

<div>
	@_quillContent
</div>

@code {
	private string? _quillContent;
    private Editor? _quillNative;

    public async Task SetHTMLAsync()
    {
    	string img = "<p><img src='https://images.stockcake.com/public/5/0/2/502bbf64-d559-4efe-bba5-59ef7ffad5a8/toddler-baking-cookies-stockcake.jpg' /></p>";
    	await _quillNative!.SetHTMLAsync(img);
    	StateHasChanged();
    }
    public async Task GetHTMLAsync()
    {
    	_quillContent = await _quillNative!.GetHTMLAsync();
    	StateHasChanged();
    }
	public async Task SetContentAsync()
    {
    	var image = new Dictionary<string, object>()
    	{
    	    {
    	        "image", new ImageFile(){
    	            Src = "https://img.freepik.com/free-photo/woman-black-shirt-pointing-left-showing-her-emotions_144627-60740.jpg?w=1800&t=st=1726579146~exp=1726579746~hmac=3e36f98f85bdf588086750ae11e6d218f846b9f9e74ba0418bd1c92175ad09c3",
    	            Alt = "Image In Editor"
    	        }
    	    }
    	};

		var image2 = new Dictionary<string, object>()
    	{
    	    {
    	        "image", new ImageFile(){
    	            Src = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/...",
    	            Alt = "Image In Editor"
    	        }
    	    }
    	};

		var ops = new List<Operation>
        {
            new() { Attributes = new() { { "align", "center" } }, Insert = "Hello, World!\n" },
            new() { Insert = "This is first line\n", Attributes = new() { { "bold", true } } },
            new() { Insert = "This is second line\n" },
            new() { Insert= image},
            new() { Insert= "\n},
            new() { Insert= image1},
            new() { Insert = "\n" }
        };

        var delta = new Delta(ops);
        await _quillNative!.SetContentAsync(delta);
        StateHasChanged();
	}
	public async Task GetContentAsync()
    {
        _quillContent = await _quillNative!.GetContentAsync();
        StateHasChanged();
    }
}
```

### Editor Picture

![Editor](https://raw.githubusercontent.com/ray-vinod/Blazor.Editor/refs/heads/main/Images/editor.jpg)

![Image selection](https://raw.githubusercontent.com/ray-vinod/Blazor.Editor/refs/heads/main/Images/image-choosing.jpg)

![Inserted image](https://raw.githubusercontent.com/ray-vinod/Blazor.Editor/refs/heads/main/Images/image-in-editor.jpg)

![Resize and Format](https://raw.githubusercontent.com/ray-vinod/Blazor.Editor/refs/heads/main/Images/resize-options.jpg)

![Delta Object](https://raw.githubusercontent.com/ray-vinod/Blazor.Editor/refs/heads/main/Images/delta-object.jpg)