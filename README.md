# FaceApp.Net
A lightweight wrapper for the FaceApp API.

# Note
This library does not implement ratelimit to prevent you from getting blocked from the API, so I would be careful while using it. 

## How to use?
~~Don't~~
```cs
//Create a new instance of the FaceAppClient and pass an instance of HttpClient to its constructor (you generally want to only have one instance of the HttpClient).
//Note: Using Dependency Injection is recommended because it handles everything for you.

//_faceApp: FaceAppClient, url: string url
if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
{
    try
    {
        var code = await _faceApp.GetCodeAsync(uri);
        using (var imgStream = await _faceApp.ApplyFilterAsync(code, FilterType.Old))
        {
            //do stuff.
        }
    }
    catch (FaceException ex)
    {
        Console.WriteLine(ex.ToString());
    }            
}
```

## Credits
I used [Faces](https://github.com/vasilysinitsin/Faces), a python wrapper, to figure out the endpoint as it was not documented.

