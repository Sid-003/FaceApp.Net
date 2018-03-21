# FaceApp.Net
A light-weight wrapper for face app API.

## How to use?
```cs
//Create a new instance of the FaceAppClient and pass in the HttpClient to it's constructor. (Use Dependency Injection if possible.)
//Let's call our instance faceClient.
//Use Uri.TryParse() to parse the string url to Uri.
if(Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
{
    var bareUpResponse = await faceClient.GetCodeAsync(uri);
    //Do success checks and cast appropriately i.e FaceUploadResponse for success and FaceErrorResponse for error. 
    //I am just going to do the success part.
    if (bareUpResponse.IsSuccess)
    {
        var uploadResponse = bareUpResponse as FaceUploadResponse. //Always use soft-cast when possible.
        var bareApplyResponse = await faceClient.ApplyFilterAsync(uploadResponse.ImageCode, FilterType.Smile); //here is where you pick the filer.
        //Do success checks again and cast appopriately. Use FaceApplyResponse for success and FaceErrorResponse for error.
        //Note: For the apply endpoint, the API never provided a description so all you have is the error code (For ex: "no_photo_found").
        if (bareApplyResponse.IsSuccess)
        {
            var uploadResponse = bareApplyResponse as FaceApplyResponse;
            //Now you can access the ImageStream property, do whatever you want with it. 
            //Note: The image will probably be in jpeg format. (Not my fault, don't hit me.)
        }
    }
}
```

## Credits
I used [Faces](https://github.com/vasilysinitsin/Faces), a python wrapper, to figure out the endpoint as it was not documented.

