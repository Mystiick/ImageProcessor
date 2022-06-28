# ImageProcessor
Console application that is responsible for creating thumbnails and loading the data for [MystiickWeb](https://github.com/Mystiick/MystiickWeb). 

It processes all images in the source folder, creates a thumbnail image (max length/width 250px) and a preview image (max length/width 1920px). Additionally it will look for a file in each folder: `tags` to automatically apply a Category, Subcategory, and Tags for quickly classifying groups of pictures at once.  
Some additional data is automatically captured from the exif of the images.
 - Camera Model
 - Date taken
 - ISO
 - Shutter Speed
 - Aperature
 - Focal Length

## Running Locally
To run the application locally, you need to complete 2 steps:
 1. Have a mysql server setup with the schema applied.
    - To easily do this, you can run `.\quickstart-dev.sh` in [MystiickWeb's Quickstart](https://github.com/Mystiick/MystiickWeb/tree/main/quickstart/). This will run a Docker container with the schema applied to it. Alternatively you can run it yourself using the schema from `create-database.sql` in that same folder.
2. Configure `appsettings.json`, or `appsettings.dev.json`, with the source and archive folders.
    - The `SourceFolder` is the folder that will hold the unprocessed files. This can have subfolders of images to be processed, or just all of them in the root.
    - The `ArchiveFolder` is where the processed images and their thumbnail/previews will be moved to.

Once setup, you can run with `dotnet run` and it will process and files in the source folder.