# FaceApp.Net
A light-weight wrapper for face app API.

## How to use?
```cs
//Create a new instance of FaceAppClient and pass a instance of HttpClient. (you always want to have only one instance of HttpClient)
//Note: Using Depedency Injection is recommended because it handles everything fot you.

//_faceApp: FaceAppClient, url: string url
 if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
{
    try
    {
        var code = await _faceApp.GetCodeAsync(uri);
        using(var imgStream = await _faceApp.ApplyFilterAsync(code, FilterType.Old))
        {
            //do stuff.
        };
    }
    catch (FaceException ex)
    {
        Console.WriteLine(ex.ToString());
    }            
}
```

## Credits
I used [Faces](https://github.com/vasilysinitsin/Faces), a python wrapper, to figure out the endpoint as it was not documented.

